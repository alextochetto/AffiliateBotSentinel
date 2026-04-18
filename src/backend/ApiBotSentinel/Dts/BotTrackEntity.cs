using Azure;
using Azure.Data.Tables;

namespace ApiBotSentinel.Dts;

public class BotTrackEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;  // yyyy-MM-dd (UTC)
    public string RowKey { get; set; } = string.Empty;        // GUID per event
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsBot { get; set; }
    public string Path { get; set; } = string.Empty;
}
