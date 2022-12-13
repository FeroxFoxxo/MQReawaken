using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;

namespace Web.Apps.Leaderboards.Services;

public class LeaderboardHandler : IService
{
    public LeaderBoardGameJson Games { get; private set; }

    private readonly EventSink _sink;

    public LeaderboardHandler(EventSink sink) => _sink = sink;

    public void Initialize() => _sink.WorldLoad += LoadLeaderboard;

    private void LoadLeaderboard() =>
        Games = new LeaderBoardGameJson
        {
            status = true,
            games = new List<LeaderBoardGameJson.Game>()
        };
}
