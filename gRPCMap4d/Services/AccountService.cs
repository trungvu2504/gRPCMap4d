using Grpc.Core;
using log4net;
using System.Net.Http.Headers;
using System.Text.Json;
using gRPCMap4d.Protos;
using gRPCMap4d.Utils;
using Microsoft.AspNetCore.Http;
using System.Text;
using gRPCMap4d.Models.KeyCloak;

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
    /// Get account keycloak by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<AccountReply?> GetAccountByIdAsync(string id)
    {
        string url = $"{appSetting.KeyCloak.KeyCloakEndpoint}/users/{id}";
        HttpClient httpClient = await MakeHttpClientWithTokenAsync();
        var apiResponse = await httpClient.GetAsync(url);
        if (apiResponse.IsSuccessStatusCode)
        {
            var content = await apiResponse.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<AccountReply>(content, Map4dApiShared.Utils.Commons.SerializerOptions);
            return user;
        }
        Metadata metadata = new()
            {
                { "error", $"error get account with id = {id}" }
            };
        log.Error(metadata.Select(s => s.Value));
        throw new RpcException(Commons.ErrorExceptionServiceStatus, metadata);
    }

    /// <summary>
    /// Xóa User From Key Cloak
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<bool> DeleteUserFromKeyCloakAsync(string id)
    {
        string url = $"{appSetting.KeyCloak.KeyCloakEndpoint}/users/{id}";
        HttpClient httpClient = await MakeHttpClientWithTokenAsync();
        var apiResponse = await httpClient.DeleteAsync(url);
        if (apiResponse.IsSuccessStatusCode)
        {
            return true;
        }
        Metadata metadata = new()
            {
                { "error", $"error get delete with id = {id}" }
            };
        log.Error(metadata.Select(s => s.Value));
        throw new RpcException(Commons.ErrorExceptionServiceStatus, metadata);
    }

    /// <summary>
    /// Update User Info Async
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task UpdateUserInfoAsync(UpdateAccountInfoRequest request)
    {
        UpdateAccountRequest updateUserRequest = new()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Attributes = new Attributes()
            {
                Province = new List<string> { request.Province },
                Phone = new List<string> { request.PhoneNumber }
            }
        };
        string jsonContent = JsonSerializer.Serialize(updateUserRequest);
        StringContent body = new(jsonContent, Encoding.UTF8, Constants.ApplicationJsonAccept);
        HttpClient httpClient = await MakeHttpClientWithTokenAsync();
        string url = $"{appSetting.KeyCloak.KeyCloakEndpoint}/users/{request.Id}";
        HttpResponseMessage apiResponse = await httpClient.PutAsync(url, body);
        if (!apiResponse.IsSuccessStatusCode)
        {
            Metadata metadata = new()
            {
                { "error", $"UpdateUserInfoAsync error response KeyCloak: {apiResponse.ReasonPhrase} with request body: {jsonContent}" }
            };
            log.Error(metadata.Select(s => s.Value));
        }
    }

    /// <summary>
    /// Check password correct
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private async Task<bool> IsPasswordCorrectAsync(string username, string password)
    {
        var httpClient = httpClientFactory.CreateClient(Constants.KeyCloakHttpClient);
        var uri = appSetting.KeyCloak.ClientEndpoint;
        IList<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>> {
                    { new KeyValuePair<string, string>("username", username) },
                    { new KeyValuePair<string, string>("password", password) },
                    { new KeyValuePair<string, string>("client_id", appSetting.KeyCloak.ClientId) },
                    { new KeyValuePair<string, string>("client_secret", appSetting.KeyCloak.ClientSecret) },
                    { new KeyValuePair<string, string>("grant_type", Constants.Password) }
                };

        var apiResponse = await httpClient.PostAsync(uri, new FormUrlEncodedContent(data));
        if (apiResponse.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            log.Error($"IsPasswordCorrect error password: {apiResponse.ReasonPhrase}");
            return false;
        }
    }

    /// <summary>
    /// Update User Password
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private async Task<bool> UpdatePasswordAsync(string id, string password)
    {
        ChangePasswordRequest changePasswordRequest = new()
        {
            Type = Constants.Password,
            Value = password,
            Temporary = false
        };
        string jsonString = JsonSerializer.Serialize(changePasswordRequest);
        var body = new StringContent(jsonString, Encoding.UTF8, Constants.ApplicationJsonAccept);
        var httpClient = await MakeHttpClientWithTokenAsync();
        string url = $"{appSetting.KeyCloak.KeyCloakEndpoint}/users/{id}/reset-password";
        var apiResponse = await httpClient.PutAsync(url, body);
        if (!apiResponse.IsSuccessStatusCode)
        {
            log.Error($"UpdatePassword error update password: {apiResponse.ReasonPhrase} with body {jsonString}");
        }
        return apiResponse.IsSuccessStatusCode;
    }

    /// <summary>
    /// Get accounts
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<AccountsReply> GetAccounts(GetAccountsRequest request, ServerCallContext context)
    {
        var rs = await GetListAccountAsync(request.Q, request.MaxResult);
        AccountsReply accountsReply = new AccountsReply();
        accountsReply.Accounts.Add(rs);
        return accountsReply;
    }

    /// <summary>
    /// Get account
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<AccountReply?> GetAccount(GetAccountRequest request, ServerCallContext context)
    {
        return await GetAccountByIdAsync(request.Id);
    }

    /// <summary>
    /// Delete account
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<DeleteAccountReply> DeleteAccount(GetAccountRequest request, ServerCallContext context)
    {
        DeleteAccountReply isSuccess = new();
        isSuccess.IsSuccess = await DeleteUserFromKeyCloakAsync(request.Id);
        return isSuccess;
    }

    /// <summary>
    /// Update account info
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<UpdateAccountReply> UpdateAccount(UpdateAccountInfoRequest request, ServerCallContext context)
    {
        await UpdateUserInfoAsync(request);
        UpdateAccountReply response = new() { IsSuccess = true };
        return response;
    }

    /// <summary>
    /// Check password user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<UpdateAccountReply> CheckPassword(CheckPasswordAccountRequest request, ServerCallContext context)
    {
        UpdateAccountReply response = new() { IsSuccess = await IsPasswordCorrectAsync(request.UserName, request.Password) };
        return response;
    }

    /// <summary>
    /// Change password account
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<UpdateAccountReply> ChangePassword(ChangePasswordAccountRequest request, ServerCallContext context)
    {
        UpdateAccountReply response = new() { IsSuccess = await UpdatePasswordAsync(request.Id, request.Password) };
        return response;
    }
}

