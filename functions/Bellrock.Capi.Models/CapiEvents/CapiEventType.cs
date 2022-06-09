using System.Text.Json.Serialization;

namespace Bellrock.Capi.Models.CapiEvents;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CapiEventType
{
    OrderCreated,
    OrderAssigned,
    OrderAccepted,
    OrderRejected,
    OrderAttended,
    OrderCompleted,
    OrderCancelled,
    OrderValueChanged,
    OrderUpdated,
    TestEvent
}