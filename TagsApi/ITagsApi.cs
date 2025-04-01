using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using static TagsApi.Tags;

namespace TagsApi;

public interface ITagApi
{
    public static readonly PluginCapability<ITagApi> Capability = new("tags:api");

    public event Func<MessageProcess, HookResult>? OnMessageProcessPre;
    public event Func<MessageProcess, HookResult>? OnMessageProcess;
    public event Action<MessageProcess>? OnMessageProcessPost;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPre;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPost;

    public void AddAttribute(CCSPlayerController player, TagType types, TagPrePost prePost, string newValue);
    public void SetAttribute(CCSPlayerController player, TagType types, string newValue);
    public string? GetAttribute(CCSPlayerController player, TagType type);
    public void ResetAttribute(CCSPlayerController player, TagType types);
    public bool GetPlayerChatSound(CCSPlayerController player);
    public void SetPlayerChatSound(CCSPlayerController controller, bool value);
    public bool GetPlayerVisibility(CCSPlayerController player);
    public void SetPlayerVisibility(CCSPlayerController player, bool value);
    public void ReloadTags();

    void SetExternalTag(CCSPlayerController player, TagType types, string newValue, bool persistent = true);
    void SetPlayerTagExternal(CCSPlayerController player, bool isExternal);
    bool IsPlayerTagExternal(CCSPlayerController player);
    void ClearExternalTag(CCSPlayerController player, bool resetToDefaultPermission = true);
}