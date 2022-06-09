using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Bellrock.Capi.Helpers;
using Azure.Storage.Queues.Models;
using System.Text.Json;
using Microsoft.Azure.WebJobs.ServiceBus;
using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json.Nodes;

namespace Bellrock.Capi.Functions
{
    public class CapiWebhookHttpTrigger
    {
        private readonly string secret;
        public CapiWebhookHttpTrigger(IConfiguration configuration)
        {
            this.secret = configuration.GetValue<string>("capi-webhook-secret");
        }

        [FunctionName("CapiWebhookHttpTrigger")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "capi/webhook")]
            HttpRequest req,

            [ServiceBus("sbt-distribute-webhook-events", ServiceBusEntityType.Topic, Connection = "sas-capi-read-write")]
            IAsyncCollector<ServiceBusMessage> distributeWebhookEvents,

            ILogger log)
        {
            if (!await ValidateCapiSignature.IsValid(req, this.secret, log))
            {
                log.LogWarning("CAPI webhook request failed signature validation.");
            }

            // Reset position of request body
            req.Body.Position = 0;

            var jsonEvent = JsonNode.Parse(req.Body);
            var eventType = jsonEvent["type"];

            var sensorMessage = new ServiceBusMessage(jsonEvent.ToJsonString())
            {
                ContentType = "application/json"
            };

            sensorMessage.ApplicationProperties.Add("eventType", eventType.GetValue<string>());

            // Add to the distribution queue
            await distributeWebhookEvents.AddAsync(sensorMessage);

            return new OkResult();
        }
    }
}
