using Achievement.Categories;
using Achievement.CharacterTitles;
using Achievement.StaticData;
using Achievement.Types;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;

namespace Web.Apps.Achievements.Services;

public class AchievementHandler : IService
{
    public AchievementStaticJson.AchievementDefinition Definitions { get; private set; }

    private readonly EventSink _sink;

    public AchievementHandler(EventSink sink) => _sink = sink;

    public void Initialize() => _sink.WorldLoad += LoadAchievements;

    private void LoadAchievements() =>
        Definitions = new AchievementStaticJson.AchievementDefinition
        {
            status = true,
            achievements = new List<AchievementStaticData>(),
            categories = new List<AchievementCategoryType>(),
            characterTitles = new List<AchievementCharacterTitlesType>(),
            types = new AchievementType
            {
                conditions = new List<AchievementConditionType>(),
                rewards = new List<AchievementRewardType>(),
                timeWindows = new List<AchievementTimeWindowType>()
            }
        };
}
