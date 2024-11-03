using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;
using static Tags.Tags;
using static Tags.TagsLibrary;
using static TagsApi.Tags;

namespace Tags;

public static class Config_Config
{
    public class Settings
    {
        public string Tag { get; set; } = string.Empty;
        public string DeadName { get; set; } = string.Empty;
        public Dictionary<CsTeam, string> TeamNames { get; set; } = [];
    }

    public class Cfg
    {
        public Settings Settings { get; set; } = new();
        public Tag DefaultTags { get; set; } = new();
        public Dictionary<string, Tag> Tags { get; set; } = [];
    }

    public static Cfg Config { get; set; } = new();
    private static readonly string ConfigPath;

    static Config_Config()
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

        ConfigPath = Path.Combine(Server.GameDirectory,
            "csgo",
            "addons",
            "counterstrikesharp",
            "configs",
            "plugins",
            assemblyName,
            "config.toml"
        );
    }

    public static void Reload()
    {
        Config.Tags.Clear();
        PlayerTagsList.Clear();

        Load();
        LoadPlayersTag();
    }

    public static void LoadPlayersTag()
    {
        GetPlayers(out HashSet<CCSPlayerController> players);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController player in players)
        {
            player.LoadTag();
        }
    }

    public static void Load()
    {
        if (!File.Exists(ConfigPath))
        {
            throw new FileNotFoundException($"Configuration file not found: {ConfigPath}");
        }

        string configText = File.ReadAllText(ConfigPath);
        TomlTable model = Toml.ToModel(configText);

        TomlTable table = (TomlTable)model["Settings"];
        Config.Settings.Tag = table["Tag"].ToString()!.ReplaceColorTags();

        Config.Settings.DeadName = table["DeadName"].ToString()!;
        Config.Settings.TeamNames[CsTeam.None] = TagsLibrary.ReplaceTags(table["NoneName"]?.ToString()!, CsTeam.None);
        Config.Settings.TeamNames[CsTeam.Spectator] = TagsLibrary.ReplaceTags(table["SpecName"]?.ToString()!, CsTeam.Spectator);
        Config.Settings.TeamNames[CsTeam.Terrorist] = TagsLibrary.ReplaceTags(table["TName"]?.ToString()!, CsTeam.Terrorist);
        Config.Settings.TeamNames[CsTeam.CounterTerrorist] = TagsLibrary.ReplaceTags(table["CTName"]?.ToString()!, CsTeam.CounterTerrorist);

        table = (TomlTable)model["Default"];
        Config.DefaultTags.ScoreTag = table["ScoreTag"].ToString()!;
        Config.DefaultTags.ChatTag = table["ChatTag"].ToString()!;
        Config.DefaultTags.ChatColor = table["ChatColor"].ToString()!;
        Config.DefaultTags.NameColor = table["NameColor"].ToString()!;
        Config.DefaultTags.ChatSound = bool.Parse(table["ChatSound"].ToString()!);

        foreach (KeyValuePair<string, object> tags in model)
        {
            string key = tags.Key;

            if (key is "Settings" or "Default")
            {
                continue;
            }

            table = (TomlTable)tags.Value;

            Tag configTag = new();

            table.SetIfPresent("ScoreTag", (string stag) => configTag.ScoreTag = stag);
            table.SetIfPresent("ChatTag", (string ctag) => configTag.ChatTag = ctag);
            table.SetIfPresent("ChatColor", (string ccolor) => configTag.ChatColor = ccolor);
            table.SetIfPresent("NameColor", (string ncolor) => configTag.NameColor = ncolor);
            table.SetIfPresent("ChatSound", (bool csound) => configTag.ChatSound = csound);

            Config.Tags.Add(key, configTag);
        }
    }
}