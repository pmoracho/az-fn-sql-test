using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddHttpClient()
    .AddTransient<SqlConnection>(sp => {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetValue<string>("SqlConnectionString");
        return new SqlConnection(connectionString);
    });

builder.Build().Run();

// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Data.SqlClient;
// using Azure.Identity;

// var host = new HostBuilder()
//     .ConfigureFunctionsWorkerDefaults()
//     .ConfigureServices((hostContext, services) =>
//     {
//         // ... otros servicios ...
        
//         // 1. Registrar HttpClient para la inyección de dependencias
//         services.AddHttpClient();

//         // 2. Registrar el SqlConnection con el inyector de dependencias
//         var configuration = hostContext.Configuration;
//         var connectionString = configuration.GetConnectionString("SqlConnectionString");
        
//         services.AddScoped(s => {
//             var conn = new SqlConnection(connectionString);
            
//             if (string.IsNullOrEmpty(conn.ConnectionString) || !conn.ConnectionString.Contains("Authentication", StringComparison.OrdinalIgnoreCase))
//             {
//                 var credential = new DefaultAzureCredential();
//                 var accessToken = credential.GetToken(
//                     new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
//                 conn.AccessToken = accessToken.Token;
//             }
//             return conn;
//         });

//     })
//     .Build();

// host.Run();