using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Bellrock.Capi.CapiConnector.Api;
using Microsoft.Azure.WebJobs.ServiceBus;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using Bellrock.Capi.Models.CapiEvents;
using Bellrock.Capi.Functions.QueueMessages;

namespace Bellrock.Capi.Functions
{
    public class PendingOrderMessagesQueueTrigger
    {
        private readonly ICapiService capiService;
        private readonly ILogger<PendingOrderMessagesQueueTrigger> log;

        public PendingOrderMessagesQueueTrigger(ICapiService capiService, ILogger<PendingOrderMessagesQueueTrigger> log)
        {
            this.capiService = capiService;
            this.log = log;
        }

        [FunctionName("PendingOrderMessagesQueueTrigger")]
        public async Task Run(
            [ServiceBusTrigger("sbq-pending-order-messages", Connection = "sas-capi-read-write")] 
            string message,

            [ServiceBus("sbt-distribute-channel-events", ServiceBusEntityType.Topic, Connection = "sas-capi-read-write")] 
            IAsyncCollector<ServiceBusMessage> outputQueue)
        {

            log.LogInformation("Pending order message recieved, processing...");

            // Deserialize the service bus message content into an order object
            var orderEvent = JsonSerializer.Deserialize<CapiGenericMessage<CapiOrderEventData>>(message);

            // Using the order ID from the webhook payload, get the full order details from CAPI
            var order = await capiService.GetOrderAsync(orderEvent.Data.OrderId);

            log.LogInformation("Got order information from Order API. Creating enriched message...");

            // Create the enriched order event using data from both the webhook event and the order API
            var enrichedOrderEvent = new EnrichedOrderEventQueueMessage()
            {
                OrderId = order.Id,
                OrderRef = order.Ref,
                EventType = orderEvent.Type.ToString(),
                EventDateTimeUtc = orderEvent.EventDateUtc,
                OrderDateTimeUtc = DateTime.Parse(order.Date),
                OrderType= order.Type,
                OrderStatus = order.Status,
                OrderDescription = order.Description,
                CompletionDate = DateTime.Parse(order.CompletionDate)
            };

            // Create the service bus message
            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(enrichedOrderEvent))
            {
                ContentType = "application/json"
            };

            serviceBusMessage.ApplicationProperties.Add("channel", orderEvent.Channel);

            // Add the message to the output queue
            await outputQueue.AddAsync(serviceBusMessage);

            log.LogInformation("Enriched message added to output queue...");
        }
    }
}
