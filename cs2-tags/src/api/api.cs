using CounterStrikeSharp.API.Core;
using TagsApi;
using static Tags.Config_Config;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public class TagsAPI : ITagApi
{
    public string GetPlayerTag(CCSPlayerController player, Tags_Tags tag)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out var playerData))
        {
            return tag switch
            {
                Tags_Tags.ScoreTag => playerData.PlayerTag.ScoreTag,
                Tags_Tags.ChatTag => playerData.PlayerTag.ChatTag,
                _ => string.Empty
            };
        }
        return string.Empty;
    }

    public void SetPlayerTag(CCSPlayerController player, Tags_Tags tag, string newtag)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out var playerData))
        {
            switch (tag)
            {
                case Tags_Tags.ScoreTag:
                    playerData.PlayerTag.ScoreTag = newtag;
                    break;
                case Tags_Tags.ChatTag:
                    playerData.PlayerTag.ChatTag = newtag;
                    break;
            }
        }
    }

    public void ResetPlayerTag(CCSPlayerController player, Tags_Tags tag)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out var playerData))
        {
            Tag defaultTag = GetTag(player);
            switch (tag)
            {
                case Tags_Tags.ScoreTag:
                    playerData.PlayerTag.ScoreTag = defaultTag.ScoreTag;
                    break;
                case Tags_Tags.ChatTag:
                    playerData.PlayerTag.ChatTag = defaultTag.ChatTag;
                    break;
            }
        }
    }

    public string GetPlayerColor(CCSPlayerController player, Tags_Colors color)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out var playerData))
        {
            return color switch
            {
                Tags_Colors.NameColor => playerData.PlayerTag.NameColor,
                Tags_Colors.ChatColor => playerData.PlayerTag.ChatColor,
                _ => string.Empty
            };
        }

        return string.Empty;
    }

    public void SetPlayerColor(CCSPlayerController player, Tags_Colors color, string newcolor)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out var playerData))
        {
            switch (color)
            {
                case Tags_Colors.NameColor:
                    playerData.PlayerTag.NameColor = newcolor;
                    break;
                case Tags_Colors.ChatColor:
                    playerData.PlayerTag.ChatColor = newcolor;
                    break;
            }
        }
    }

    public void ResetPlayerColor(CCSPlayerController player, Tags_Colors color)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out var playerData))
        {
            Tag defaultTag = GetTag(player);
            switch (color)
            {
                case Tags_Colors.NameColor:
                    playerData.PlayerTag.NameColor = defaultTag.NameColor;
                    break;
                case Tags_Colors.ChatColor:
                    playerData.PlayerTag.ChatColor = defaultTag.ChatColor;
                    break;
            }
        }
    }

    public void ReloadTags()
    {
        Reload();
    }
}
