using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace WeatherFunction;

public class VersionFunctions
{
    private readonly ILogger<VersionFunctions> _logger;

    public VersionFunctions(ILogger<VersionFunctions> logger)
    {
        _logger = logger;
    }

    [Function("version")]
    public IActionResult GetVersion(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function to get assembly version.");

        // Obtener el assembly actual (el que contiene esta clase)
        var assembly = Assembly.GetExecutingAssembly();
        
        // Obtener el nombre y la versión del assembly
        var assemblyName = assembly.GetName().Name;
        var assemblyVersion = assembly.GetName().Version;
        
        var response = new
        {
            AssemblyName = assemblyName,
            AssemblyVersion = assemblyVersion!.ToString() ?? "Unknown"
        };

        return new OkObjectResult(response);
    }
}