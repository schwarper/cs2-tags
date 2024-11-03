using CounterStrikeSharp.API.Core;
using TagsApi;
using static Tags.Config_Config;
using static TagsApi.Tags;

namespace Tags;

public class TagsAPI : ITagApi
{
    public event Func<string, string, bool, bool, HookResult>? OnMessageProcessPre;
    public event Func<string, string, bool, bool, HookResult>? OnMessageProcess;
    public event Action<string, string, bool, bool>? OnMessageProcessPost;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPre;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPost;

    public HookResult MessageProcessPre(string playername, string message, bool chatsound, bool teammessage)
    {
        return OnMessageProcessPre?.Invoke(playername, message, chatsound, teammessage) ?? HookResult.Continue;
    }
    public HookResult MessageProcess(string playername, string message, bool chatsound, bool teammessage)
    {
        return OnMessageProcess?.Invoke(playername, message, chatsound, teammessage) ?? HookResult.Continue;
    }
    public void MessageProcessPost(string playername, string message, bool chatsound, bool teammessage)
    {
        OnMessageProcessPost?.Invoke(playername, message, chatsound, teammessage);
    }
    public void TagsUpdatedPre(CCSPlayerController player, Tag tag)
    {
        OnTagsUpdatedPre?.Invoke(player, tag);
    }
    public void TagsUpdatedPost(CCSPlayerController player, Tag tag)
    {
        OnTagsUpdatedPost?.Invoke(player, tag);
    }
    public string GetPlayerTag(CCSPlayerController player, Tags_Tags tag)
    {
        return player.GetTag(tag);
    }
    public void SetPlayerTag(CCSPlayerController player, Tags_Tags tag, string newtag)
    {
        player.SetTag(tag, newtag);
    }
    public void ResetPlayerTag(CCSPlayerController player, Tags_Tags tag)
    {
        player.ResetTag(tag);
    }
    public string GetPlayerColor(CCSPlayerController player, Tags_Colors color)
    {
        return player.GetColor(color);
    }
    public void SetPlayerColor(CCSPlayerController player, Tags_Colors color, string newcolor)
    {
        player.SetColor(color, newcolor);
    }
    public void ResetPlayerColor(CCSPlayerController player, Tags_Colors color)
    {
        player.ResetColor(color);
    }
    public bool GetPlayerChatSound(CCSPlayerController player)
    {
        return player.GetChatSound();
    }
    public void SetPlayerChatSound(CCSPlayerController player, bool value)
    {
        player.SetChatSound(value);
    }
    public bool GetPlayerToggleTags(CCSPlayerController player)
    {
        return player.GetToggleTags();
    }
    public void SetPlayerToggleTags(CCSPlayerController player, bool value)
    {
        player.SetToggleTags(value);
    }
    public void ReloadTags()
    {
        Reload();
    }
}