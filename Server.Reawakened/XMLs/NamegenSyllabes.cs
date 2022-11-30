using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Reawakened.Data.Services;

namespace Server.Reawakened.XMLs;

public class NameGenSyllables : IService
{
    private readonly Random _random;
    private readonly EventSink _sink;
    private readonly ILogger<NameGenSyllables> _logger;

    private Dictionary<bool, List<List<string>>> _syllables;

    public NameGenSyllables(Random random, EventSink sink, ILogger<NameGenSyllables> logger)
    {
        _random = random;
        _sink = sink;
        _logger = logger;
        _syllables = new Dictionary<bool, List<List<string>>>();
    }

    public void Initialize() => _sink.WorldLoad += CreateSyllables;

    private void CreateSyllables()
    {
        var syllablesXml = new NamegenSyllablesXML();

        try
        {
            syllablesXml.ReadDescriptionXml(File.ReadAllText("XMLs/NamegenSyllabes.xml"));
        }
        catch
        {
            _logger.LogWarning("{Name} could not load! Skipping...", GetType().Name);
        }

        _syllables = new Dictionary<bool, List<List<string>>>
        {
            {
                false,
                new List<List<string>>
                {
                    syllablesXml.GetSyllables(0, false),
                    syllablesXml.GetSyllables(1, false),
                    syllablesXml.GetSyllables(2, false)
                }
            },
            {
                true,
                new List<List<string>>
                {
                    syllablesXml.GetSyllables(0, true),
                    syllablesXml.GetSyllables(1, true),
                    syllablesXml.GetSyllables(2, true)
                }
            }
        };
    }

    private string GetRandomFromList(IReadOnlyList<string> names) =>
        names[_random.Next(names.Count)];

    public static bool IsNameReserved(string[] names, UserInfoHandler handler)
    {
        var name = $"{names[0]}{names[1]}{names[2]}";
        return handler.Data.Select(a => a.Value.Characters)
            .SelectMany(cl => cl).Any(c => c.Name == name);
    }

    public bool IsPossible(bool isMale, string[] names) =>
        _syllables[isMale][0].Contains(names[0]) &&
        _syllables[isMale][1].Contains(names[1]) &&
        _syllables[isMale][2].Contains(names[2]);

    public string[] GetRandomName(bool isMale, UserInfoHandler handler)
    {
        while (true)
        {
            var names = new[]
            {
                GetRandomFromList(_syllables[isMale][0]),
                GetRandomFromList(_syllables[isMale][1]),
                GetRandomFromList(_syllables[isMale][2])
            };

            if (IsNameReserved(names, handler))
                continue;

            return names;
        }
    }
}
