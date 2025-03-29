using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using TagsApi;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public class TagsAPI : ITagApi
{
    public event Func<MessageProcess, HookResult>? OnMessageProcessPre;
    public event Func<MessageProcess, HookResult>? OnMessageProcess;
    public event Action<MessageProcess>? OnMessageProcessPost;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPre;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPost;

    public HookResult MessageProcessPre(MessageProcess messageProcess)
    {
        return OnMessageProcessPre?.Invoke(messageProcess) ?? HookResult.Continue;
    }

    public HookResult MessageProcess(MessageProcess messageProcess)
    {
        return OnMessageProcess?.Invoke(messageProcess) ?? HookResult.Continue;
    }

    public void MessageProcessPost(MessageProcess messageProcess)
    {
        OnMessageProcessPost?.Invoke(messageProcess);
    }

    public void TagsUpdatedPre(CCSPlayerController player, Tag tag)
    {
        OnTagsUpdatedPre?.Invoke(player, tag);
    }

    public void TagsUpdatedPost(CCSPlayerController player, Tag tag)
    {
        OnTagsUpdatedPost?.Invoke(player, tag);
    }

    public void AddAttribute(CCSPlayerController player, TagType types, TagPrePost prePost, string newValue)
    {
        player.AddAttribute(types, prePost, newValue);
    }

    public void SetAttribute(CCSPlayerController player, TagType types, string newValue)
    {
        player.SetAttribute(types, newValue);
    }

    public string? GetAttribute(CCSPlayerController player, TagType type)
    {
        return player.GetAttribute(type);
    }

    public void ResetAttribute(CCSPlayerController player, TagType types)
    {
        player.ResetAttribute(types);
    }

    public bool GetPlayerChatSound(CCSPlayerController player)
    {
        return player.GetChatSound();
    }

    public void SetPlayerChatSound(CCSPlayerController player, bool value)
    {
        player.SetChatSound(value);
    }

    public bool GetPlayerVisibility(CCSPlayerController player)
    {
        return player.GetVisibility();
    }

    public void SetPlayerVisibility(CCSPlayerController player, bool value)
    {
        player.SetVisibility(value);
    }

    public void ReloadTags()
    {
        Instance.Config.Reload();
        UpdateConfig(Instance.Config);
    }
}