using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using static TagsApi.Tags;

namespace Tags;

public class Config : BasePluginConfig
{
    public Settings Settings { get; set; } = new();
    public Commands Commands { get; set; } = new();
    public Tag Default { get; set; } = new();
    public List<Tag> Tags { get; set; } = [];
}

public class Settings
{
    public string Tag { get; set; } = string.Empty;
    public string DeadName { get; set; } = string.Empty;
    public string NoneName { get; set; } = string.Empty;
    public string SpecName { get; set; } = string.Empty;
    public string TName { get; set; } = string.Empty;
    public string CTName { get; set; } = string.Empty;
    public Dictionary<CsTeam, string> TeamNames = [];

    public void Init()
    {
        Tag = Tag.ReplaceColorTags();
        TeamNames[CsTeam.None] = NoneName;
        TeamNames[CsTeam.Spectator] = SpecName;
        TeamNames[CsTeam.Terrorist] = TName;
        TeamNames[CsTeam.CounterTerrorist] = CTName;
    }
}

public class Commands
{
    public string[] TagsReload { get; set; } = [];
    public string[] Visibility { get; set; } = [];
}