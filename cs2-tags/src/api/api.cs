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
        if (PlayerTags.TryGetValue(player.SteamID, out Tag? playerTag))
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
        if (PlayerTags.TryGetValue(player.SteamID, out Tag? playertag))
        {
            switch (tag)
            {
                case Tags_Tags.ScoreTag:
                    playertag.ScoreTag = newtag;
                    break;
                case Tags_Tags.ChatTag:
                    playertag.ChatTag = newtag;
                    break;
            }
        }
    }

    public void ResetPlayerTag(CCSPlayerController player, Tags_Tags tag)
    {
        if (PlayerTags.TryGetValue(player.SteamID, out Tag? playertag))
        {
            Tag defaultTag = GetTag(player);
            switch (tag)
            {
                case Tags_Tags.ScoreTag:
                    playertag.ScoreTag = defaultTag.ScoreTag;
                    break;
                case Tags_Tags.ChatTag:
                    playertag.ChatTag = defaultTag.ChatTag;
                    break;
            }
        }
    }

    public string GetPlayerColor(CCSPlayerController player, Tags_Colors color)
    {
        if (PlayerTags.TryGetValue(player.SteamID, out Tag? playertag))
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
        if (PlayerTags.TryGetValue(player.SteamID, out Tag? playertag))
        {
            switch (color)
            {
                case Tags_Colors.NameColor:
                    playertag.NameColor = newcolor;
                    break;
                case Tags_Colors.ChatColor:
                    playertag.ChatColor = newcolor;
                    break;
            }
        }
    }

    public void ResetPlayerColor(CCSPlayerController player, Tags_Colors color)
    {
        if (PlayerTags.TryGetValue(player.SteamID, out Tag? playertag))
        {
            Tag defaultTag = GetTag(player);
            switch (color)
            {
                case Tags_Colors.NameColor:
                    playertag.NameColor = defaultTag.NameColor;
                    break;
                case Tags_Colors.ChatColor:
                    playertag.ChatColor = defaultTag.ChatColor;
                    break;
            }
        }
    }

    public void ReloadTags()
    {
        Reload();
    }
}
