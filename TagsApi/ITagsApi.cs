using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using static TagsApi.Tags;

namespace TagsApi;

public interface ITagApi
{
    public static readonly PluginCapability<ITagApi> Capability = new("tags:api");

    public event Func<string, string, bool, bool, HookResult>? OnMessageProcessPre;
    public event Func<string, string, bool, bool, HookResult>? OnMessageProcess;
    public event Action<string, string, bool, bool>? OnMessageProcessPost;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPre;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPost;

    public string GetPlayerTag(CCSPlayerController player, Tags_Tags tag);
    public void SetPlayerTag(CCSPlayerController player, Tags_Tags tag, string newtag);
    public void ResetPlayerTag(CCSPlayerController player, Tags_Tags tag);
    public string GetPlayerColor(CCSPlayerController player, Tags_Colors color);
    public void SetPlayerColor(CCSPlayerController player, Tags_Colors color, string newcolor);
    public void ResetPlayerColor(CCSPlayerController player, Tags_Colors color);
    public bool GetPlayerChatSound(CCSPlayerController player);
    public void SetPlayerChatSound(CCSPlayerController controller, bool value);
    public bool GetPlayerToggleTags(CCSPlayerController player);
    public void SetPlayerToggleTags(CCSPlayerController player, bool value);
    public void ReloadTags();
}