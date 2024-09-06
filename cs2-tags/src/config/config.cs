using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static class Config_Config
{
    public static Cfg Config { get; set; } = new Cfg();

    public static void Load()
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
        string cfgPath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/{assemblyName}";

        LoadConfig($"{cfgPath}/config.toml");
    }

    public static void Reload()
    {
        Config.Tags.Clear();
        PlayerDataList.Clear();

        Load();
        LoadPlayersTag();
    }

    public static void LoadPlayersTag()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();

        foreach (CCSPlayerController player in players)
        {
            if (player.IsBot)
            {
                continue;
            }

            PlayerDataList.Add(player.SteamID, new PlayerData
            {
                PlayerTag = player.GetTag(),
                ToggleTags = true
            });
        }
    }

    private static void LoadConfig(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file not found: {configPath}");
        }

        string configText = File.ReadAllText(configPath);
        TomlTable model = Toml.ToModel(configText);

        TomlTable settingsTable = (TomlTable)model["Settings"];
        Config.Settings.DeadName = settingsTable["DeadName"].ToString()!;
        Config.Settings.NoneName = settingsTable["NoneName"].ToString()!;
        Config.Settings.SpecName = settingsTable["SpecName"].ToString()!;
        Config.Settings.TName = settingsTable["TName"].ToString()!;
        Config.Settings.CTName = settingsTable["CTName"].ToString()!;

        TomlTable defaultTable = (TomlTable)model["Default"];
        Config.DefaultTags.ScoreTag = defaultTable["ScoreTag"].ToString()!;
        Config.DefaultTags.ChatTag = defaultTable["ChatTag"].ToString()!;
        Config.DefaultTags.ChatColor = defaultTable["ChatColor"].ToString()!;
        Config.DefaultTags.NameColor = defaultTable["NameColor"].ToString()!;
        Config.DefaultTags.ChatSound = bool.Parse(defaultTable["ChatSound"].ToString()!);

        foreach (KeyValuePair<string, object> tags in model)
        {
            string key = tags.Key;

            if (key is "Settings" or "Default")
            {
                continue;
            }

            TomlTable value = (TomlTable)tags.Value;

            Config.Tags.Add(key, new Tag());

            if (value.TryGetValue("ScoreTag", out object? scoretag) && scoretag is string stag)
            {
                Config.Tags[key].ScoreTag = stag;
            }

            if (value.TryGetValue("ChatTag", out object? chattag) && chattag is string ctag)
            {
                Config.Tags[key].ChatTag = ctag;
            }

            if (value.TryGetValue("ChatColor", out object? chatcolor) && chatcolor is string ccolor)
            {
                Config.Tags[key].ChatColor = ccolor;
            }

            if (value.TryGetValue("NameColor", out object? namecolor) && namecolor is string ncolor)
            {
                Config.Tags[key].NameColor = ncolor;
            }

            Config.Tags[key].ChatSound = bool.Parse(value["ChatSound"].ToString()!);
        }
    }

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
}