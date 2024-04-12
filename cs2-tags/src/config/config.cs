using CounterStrikeSharp.API.Core;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using static TagApi.Tag;

namespace Tag;

public class TagConfig : BasePluginConfig
{
    [JsonPropertyName("settings")]
    public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>
    {
        { "deadname", "☠" },
        { "nonename", "{White}(NONE)" },
        { "specname", "{Purple}(SPEC)" },
        { "tname", "{Yellow}(T)" },
        { "ctname", "{Blue}(CT)" }
    };

    [JsonPropertyName("tags")]
    public ConcurrentDictionary<string, CTag> Tags { get; set; } = new ConcurrentDictionary<string, CTag>
    {
        ["default"] = new CTag { ChatColor = "", ChatTag = "{Grey}[Player]", NameColor = "", ScoreTag = "" }
    };
}