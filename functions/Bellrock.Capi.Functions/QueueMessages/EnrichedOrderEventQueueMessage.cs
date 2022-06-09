using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bellrock.Capi.Functions.QueueMessages
{
    public class EnrichedOrderEventQueueMessage
    {
        public Guid OrderId { get; set; }
        public string OrderRef { get; set; }
        public string EventType { get; set; }
        public DateTime EventDateTimeUtc { get; set; }
        public DateTime OrderDateTimeUtc { get; set; }
        public string OrderType { get; set; }

        public string OrderStatus { get; set; }
        public string OrderDescription { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}
