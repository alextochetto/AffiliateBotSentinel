using ApiBotSentinel.Dts;
using Azure;
using Azure.Data.Tables;

namespace ApiBotSentinel.Services;

public class TrackBotService : ITrackBotService
{
    private readonly TableClient _tableClient;
    private readonly ILogger<TrackBotService> _logger;

    public TrackBotService(TableClient tableClient, ILogger<TrackBotService> logger)
    {
        _tableClient = tableClient;
        _logger = logger;
    }

    public async Task TrackAsync(TrackBotPostDtq query)
    {
        BotTrackEntity entity = new BotTrackEntity
        {
            PartitionKey = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
            RowKey = Guid.NewGuid().ToString(),
            Ip = query.Ip ?? string.Empty,
            UserAgent = query.UserAgent ?? string.Empty,
            IsBot = query.IsBot,
            Path = query.Path ?? string.Empty,
            Gclid = query.Gclid ?? string.Empty,
            Timestamp = DateTimeOffset.UtcNow
        };

        try
        {
            await _tableClient.AddEntityAsync(entity);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to write bot track entity to Azure Table Storage.");
            throw;
        }
    }
}
