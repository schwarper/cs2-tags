using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using static TagsApi.Tags;
using static Tags.Tags;

namespace Tags;

public class Config : BasePluginConfig
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

    private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
    private static readonly string CfgPath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/{AssemblyName}/{AssemblyName}.json";

    public static void Update()
    {
        try
        {
            var file = File.ReadAllText(CfgPath);

            var jsonContent = JsonSerializer.Deserialize<Config>(file);

            if (jsonContent == null)
            {
                return;
            }

            Instance.Config = jsonContent;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error: {ex.Message}");
        }
    }
}