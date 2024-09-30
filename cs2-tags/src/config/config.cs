using CounterStrikeSharp.API;
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
        public string DeadName { get; set; } = string.Empty;
        public string NoneName { get; set; } = string.Empty;
        public string SpecName { get; set; } = string.Empty;
        public string TName { get; set; } = string.Empty;
        public string CTName { get; set; } = string.Empty;
    }

    public class Cfg
    {
        public Settings Settings { get; set; } = new();
        public Tag DefaultTags { get; set; } = new();
        public Dictionary<string, Tag> Tags { get; set; } = [];
    }

    public static Cfg Config { get; set; } = new Cfg();
    private static string? ConfigPath;

    public static void Load()
    {
        if (ConfigPath == null)
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

            if (!File.Exists(ConfigPath))
            {
                throw new FileNotFoundException($"Configuration file not found: {ConfigPath}");
            }
        }

        LoadConfig(ConfigPath);
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
        var players = GetPlayers();

        foreach (var player in players)
        {
            player.LoadTag();
        }
    }

    private static void LoadConfig(string configPath)
    {
        string configText = File.ReadAllText(configPath);
        TomlTable model = Toml.ToModel(configText);

        TomlTable table = (TomlTable)model["Settings"];
        Config.Settings.DeadName = table["DeadName"].ToString()!;
        Config.Settings.NoneName = table["NoneName"].ToString()!;
        Config.Settings.SpecName = table["SpecName"].ToString()!;
        Config.Settings.TName = table["TName"].ToString()!;
        Config.Settings.CTName = table["CTName"].ToString()!;

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