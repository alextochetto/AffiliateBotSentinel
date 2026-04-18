using ApiBotSentinel.Dts;

namespace ApiBotSentinel.Services;

public interface ITrackBotService
{
    Task TrackAsync(TrackBotPostDtq query);
}
