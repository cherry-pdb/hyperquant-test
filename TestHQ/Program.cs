using Microsoft.OpenApi.Models;
using Serilog;
using TestHQ.Core.Clients;
using TestHQ.Core.Configurations;
using TestHQ.Core.Connector;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Host);
Configure(builder.Build());

void ConfigureServices(IServiceCollection services, IHostBuilder host)
{
    services.AddControllers();
    services.Configure<BitfinexConfiguration>(builder.Configuration.GetSection(nameof(BitfinexConfiguration)));
    services.AddHttpClient<BitfinexRestClient>();
    services.AddSingleton<BitfinexWebSocketClient>();
    services.AddSingleton<ITestConnector, BitfinexConnector>();
    
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "TestHQ", Version = "v1" });
    });
    
    host.UseSerilog((context, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    });
}

void Configure(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.MapControllers();

    app.Run();
}