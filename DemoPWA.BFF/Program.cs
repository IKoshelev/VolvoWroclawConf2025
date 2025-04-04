using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();


builder.Services.AddHttpClient("API", client =>
{
#if DEBUG
    var baseUrl = "http://localhost:7011/api/";
#else
                var baseUrl = "https://volvo-wroclaw-conf-2025-api.azurewebsites.net/api/";
#endif

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
});

builder.Build().Run();
