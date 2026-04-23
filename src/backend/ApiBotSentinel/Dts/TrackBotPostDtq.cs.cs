namespace ApiBotSentinel.Dts;

public class TrackBotPostDtq
{
    public string Ip { get; set; }
    public string UserAgent { get; set; }
    public bool IsBot { get; set; }
    public string Path { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Gclid { get; set; }
}