using Serilog;
using TestHQ.Core.Configurations;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Host);
Configure(builder.Build());

void ConfigureServices(IServiceCollection services, IHostBuilder host)
{
    services.AddControllers();
    services.Configure<BitfinexConfiguration>(builder.Configuration.GetSection(nameof(BitfinexConfiguration)));
    
    host.UseSerilog((context, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    });
}

void Configure(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.MapControllers();

    app.Run();
}