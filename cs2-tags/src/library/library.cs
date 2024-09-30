using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text;
using System.Text.RegularExpressions;
using Tomlyn.Model;
using static Tags.Config_Config;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static partial class TagsLibrary
{
    public static void LoadTag(this CCSPlayerController player)
    {
        PlayerTagsList.TryAdd(player.SteamID, player.GetTag());
    }
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
        if (PlayerTagsList.TryGetValue(player.SteamID, out var playerData))
        {
            return tag switch
            {
                Tags_Tags.ScoreTag => playerData.ScoreTag,
                Tags_Tags.ChatTag => playerData.ChatTag,
                _ => string.Empty
            };
        }
        return string.Empty;
    }

    public static void SetTag(this CCSPlayerController player, Tags_Tags tag, string newtag)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out var playerData))
        {
            switch (tag)
            {
                case Tags_Tags.ScoreTag:
                    playerData.ScoreTag = newtag;
                    break;
                case Tags_Tags.ChatTag:
                    playerData.ChatTag = newtag;
                    break;
            }
        }
    }

    public static void ResetTag(this CCSPlayerController player, Tags_Tags tag)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out var playerData))
        {
            Tag defaultTag = GetTag(player);
            switch (tag)
            {
                case Tags_Tags.ScoreTag:
                    playerData.ScoreTag = defaultTag.ScoreTag;
                    break;
                case Tags_Tags.ChatTag:
                    playerData.ChatTag = defaultTag.ChatTag;
                    break;
            }
        }
    }

    public static string GetColor(this CCSPlayerController player, Tags_Colors color)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out var playerData))
        {
            return color switch
            {
                Tags_Colors.NameColor => playerData.NameColor,
                Tags_Colors.ChatColor => playerData.ChatColor,
                _ => string.Empty
            };
        }

        return string.Empty;
    }

    public static void SetColor(this CCSPlayerController player, Tags_Colors color, string newcolor)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out var playerData))
        {
            switch (color)
            {
                case Tags_Colors.NameColor:
                    playerData.NameColor = newcolor;
                    break;
                case Tags_Colors.ChatColor:
                    playerData.ChatColor = newcolor;
                    break;
            }
        }
    }

    public static void ResetColor(this CCSPlayerController player, Tags_Colors color)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out var playerData))
        {
            Tag defaultTag = GetTag(player);
            switch (color)
            {
                case Tags_Colors.NameColor:
                    playerData.NameColor = defaultTag.NameColor;
                    break;
                case Tags_Colors.ChatColor:
                    playerData.ChatColor = defaultTag.ChatColor;
                    break;
            }
        }
    }

    public static bool GetChatSound(this CCSPlayerController player)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out var playerData))
        {
            return playerData.ChatSound;
        }

        return true;
    }

    public static void SetChatSound(this CCSPlayerController player, bool value)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out var playerData))
        {
            playerData.ChatSound = value;
        }
    }

    public static bool GetToggleTags(this CCSPlayerController player) => PlayerToggleTagsList.Contains(player.SteamID);

    public static void SetToggleTags(this CCSPlayerController player, bool value)
    {
        var steamid = player.SteamID;

        if (value)
        {
            PlayerToggleTagsList.Add(steamid);
        }
        else
        {
            PlayerToggleTagsList.Remove(steamid);
        }
    }

    public static void SetTag(this CCSPlayerController player, string tag)
    {
        player.Clan = tag;
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");

        EventNextlevelChanged fakeEvent = new(false);
        fakeEvent.FireEventToClient(player);
    }

    [GeneratedRegex(@"\{.*?\}|\p{C}")] public static partial Regex MyRegex();
    public static string RemoveCurlyBraceContent(this string message)
    {
        return MyRegex().Replace(message, string.Empty);
    }

    public static string ReplaceTags(this string message, CsTeam team)
    {
        string modifiedValue = StringExtensions.ReplaceColorTags(message)
            .Replace("{TeamColor}", ChatColors.ForTeam(team).ToString());

        return modifiedValue;
    }

    public static string Name(this CsTeam team)
    {
        return team switch
        {
            CsTeam.Spectator => ReplaceTags(Config.Settings.SpecName, CsTeam.Spectator),
            CsTeam.Terrorist => ReplaceTags(Config.Settings.TName, CsTeam.Terrorist),
            CsTeam.CounterTerrorist => ReplaceTags(Config.Settings.CTName, CsTeam.CounterTerrorist),
            CsTeam.None => ReplaceTags(Config.Settings.NoneName, CsTeam.None),
            _ => ReplaceTags(Config.Settings.NoneName, CsTeam.None)
        };
    }

    public static string FormatMessage(string deadIcon, string teamname, string tag, string namecolor, string chatcolor, string playername, string message, CsTeam team)
    {
        var sb = new StringBuilder();
        sb.Append(deadIcon).Append(teamname).Append(tag).Append(namecolor).Append(playername)
          .Append(ChatColors.Default).Append(": ").Append(chatcolor).Append(message);

        return ReplaceTags(sb.ToString(), team);
    }

    public static void SetIfPresent<T>(this TomlTable table, string key, Action<T> setter)
    {
        if (table.TryGetValue(key, out object? value) && value is T typedValue)
        {
            setter(typedValue);
        }
    }

    public static HashSet<CCSPlayerController> GetPlayers()
    {
        HashSet<CCSPlayerController> players = [];

        const string playerdesignername = "cs_player_controller";

        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            CCSPlayerController? player = Utilities.GetEntityFromIndex<CCSPlayerController>(i + 1);

            if (player?.DesignerName != playerdesignername || player.IsBot)
            {
                continue;
            }

            players.Add(player);
        }

        return players;
    }
}