using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static TagsApi.Tags;

namespace Tags;

public class Config : BasePluginConfig
{
    public class Config_Settings
    {
        public string Tag { get; set; } = string.Empty;
        public string MenuType { get; set; } = "ScreenMenu";
        public string DeadName { get; set; } = string.Empty;
        public string NoneName { get; set; } = string.Empty;
        public string SpecName { get; set; } = string.Empty;
        public string TName { get; set; } = string.Empty;
        public string CTName { get; set; } = string.Empty;
        public Dictionary<CsTeam, string> TeamNames = [];
    }
    public class Config_DatabaseConnection
    {
        public bool MySQL { get; set; } = true;
        public string Host { get; set; } = string.Empty;
        public uint Port { get; set; } = 3306;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
    public class Config_Commands
    {
        public string[] TagsReload { get; set; } = [];
        public string[] Visibility { get; set; } = [];
        public string[] TagsMenu { get; set; } = [];
    }

    public Config_Settings Settings { get; set; } = new();
    public Config_DatabaseConnection DatabaseConnection { get; set; } = new();
    public Config_Commands Commands { get; set; } = new();
    public Tag Default { get; set; } = new();
    public List<Tag> Tags { get; set; } = [];
}