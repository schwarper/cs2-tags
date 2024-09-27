using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using static Tags.Config_Config;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static class Player
{
    public static Tag GetTag(this CCSPlayerController player)
    {
        Dictionary<string, Tag> tags = Config.Tags;

        if (tags.TryGetValue(player.SteamID.ToString(), out var steamidTag))
        {
            return steamidTag;
        }

        foreach (KeyValuePair<string, Tag> tag in tags.Where(tag => tag.Key.StartsWith('#')))
        {
            bool isInGroup = AdminManager.PlayerInGroup(player, tag.Key);

            if (isInGroup)
            {
                return tag.Value;
            }
        }

        foreach (KeyValuePair<string, Tag> tag in tags.Where(tag => tag.Key.StartsWith('@')))
        {
            bool hasPermission = AdminManager.PlayerHasPermissions(player, tag.Key);

            if (hasPermission)
            {
                return tag.Value;
            }
        }

        return Config.DefaultTags;
    }

    public static string GetTag(this CCSPlayerController player, Tags_Tags tag)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out PlayerData? playerData))
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

    public static void SetTag(this CCSPlayerController player, Tags_Tags tag, string newtag)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out PlayerData? playerData))
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

    public static void ResetTag(this CCSPlayerController player, Tags_Tags tag)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out PlayerData? playerData))
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

    public static string GetColor(this CCSPlayerController player, Tags_Colors color)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out PlayerData? playerData))
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

    public static void SetColor(this CCSPlayerController player, Tags_Colors color, string newcolor)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out PlayerData? playerData))
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

    public static void ResetColor(this CCSPlayerController player, Tags_Colors color)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out PlayerData? playerData))
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

    public static bool GetChatSound(this CCSPlayerController player)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out PlayerData? playerData))
        {
            return playerData.PlayerTag.ChatSound;
        }

        return true;
    }

    public static void SetChatSound(this CCSPlayerController player, bool value)
    {
        if (PlayerDataList.TryGetValue(player.SteamID, out PlayerData? playerData))
        {
            playerData.PlayerTag.ChatSound = value;
        }
    }

    public static void SetTag(this CCSPlayerController player, string tag)
    {
        player.Clan = tag;
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");

        EventNextlevelChanged fakeEvent = new(false);
        fakeEvent.FireEventToClient(player);
    }
}