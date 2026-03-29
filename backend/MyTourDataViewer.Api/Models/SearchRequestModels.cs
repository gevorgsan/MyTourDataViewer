using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MyTourDataViewer.Api.Models;

/// <summary>Request payload forwarded to the external SearchRequest endpoint.</summary>
public class SearchRequestDto
{
    [Required]
    public DateTime CreatedFrom { get; set; }

    [Required]
    public DateTime CreatedTo { get; set; }

    /// <remarks>Intentional spelling: the external API payload uses 'requestChanels' (single 'n').</remarks>
    public int? RequestChanels { get; set; }

    public int? RequestStatus { get; set; }
}

/// <summary>Raw item returned by the external GetRequestHistory endpoint.</summary>
public class RequestHistoryItem
{
    [JsonPropertyName("requestId")]
    public int RequestId { get; set; }

    [JsonPropertyName("changeType")]
    public string? ChangeType { get; set; }

    [JsonPropertyName("changedAt")]
    public DateTime ChangedAt { get; set; }

    [JsonPropertyName("changedBy")]
    public string? ChangedBy { get; set; }

    [JsonPropertyName("oldValuesJson")]
    public string? OldValuesJson { get; set; }

    [JsonPropertyName("newValuesJson")]
    public string? NewValuesJson { get; set; }
}

/// <summary>Processed history item with parsed old/new values returned to the frontend.</summary>
public class RequestHistoryItemDto
{
    public int RequestId { get; set; }
    public string? ChangeType { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public Dictionary<string, object?>? OldValues { get; set; }
    public Dictionary<string, object?>? NewValues { get; set; }
}
/// <summary>A single item returned by the external SearchRequest endpoint.</summary>
public class SearchRequestItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("offerId")]
    public int? OfferId { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("travelers")]
    public IList<object>? Travelers { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime? CreatedDate { get; set; }
}
