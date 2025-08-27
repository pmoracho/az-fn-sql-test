using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net;

namespace WeatherFunction;

public class WeatherSyncFunctions
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherSyncFunctions> _logger;
    private readonly IConfiguration _configuration;

    public WeatherSyncFunctions(HttpClient httpClient, ILogger<WeatherSyncFunctions> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    [Function("weather-sync")]
    public async Task RunTimerTrigger([TimerTrigger("0 */30 * * * *")] FunctionContext context)
    {
        _logger.LogInformation("[weather-sync] Timer trigger function executed.");
        await SyncWeatherDataAsync();
        _logger.LogInformation("[weather-sync] Weather sync completed.");
    }

    [Function("weather-sync-manual")]
    public async Task<HttpResponseData> RunHttpTrigger([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("[weather-sync-manual] HTTP trigger function called to manually start weather sync.");
        await SyncWeatherDataAsync();
        
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("[weather-sync-manual] Weather synchronization triggered successfully.");
        return response;
    }

    private async Task SyncWeatherDataAsync()
    {
        var apiKey = _configuration["OpenWeatherMapApiKey"];
        var connectionString = _configuration["SqlConnectionString"];
        var city = "Olivos,AR";

        try
        {
            var url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            var response = await _httpClient.GetStringAsync(url);
            var weatherData = JsonSerializer.Deserialize<WeatherApiResponse>(response);

            if (weatherData != null && connectionString != null)
            {
                await InsertWeatherDataToSqlAsync(connectionString, weatherData);
            }
            else
            {
                _logger.LogWarning("Weather data is null or connection string is not configured.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during weather sync: {ex.Message}");
        }
    }

    // Inserción de datos en la base de datos SQL
    private async Task InsertWeatherDataToSqlAsync(string connectionString, WeatherApiResponse data)
    {
        var query = @"INSERT INTO WeatherReadings (
                            Temp, FeelsLike, TempMin, TempMax, Pressure, Humidity,
                            Visibility, WindSpeed, WindDeg, Description, ReadingTime
                        )
                        VALUES (
                            @Temp, @FeelsLike, @TempMin, @TempMax, @Pressure, @Humidity,
                            @Visibility, @WindSpeed, @WindDeg, @Description, GETDATE()
                        )";

        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Temp", data.Main.Temp);
                    command.Parameters.AddWithValue("@FeelsLike", data.Main.FeelsLike);
                    command.Parameters.AddWithValue("@TempMin", data.Main.TempMin);
                    command.Parameters.AddWithValue("@TempMax", data.Main.TempMax);
                    command.Parameters.AddWithValue("@Pressure", data.Main.Pressure);
                    command.Parameters.AddWithValue("@Humidity", data.Main.Humidity);
                    command.Parameters.AddWithValue("@Visibility", data.Visibility);
                    command.Parameters.AddWithValue("@WindSpeed", data.Wind.Speed);
                    command.Parameters.AddWithValue("@WindDeg", data.Wind.Deg);
                    command.Parameters.AddWithValue("@Description", data.Weather[0].Description);
                    await command.ExecuteNonQueryAsync();
                }
            }
            _logger.LogInformation("Weather data successfully inserted into SQL.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while inserting data into SQL: {ex.Message}");
        }
    }
}
