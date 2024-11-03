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
    private const string playerdesignername = "cs_player_controller";
    [GeneratedRegex(@"\{.*?\}|\p{C}")] public static partial Regex MyRegex();

    public static void LoadTag(this CCSPlayerController player)
    {
        Tag tag = player.GetTag().Clone();
        Tags.Api.TagsUpdatedPre(player, tag);
        PlayerTagsList.Add(player, tag);
        player.SetTag(tag.ScoreTag);
        Tags.Api.TagsUpdatedPost(player, tag);
    }
    public static Tag GetTag(this CCSPlayerController player)
    {
        Dictionary<string, Tag> tags = Config.Tags;

        if (tags.TryGetValue(player.SteamID.ToString(), out Tag? steamidTag))
        {
            return steamidTag;
        }

        foreach (KeyValuePair<string, Tag> tag in tags)
        {
            if (tag.Key[0] == '#' && AdminManager.PlayerInGroup(player, tag.Key))
            {
                return tag.Value;
            }
        }

        foreach (KeyValuePair<string, Tag> tag in tags)
        {
            if (tag.Key[0] == '@' && AdminManager.PlayerHasPermissions(player, tag.Key))
            {
                return tag.Value;
            }
        }

        return Config.DefaultTags;
    }

    public static string GetTag(this CCSPlayerController player, Tags_Tags tag)
    {
        if (PlayerTagsList.TryGetValue(player, out Tag? playerData))
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
        if (PlayerTagsList.TryGetValue(player, out Tag? playerData))
        {
            switch (tag)
            {
                case Tags_Tags.ScoreTag:
                    playerData.ScoreTag = newtag;
                    player.SetTag(newtag);
                    break;
                case Tags_Tags.ChatTag:
                    playerData.ChatTag = newtag;
                    break;
            }
        }
    }

    public static void ResetTag(this CCSPlayerController player, Tags_Tags tag)
    {
        if (PlayerTagsList.TryGetValue(player, out Tag? playerData))
        {
            Tag defaultTag = GetTag(player).Clone();
            switch (tag)
            {
                case Tags_Tags.ScoreTag:
                    playerData.ScoreTag = defaultTag.ScoreTag;
                    player.SetTag(defaultTag.ScoreTag);
                    break;
                case Tags_Tags.ChatTag:
                    playerData.ChatTag = defaultTag.ChatTag;
                    break;
            }
        }
    }

    public static string GetColor(this CCSPlayerController player, Tags_Colors color)
    {
        if (PlayerTagsList.TryGetValue(player, out Tag? playerData))
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
        if (PlayerTagsList.TryGetValue(player, out Tag? playerData))
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
        if (PlayerTagsList.TryGetValue(player, out Tag? playerData))
        {
            Tag defaultTag = GetTag(player).Clone();
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
        if (PlayerTagsList.TryGetValue(player, out Tag? playerData))
        {
            return playerData.ChatSound;
        }

        return true;
    }

    public static void SetChatSound(this CCSPlayerController player, bool value)
    {
        if (PlayerTagsList.TryGetValue(player, out Tag? playerData))
        {
            playerData.ChatSound = value;
        }
    }

    public static bool GetToggleTags(this CCSPlayerController player) => PlayerToggleTagsList.Contains(player);

    public static void SetToggleTags(this CCSPlayerController player, bool value)
    {
        if (value)
        {
            PlayerToggleTagsList.Add(player);
            player.SetTag(Config.DefaultTags.ScoreTag);
        }
        else
        {
            PlayerToggleTagsList.Remove(player);
            player.SetTag(player.GetTag(Tags_Tags.ScoreTag));
        }
    }

    public static void SetTag(this CCSPlayerController player, string tag)
    {
        if (player.Clan == tag)
        {
            return;
        }

        player.Clan = tag;
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");

        EventNextlevelChanged fakeEvent = new(false);
        fakeEvent.FireEventToClient(player);
    }

    public static string RemoveCurlyBraceContent(this string message) =>
        MyRegex().Replace(message, string.Empty);

    public static string ReplaceTags(this string message, CsTeam team) =>
        StringExtensions.ReplaceColorTags(message)
            .Replace("{TeamColor}", ChatColors.ForTeam(team).ToString());

    public static string Name(this CsTeam team) => Config.Settings.TeamNames[team];

    public static string FormatMessage(CsTeam team, params string[] args)
    {
        StringBuilder sb = new(args.Sum(arg => arg.Length));

        foreach (string arg in args)
        {
            sb.Append(arg);
        }

        return ReplaceTags(sb.ToString(), team);
    }

    public static void SetIfPresent<T>(this TomlTable table, string key, Action<T> setter)
    {
        if (table.TryGetValue(key, out object? value) && value is T typedValue)
        {
            setter(typedValue);
        }
    }

    public static void GetPlayers(out HashSet<CCSPlayerController> players)
    {
        players = [];

        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            CCSPlayerController? player = Utilities.GetEntityFromIndex<CCSPlayerController>(i + 1);

            if (player?.DesignerName != playerdesignername || player.IsBot)
            {
                continue;
            }

            players.Add(player);
        }
    }
}