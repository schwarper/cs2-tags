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
        const string playerdesignername = "cs_player_controller";

        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            CCSPlayerController? player = Utilities.GetEntityFromIndex<CCSPlayerController>(i + 1);

            if (player?.DesignerName != playerdesignername || player.IsBot)
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

            Tag configTag = new();

            value.SetIfPresent("ScoreTag", (string stag) => configTag.ScoreTag = stag);
            value.SetIfPresent("ChatTag", (string ctag) => configTag.ChatTag = ctag);
            value.SetIfPresent("ChatColor", (string ccolor) => configTag.ChatColor = ccolor);
            value.SetIfPresent("NameColor", (string ncolor) => configTag.NameColor = ncolor);
            value.SetIfPresent("ChatSound", (bool csound) => configTag.ChatSound = csound);

            Config.Tags.Add(key, configTag);
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