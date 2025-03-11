using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static class ConfigManager
{
    public class Cfg
    {
        public Settings Settings { get; set; } = new();
        public DatabaseConnection DatabaseConnection { get; set; } = new();
        public Commands Commands { get; set; } = new();
        public Tag DefaultTags { get; set; } = new();
        public Dictionary<string, Tag> Tags { get; set; } = [];
    }

    public class Settings
    {
        public string Tag { get; set; } = string.Empty;
        public string DeadName { get; set; } = string.Empty;
        public Dictionary<CsTeam, string> TeamNames { get; set; } = [];
    }

    public class DatabaseConnection
    {
        public string Host { get; set; } = string.Empty;
        public uint Port { get; set; } = 3306;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class Commands
    {
        public string[] TagsReload { get; set; } = [];
        public string[] Visibility { get; set; } = [];
        public string[] TagsMenu { get; set; } = [];
    }

    public static Cfg Config { get; set; } = new();
    private static readonly string ConfigFilePath;

    static ConfigManager()
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
        ConfigFilePath = Path.Combine(
            Server.GameDirectory,
            "csgo",
            "addons",
            "counterstrikesharp",
            "configs",
            "plugins",
            assemblyName,
            "config.toml"
        );
    }

    public static void LoadConfig(bool hotReload)
    {
        if (!File.Exists(ConfigFilePath))
            throw new FileNotFoundException($"Configuration file not found: {ConfigFilePath}");

        Config = new();
        PlayerTagsList.Clear();

        string configText = File.ReadAllText(ConfigFilePath);
        TomlTable model = Toml.ToModel(configText);

        foreach ((string key, object value) in model)
        {
            TomlTable table = (TomlTable)value;

            switch (key)
            {
                case "Settings":
                    LoadSettings(table);
                    break;
                case "Default":
                    LoadDefaultTags(table);
                    break;
                case "DatabaseConnection":
                    LoadDatabaseConnection(table);
                    break;
                case "Commands":
                    LoadCommands(table);
                    break;
                default:
                    LoadCustomTag(key, table);
                    break;
            }
        }

        if (hotReload)
            LoadPlayerTags();
    }

    private static void LoadSettings(TomlTable table)
    {
        Config.Settings.Tag = table["Tag"].ToString()!.ReplaceColorTags();
        Config.Settings.DeadName = table["DeadName"].ToString()!;

        Config.Settings.TeamNames[CsTeam.None] = TagsLibrary.ReplaceTags(table["NoneName"]?.ToString()!, CsTeam.None);
        Config.Settings.TeamNames[CsTeam.Spectator] = TagsLibrary.ReplaceTags(table["SpecName"]?.ToString()!, CsTeam.Spectator);
        Config.Settings.TeamNames[CsTeam.Terrorist] = TagsLibrary.ReplaceTags(table["TName"]?.ToString()!, CsTeam.Terrorist);
        Config.Settings.TeamNames[CsTeam.CounterTerrorist] = TagsLibrary.ReplaceTags(table["CTName"]?.ToString()!, CsTeam.CounterTerrorist);
    }

    private static void LoadDefaultTags(TomlTable table)
    {
        table.SetIfPresent("ScoreTag", (string value) => Config.DefaultTags.ScoreTag = value);
        table.SetIfPresent("ChatTag", (string value) => Config.DefaultTags.ChatTag = value);
        table.SetIfPresent("ChatColor", (string value) => Config.DefaultTags.ChatColor = value);
        table.SetIfPresent("NameColor", (string value) => Config.DefaultTags.NameColor = value);
        table.SetIfPresent("ChatSound", (bool value) => Config.DefaultTags.ChatSound = value);
    }

    private static void LoadDatabaseConnection(TomlTable table)
    {
        Config.DatabaseConnection = new DatabaseConnection
        {
            Host = table["Host"].ToString()!,
            Port = uint.Parse(table["Port"].ToString()!),
            User = table["User"].ToString()!,
            Password = table["Password"].ToString()!,
            Name = table["Name"].ToString()!,
        };

        Database.SetConnectionString(Config.DatabaseConnection);
        Task.Run(Database.CreateDatabaseAsync);
    }

    private static void LoadCommands(TomlTable table)
    {
        Config.Commands.TagsReload = table["TagsReload"].GetStringArray();
        Config.Commands.Visibility = table["Visibility"].GetStringArray();
        Config.Commands.TagsMenu = table["TagsMenu"].GetStringArray();
    }

    private static void LoadCustomTag(string key, TomlTable table)
    {
        Tag configTag = new();
        table.SetIfPresent("ScoreTag", (string value) => configTag.ScoreTag = value);
        table.SetIfPresent("ChatTag", (string value) => configTag.ChatTag = value);
        table.SetIfPresent("ChatColor", (string value) => configTag.ChatColor = value);
        table.SetIfPresent("NameColor", (string value) => configTag.NameColor = value);
        table.SetIfPresent("ChatSound", (bool value) => configTag.ChatSound = value);

        Config.Tags[key] = configTag;
    }

    public static void LoadPlayerTags()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers().Where(p => !p.IsBot).ToList();

        if (players.Count == 0)
            return;

        foreach (CCSPlayerController player in players)
        {
            Task.Run(() => Database.LoadPlayer(player));
        }
    }
}