using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Bellrock.Capi.Models.CapiEvents;

[Serializable]
public class CapiGenericMessage<T> where T : notnull
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("eventDateUtc")]
    public DateTime EventDateUtc { get; set; }
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;
    [JsonPropertyName("type")]
    public CapiEventType Type { get; set; }
    [JsonPropertyName("data")]
    public T Data { get; set; }
}