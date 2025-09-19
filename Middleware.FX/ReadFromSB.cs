namespace Middleware.FX;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Middleware.FX.Models;
using System.Text.Json;

public class ReadFromSB
{
    private readonly SecretClient _secretClient;
    private readonly string _secretName;


    public ReadFromSB(IConfiguration configuration)
    {
        var keyVaultUri = configuration["KeyVaultUri"];
        _secretName = configuration["SqlSecretName"];
        _secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());

    }

    [Function(nameof(ReadFromSB))]
    public async Task Run(
        [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnection")]
             string message, FunctionContext context)
    {
        var logger = context.GetLogger("ReadFromQueue");

        try
        {
            var country = JsonSerializer.Deserialize<Country>(message);
            logger.LogInformation($"Mensaje recibido: {country?.Name}");

            var secret = await _secretClient.GetSecretAsync(_secretName);
            var connectionString = secret.Value.Value;

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("INSERT INTO Country (Id,Name,IsSynced,Origin) VALUES (@Id,@Name,@IsSynced,@Origin)", conn);
            cmd.Parameters.AddWithValue("@Id", country.Id);
            cmd.Parameters.AddWithValue("@Name", country.Name);
            cmd.Parameters.AddWithValue("@IsSynced", true);
            cmd.Parameters.AddWithValue("@Origin", "Service Bus");
            await cmd.ExecuteNonQueryAsync();

        }
        catch (Exception ex)
        {
            logger.LogError($"Error al procesar el mensaje: {ex.Message}");
        }
    }
}