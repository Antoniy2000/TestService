using Microsoft.AspNetCore.Mvc.Formatters;
using TestService.Core.Interfaces;
using TestService.Core.Managers;
using TestService.Core.Services;

namespace TestService;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        var manager = app.Services.CreateScope().ServiceProvider.GetRequiredService<IConverterManager>();
        manager.ReInitialize();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(cors =>
            {
                cors.AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin();
            });
        }
        app.UseRouting();
        app.UseEndpoints(x =>
        {
            x.MapControllers();
        });
        if (app.Environment.IsDevelopment())
        {
            app.UseSpa(spa =>
            {
                var spaDevelopmentServerUrl = builder.Configuration["SpaDevelopmentServerUrl"];
                if (!string.IsNullOrEmpty(spaDevelopmentServerUrl))
                {
                    spa.UseProxyToSpaDevelopmentServer(spaDevelopmentServerUrl);
                };
            });
        }

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddScoped<IConverterManager, DiskMemoryConverterManager>(x =>
        {
            var worker = x.GetRequiredService<IConverterWorkerService>();
            return new(configuration["CachePath"] ?? Path.Combine(Environment.CurrentDirectory, "Cache"), worker);
        });
        services.AddSingleton<IConverterWorkerService, ConverterWorkerService>(x =>
        {
            var threadsCount = int.TryParse(configuration["ThreadsCount"], out var result) ? result : 10;
            return new(threadsCount);
        });
    }
}