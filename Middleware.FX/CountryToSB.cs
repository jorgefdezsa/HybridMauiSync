namespace Middleware.FX;

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Middleware.FX.Models;
using System.Net;
using System.Text.Json;

public class CountryToSB
{
    private readonly IConfiguration _config;
    public CountryToSB(IConfiguration config)
    {
        _config = config;
    }


    [Function("SendCountryToBus")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "send-country")] HttpRequestData req,
        FunctionContext context)
    {
        var logger = context.GetLogger("SendCountryToBus");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var country = JsonSerializer.Deserialize<Country>(requestBody);

        var namespaceFqdn = _config["ServiceBusNamespace"];
        var queueName = _config["QueueName"];

        var client = new ServiceBusClient(namespaceFqdn, new DefaultAzureCredential());
        var sender = client.CreateSender(queueName);

        try
        {

            var message = new ServiceBusMessage(JsonSerializer.Serialize(country))
            {
                ContentType = "application/json",
                Subject = "NewCountry"
            };

            await sender.SendMessageAsync(message);
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
        catch (Exception ex)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Error enviando el mensaje a Service Bus.");
            return errorResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Mensaje enviado correctamente a Service Bus.");
        return response;
    }

}