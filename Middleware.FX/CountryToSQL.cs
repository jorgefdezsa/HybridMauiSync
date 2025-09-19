namespace Middleware.FX;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Middleware.FX.Models;
using System.Net;
using System.Text.Json;

public class CountryToSQL
{
    private readonly SecretClient _secretClient;
    private readonly string _secretName;


    public CountryToSQL(IConfiguration configuration)
    {
        var keyVaultUri = configuration["KeyVaultUri"];
        _secretName = configuration["SqlSecretName"];
        _secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());

    }

    [Function("AddCountry")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "countries")] HttpRequestData req,
        FunctionContext context)
    {
        var logger = context.GetLogger("AddCountry");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var country = JsonSerializer.Deserialize<Country>(requestBody);

        var secret = await _secretClient.GetSecretAsync(_secretName);
        var connectionString = secret.Value.Value;

        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand("INSERT INTO Country (Id,Name,IsSynced,Origin) VALUES (@Id,@Name,@IsSynced,@Origin)", conn);
        cmd.Parameters.AddWithValue("@Id", country.Id);
        cmd.Parameters.AddWithValue("@Name", country.Name);
        cmd.Parameters.AddWithValue("@IsSynced", true);
        cmd.Parameters.AddWithValue("@Origin", "Azure SQL");
        await cmd.ExecuteNonQueryAsync();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("País insertado correctamente.");
        return response;

    }
}