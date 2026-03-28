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
