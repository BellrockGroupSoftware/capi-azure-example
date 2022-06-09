using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bellrock.Capi.Models.CapiEvents
{
    [Serializable]
    public class CapiOrderEventData
    {
        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }
        [JsonPropertyName("orderRef")]
        public string OrderRef { get; set; } = string.Empty;
    }
}
