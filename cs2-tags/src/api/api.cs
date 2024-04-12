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
        return tag switch
        {
            Tags.ScoreTag => Instance.PlayerDatas[player.Slot].ScoreTag,
            Tags.ChatTag => Instance.PlayerDatas[player.Slot].ChatTag,
            _ => string.Empty
        };
    }

    public void SetClientTag(CCSPlayerController player, Tags tag, string newtag)
    {
        if (tag == Tags.ScoreTag)
        {
            Instance.PlayerDatas[player.Slot].ScoreTag = newtag;
        }
        else
        {
            Instance.PlayerDatas[player.Slot].ChatTag = newtag;
        }
    }

    public void ResetClientTag(CCSPlayerController player, Tags tag)
    {
        if (tag == Tags.ScoreTag)
        {
            Instance.PlayerDatas[player.Slot].ScoreTag = GetTag(player).ScoreTag;
        }
        else
        {
            Instance.PlayerDatas[player.Slot].ChatTag = GetTag(player).ChatTag;
        }
    }

    public string GetClientColor(CCSPlayerController player, Colors color)
    {
        return color switch
        {
            Colors.NameColor => Instance.PlayerDatas[player.Slot].NameColor,
            Colors.ChatColor => Instance.PlayerDatas[player.Slot].ChatColor,
            _ => string.Empty
        };
    }

    public void SetClientColor(CCSPlayerController player, Colors color, string newcolor)
    {
        if (color == Colors.ChatColor)
        {
            Instance.PlayerDatas[player.Slot].ChatColor = newcolor;
        }
        else
        {
            Instance.PlayerDatas[player.Slot].NameColor = newcolor;
        }
    }

    public void ResetClientColor(CCSPlayerController player, Colors color)
    {
        CTag defaultTag = GetTag(player);

        if (color == Colors.ChatColor)
        {
            Instance.PlayerDatas[player.Slot].ChatColor = defaultTag.ChatColor;
        }
        else
        {
            Instance.PlayerDatas[player.Slot].NameColor = defaultTag.NameColor;
        }
    }

    public void ReloadTags()
    {
        Json.ReadConfig();
        UpdatePlayerTags();
    }
}