using CounterStrikeSharp.API.Core;
using TagApi;
using static Tag.Tag;
using static TagApi.Tag;

namespace Tag;

public class TagAPI : ITagApi
{
    public TagAPI()
    {
    }

    public string GetClientTag(CCSPlayerController player, Tags tag)
    {
        if (!Instance.PlayerTagDatas.TryGetValue(player.Slot, out CTag? playertag) || playertag == null)
        {
            return string.Empty;
        }

        return tag switch
        {
            Tags.ScoreTag => playertag.ScoreTag,
            Tags.ChatTag => playertag.ChatTag,
            _ => string.Empty
        };
    }

    public void SetClientTag(CCSPlayerController player, Tags tag, string newtag)
    {
        if (!Instance.PlayerTagDatas.TryGetValue(player.Slot, out CTag? playertag) || playertag == null)
        {
            return;
        }

        if (tag == Tags.ScoreTag)
        {
            playertag.ScoreTag = newtag;
        }
        else
        {
            playertag.ChatTag = newtag;
        }
    }

    public void ResetClientTag(CCSPlayerController player, Tags tag)
    {
        if (!Instance.PlayerTagDatas.TryGetValue(player.Slot, out CTag? playertag) || playertag == null)
        {
            return;
        }

        if (tag == Tags.ScoreTag)
        {
            playertag.ScoreTag = GetTag(player).ScoreTag;
        }
        else
        {
            playertag.ChatTag = GetTag(player).ChatTag;
        }
    }

    public string GetClientColor(CCSPlayerController player, Colors color)
    {
        if (!Instance.PlayerTagDatas.TryGetValue(player.Slot, out CTag? playertag) || playertag == null)
        {
            return string.Empty;
        }

        return color switch
        {
            Colors.NameColor => playertag.NameColor,
            Colors.ChatColor => playertag.ChatColor,
            _ => string.Empty
        };
    }

    public void SetClientColor(CCSPlayerController player, Colors color, string newcolor)
    {
        if (!Instance.PlayerTagDatas.TryGetValue(player.Slot, out CTag? playertag) || playertag == null)
        {
            return;
        }

        if (color == Colors.ChatColor)
        {
            playertag.ChatColor = newcolor;
        }
        else
        {
            playertag.NameColor = newcolor;
        }
    }

    public void ResetClientColor(CCSPlayerController player, Colors color)
    {
        if (!Instance.PlayerTagDatas.TryGetValue(player.Slot, out CTag? playertag) || playertag == null)
        {
            return;
        }

        CTag defaultTag = GetTag(player);

        if (color == Colors.ChatColor)
        {
            playertag.ChatColor = defaultTag.ChatColor;
        }
        else
        {
            playertag.NameColor = defaultTag.NameColor;
        }
    }

    public void ReloadTags()
    {
        UpdatePlayerTags();
    }
}