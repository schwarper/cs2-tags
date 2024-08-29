using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using static TagsApi.Tags;

namespace TagsApi;

public interface ITagApi
{
    public static readonly PluginCapability<ITagApi?> Capability = new("tags:api");
    public string GetPlayerTag(CCSPlayerController player, Tags_Tags tag);
    public void SetPlayerTag(CCSPlayerController player, Tags_Tags tag, string newtag);
    public void ResetPlayerTag(CCSPlayerController player, Tags_Tags tag);
    public string GetPlayerColor(CCSPlayerController player, Tags_Colors color);
    public void SetPlayerColor(CCSPlayerController player, Tags_Colors color, string newcolor);
    public void ResetPlayerColor(CCSPlayerController player, Tags_Colors color);
    public bool GetPlayerChatSound(CCSPlayerController player);
    public void SetPlayerChatSound(CCSPlayerController controller, bool value);
    public void ReloadTags();
}