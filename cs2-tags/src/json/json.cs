using CounterStrikeSharp.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using static Tag.Tag;

namespace Tag;

public class CoreConfig
{
    public JArray PublicChatTrigger { get; set; } = [];
    public JArray SilentChatTrigger { get; set; } = [];
}

public static class Json
{
    private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
    private static readonly string CorePath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/core.json";
    private static readonly string PluginJsonPath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/{AssemblyName}/{AssemblyName}.json";

    public static string[] PublicChatTrigger { get; set; } = [];
    public static string[] SilentChatTrigger { get; set; } = [];

    public static void ReadCore()
    {
        string jsonContent = File.ReadAllText(CorePath);

        dynamic? config = JsonConvert.DeserializeObject<dynamic>(jsonContent);

        if (config == null)
        {
            return;
        }

        PublicChatTrigger = ((JArray)config.PublicChatTrigger).Select(x => x.ToString()).ToArray();
        SilentChatTrigger = ((JArray)config.SilentChatTrigger).Select(x => x.ToString()).ToArray();
    }

    public static void ReadConfig()
    {
        string jsonContent = File.ReadAllText(PluginJsonPath);

        TagConfig? config = JsonConvert.DeserializeObject<TagConfig>(jsonContent);

        if (config == null)
        {
            return;
        }

        Instance.Config = config;
    }
}