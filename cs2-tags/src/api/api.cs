using CounterStrikeSharp.API.Core;
using TagsApi;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public class TagsAPI : ITagApi
{
    public TagsAPI()
    {
    }

    public event OnPlayerChatDelegate? OnPlayerChat
    {
        add => Event.OnPlayerSay += value;
        remove => Event.OnPlayerSay -= value;
    }

    public string GetPlayerTag(CCSPlayerController player, Tags_Tags tag)
    {
        if (Instance.PlayerTagDatas.TryGetValue(player.Slot, out Tag? playerTag) && playerTag != null)
        {
            return tag switch
            {
                Tags_Tags.ScoreTag => playerTag.ScoreTag,
                Tags_Tags.ChatTag => playerTag.ChatTag,
                _ => string.Empty
            };
        }

        return string.Empty;
    }

    public void SetPlayerTag(CCSPlayerController player, Tags_Tags tag, string newtag)
    {
        if (Instance.PlayerTagDatas.TryGetValue(player.Slot, out Tag? playertag) && playertag != null)
        {
            if (tag == Tags_Tags.ScoreTag)
            {
                playertag.ScoreTag = newtag;
            }
            else
            {
                playertag.ChatTag = newtag;
            }
        }
    }

    public void ResetPlayerTag(CCSPlayerController player, Tags_Tags tag)
    {
        if (Instance.PlayerTagDatas.TryGetValue(player.Slot, out Tag? playertag) && playertag != null)
        {
            if (tag == Tags_Tags.ScoreTag)
            {
                playertag.ScoreTag = GetTag(player).ScoreTag;
            }
            else
            {
                playertag.ChatTag = GetTag(player).ChatTag;
            }
        }
    }

    public string GetPlayerColor(CCSPlayerController player, Tags_Colors color)
    {
        if (Instance.PlayerTagDatas.TryGetValue(player.Slot, out Tag? playertag) && playertag != null)
        {
            return color switch
            {
                Tags_Colors.NameColor => playertag.NameColor,
                Tags_Colors.ChatColor => playertag.ChatColor,
                _ => string.Empty
            };
        }

        return string.Empty;
    }

    public void SetPlayerColor(CCSPlayerController player, Tags_Colors color, string newcolor)
    {
        if (Instance.PlayerTagDatas.TryGetValue(player.Slot, out Tag? playertag) && playertag != null)
        {
            if (color == Tags_Colors.ChatColor)
            {
                playertag.ChatColor = newcolor;
            }
            else
            {
                playertag.NameColor = newcolor;
            }
        }
    }

    public void ResetPlayerColor(CCSPlayerController player, Tags_Colors color)
    {
        if (Instance.PlayerTagDatas.TryGetValue(player.Slot, out Tag? playertag) && playertag != null)
        {
            Tag defaultTag = GetTag(player);

            if (color == Tags_Colors.ChatColor)
            {
                playertag.ChatColor = defaultTag.ChatColor;
            }
            else
            {
                playertag.NameColor = defaultTag.NameColor;
            }
        }
    }

    public void ReloadTags()
    {
        UpdatePlayerTags();
    }
}