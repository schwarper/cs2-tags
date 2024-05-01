using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;
using static TagsApi.Tags;

namespace Tags;

public class TagsConfig : BasePluginConfig
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
    public Dictionary<string, Tag> Tags { get; set; } = new Dictionary<string, Tag>
    {
        ["default"] = new Tag { ChatColor = "{White}", ChatTag = "{Grey}[Player]", NameColor = "{TeamColor}", ScoreTag = string.Empty }
    };
}