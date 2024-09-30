using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.UserMessages;
using TagsApi;
using static Tags.Config_Config;
using static TagsApi.Tags;

namespace Tags;

public class TagsAPI : ITagApi
{
    public event Action<UserMessage>? OnPlayerChatPre;

    public void PlayerChat(UserMessage message)
    {
        OnPlayerChatPre?.Invoke(message);
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