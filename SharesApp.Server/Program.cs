using Microsoft.AspNetCore.Builder;
using OpenQA.Selenium.Chrome;
using SharesApp.Server.Classes;
using SharesApp.Server.Models;
using SharesApp.Server.Services;
using SharesApp.Server.Tools;
using Stock_Analysis_Web_App.Classes;
using Stock_Analysis_Web_App.Controllers;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<MoexHttpClient>();
        builder.Services.AddSingleton<BankiRuHttpClient>();
        builder.Services.AddSingleton<ChromeDriverFactory>();
        builder.Services.AddTransient<ChromeDriver>(provider =>
        { 
            var factory = new ChromeDriverFactory();
            return factory.CreateDriver();
        });
        builder.Services.AddTransient<SecuritiesContext>();
        builder.Services.AddHostedService<FetchingDataService>();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


            app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}