using gRPCMap4d;
using gRPCMap4d.Services;
using log4net;

ILog log = LogManager.GetLogger(typeof(Program));

try
{
    var builder = WebApplication.CreateBuilder(args);
    log.Info("Service has started");
    Startup startup = new(builder.Environment);

    startup.ConfigureServices(builder.Services);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.MapGrpcService<AccountService>();
    app.MapGrpcService<RouteService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    log.Info("Configure service done");
    app.Run();
}
catch (Exception ex)
{
    log.Error(ex.Message);
}

