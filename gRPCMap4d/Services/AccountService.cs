using Grpc.Core;
using log4net;
using System.Net.Http.Headers;
using System.Text.Json;
using gRPCMap4d.Protos;
using gRPCMap4d.Utils;

namespace gRPCMap4d.Services;
public class AccountService : Account.AccountBase
{

    /// <summary>
    /// Log Manager
    /// </summary>
    private static readonly ILog log = LogManager.GetLogger(typeof(AccountService));

    /// <summary>
    /// IHttpClientFactory service
    /// </summary>
    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// Appsetting 
    /// </summary>
    private readonly AppSetting appSetting;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClientFactory"></param>
    public AccountService(IHttpClientFactory httpClientFactory, AppSetting appSetting)
    {
        this.httpClientFactory = httpClientFactory;
        this.appSetting = appSetting;
    }

    /// <summary>
    /// MakeHttpClientWithToken for keyCloack calling
    /// </summary>
    /// <returns></returns>
    private async Task<string?> GetTokenFromMasterAsync()
    {
        var httpClient = httpClientFactory.CreateClient(Constants.KeyCloakHttpClient);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var uri = appSetting.KeyCloak.MasterEndpoint;
        IList<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>> {
                    { new KeyValuePair<string, string>("username", appSetting.KeyCloak.UserName) },
                    { new KeyValuePair<string, string>("password", appSetting.KeyCloak.PassWord) },
                    { new KeyValuePair<string, string>("client_id", appSetting.KeyCloak.ClientIdMaster) },
                    { new KeyValuePair<string, string>("grant_type", "password") }
                };

        var apiResponse = await httpClient.PostAsync(uri, new FormUrlEncodedContent(data));
        if (apiResponse.IsSuccessStatusCode)
        {
            var content = await apiResponse.Content.ReadAsStringAsync();
            var rs = JsonSerializer.Deserialize<Models.KeyCloak.TokenResponse>(content, Commons.SerializerOptions);
            return rs?.AccessToken;
        }
        return string.Empty;
    }

    /// <summary>
    /// MakeHttpClientWithToken for keyCloack calling
    /// </summary>
    /// <returns></returns>
    private async Task<HttpClient> MakeHttpClientWithTokenAsync()
    {
        var httpClient = httpClientFactory.CreateClient(Constants.KeyCloakHttpClient);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        string? token = await GetTokenFromMasterAsync();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }

    /// <summary>
    /// Get List Account With Max Number Of Account 
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    private async Task<IList<AccountReply>?> GetListAccountAsync(string q, int? maxResult)
    {
        maxResult ??= appSetting.KeyCloak.MaxResultRequest;

        string url = $"{appSetting.KeyCloak.KeyCloakEndpoint}/users?max={maxResult}&search={q}&realm={appSetting.KeyCloak.Realm}";
        HttpClient httpClient = await MakeHttpClientWithTokenAsync();
        var apiResponse = await httpClient.GetAsync(url);
        if (apiResponse.IsSuccessStatusCode)
        {
            var content = await apiResponse.Content.ReadAsStringAsync();
            var listUsers = JsonSerializer.Deserialize<IList<AccountReply>>(content, Commons.SerializerOptions);
            return listUsers;
        }
        else
        {

            Metadata metadata = new()
            {
                { "error", $"error get account with q={q} and maxResult ={maxResult}" }
            };
            log.Error(metadata.Select(s => s.Value));
            throw new RpcException(Commons.ErrorExceptionServiceStatus, metadata);
        }
    }

    /// <summary>
    /// Get accounts
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<AccountsReply> GetAccounts(AccountRequest request, ServerCallContext context)
    {
        var rs = await GetListAccountAsync(request.Q, request.MaxResult);
        AccountsReply accountsReply = new AccountsReply();
        accountsReply.Accounts.Add(rs);
        return accountsReply;
    }
}

