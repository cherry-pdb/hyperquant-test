using Microsoft.OpenApi.Models;
using Serilog;
using TestHQ.Core.Configurations;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Host);
Configure(builder.Build());

void ConfigureServices(IServiceCollection services, IHostBuilder host)
{
    services.AddControllers();
    services.Configure<BitfinexConfiguration>(builder.Configuration.GetSection(nameof(BitfinexConfiguration)));
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
    if (!app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("v1/swagger.json", "SilverScreenSyndicate v1"));
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.MapControllers();

    app.Run();
}