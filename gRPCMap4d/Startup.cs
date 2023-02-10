using gRPCMap4d.Utils;
using log4net;
using log4net.Config;
using System.Net;
using System.Reflection;

namespace gRPCMap4d
{
    /// <summary>
    /// Startup app configure services
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configuration
        /// </summary>
        public IConfigurationRoot configuration { get; }

        /// <summary>
        /// App setting
        /// </summary>
        private AppSetting? appSetting { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                //.AddJsonFile($"appsettings.{appSettingString}.json", optional: true)
                .AddEnvironmentVariables("APPSETTING_");
            var path = Path.Combine(env.ContentRootPath, "logger.xml");
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()), new FileInfo(path));
            builder.AddEnvironmentVariables();
            configuration = builder.Build();
        }

        /// <summary>
        /// Configure include service
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSetting>(appSettingsSection);
            appSetting = appSettingsSection.Get<AppSetting>();
            services.AddSingleton(appSetting);

            services.AddHttpClient(Constants.KeyCloakHttpClient)
                    .ConfigurePrimaryHttpMessageHandler(x => new SocketsHttpHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        Proxy = null,
                        UseProxy = false
                    });
            services.AddGrpc();
        }
    }
}
