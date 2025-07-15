using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static partial class TagExtensions
{
    [GeneratedRegex(@"\{.*?\}|\p{C}")]
    private static partial Regex MyRegex();

    public static string Name(this CsTeam team)
    {
        return Instance.Config.Settings.TeamNames[team];
    }

    public static string RemoveCurlyBraceContent(this string message)
    {
        return MyRegex().Replace(message, string.Empty);
    }

    public static string ReplaceTags(this string message, CsTeam team)
    {
        return message.ReplaceColorTags()
                      .Replace("{TeamColor}", ChatColors.ForTeam(team).ToString());
    }

    public static string FormatMessage(CsTeam team, params string[] args)
    {
        return ReplaceTags(string.Concat(args), team);
    }

    private static Tag GetOrCreatePlayerTag(CCSPlayerController player, bool force)
    {
        if (force || !PlayerTagsList.TryGetValue(player.SteamID, out Tag? tag) || tag is null)
        {
            tag = player.GetTag();
            PlayerTagsList[player.SteamID] = tag;
        }

        return tag;
    }


    public static Tag GetTag(this CCSPlayerController player)
    {
        string steamId = player.SteamID.ToString();

        Tag? steamIdTag = Instance.Config.Tags.FirstOrDefault(t => steamId == t.Role)?.Clone();
        if (steamIdTag != null)
            return steamIdTag;

        SteamID steamID = new(player.SteamID);

        Tag? groupTag = Instance.Config.Tags
            .Where(t => t.Role is { Length: > 0 } && t.Role[0] == '#' && AdminManager.PlayerInGroup(steamID, t.Role))
            .Select(t => t.Clone())
            .FirstOrDefault();

        if (groupTag != null)
            return groupTag;

        Tag? permissionTag = Instance.Config.Tags
            .Where(t => t.Role is { Length: > 0 } && t.Role[0] == '@' && AdminManager.PlayerHasPermissions(steamID, t.Role))
            .Select(t => t.Clone())
            .FirstOrDefault();

        return permissionTag ?? Instance.Config.Default.Clone();
    }

    public static string GetPrePostValue(TagPrePost prePost, string? oldValue, string newValue)
    {
        return prePost switch
        {
            TagPrePost.Pre => newValue + oldValue,
            TagPrePost.Post => oldValue + newValue,
            _ => newValue
        };
    }

    public static void AddAttribute(this CCSPlayerController player, TagType types, TagPrePost prePost, string newValue)
    {
        Tag tag = GetOrCreatePlayerTag(player, false);

        Tags.Api.TagsUpdatedPre(player, tag);

        if ((types & TagType.ScoreTag) != 0)
        {
            string value = GetPrePostValue(prePost, tag.ScoreTag, newValue);
            tag.ScoreTag = value;
            player.SetScoreTag(value);
        }
        if ((types & TagType.ChatTag) != 0)
            tag.ChatTag = GetPrePostValue(prePost, tag.ChatTag, newValue);
        if ((types & TagType.NameColor) != 0)
            tag.NameColor = GetPrePostValue(prePost, tag.NameColor, newValue);
        if ((types & TagType.ChatColor) != 0)
            tag.ChatColor = GetPrePostValue(prePost, tag.ChatColor, newValue);

        Tags.Api.TagsUpdatedPost(player, tag);
    }

    public static void SetAttribute(this CCSPlayerController player, TagType types, string newValue)
    {
        Tag tag = GetOrCreatePlayerTag(player, false);

        Tags.Api.TagsUpdatedPre(player, tag);

        if ((types & TagType.ScoreTag) != 0)
        {
            tag.ScoreTag = newValue;
            player.SetScoreTag(newValue);
        }
        if ((types & TagType.ChatTag) != 0)
            tag.ChatTag = newValue;
        if ((types & TagType.NameColor) != 0)
            tag.NameColor = newValue;
        if ((types & TagType.ChatColor) != 0)
            tag.ChatColor = newValue;

        Tags.Api.TagsUpdatedPost(player, tag);
    }

    public static string? GetAttribute(this CCSPlayerController player, TagType type)
    {
        Tag tag = GetOrCreatePlayerTag(player, false);

        return type switch
        {
            TagType.ScoreTag => tag.ScoreTag,
            TagType.ChatTag => tag.ChatTag,
            TagType.NameColor => tag.NameColor,
            TagType.ChatColor => tag.ChatColor,
            _ => null
        };
    }

    public static void ResetAttribute(this CCSPlayerController player, TagType types)
    {
        Tag tag = GetOrCreatePlayerTag(player, false);
        Tag defaultTag = player.GetTag();

        Tags.Api.TagsUpdatedPre(player, tag);

        if ((types & TagType.ScoreTag) != 0)
        {
            tag.ScoreTag = defaultTag.ScoreTag;
            player.SetScoreTag(defaultTag.ScoreTag);
        }
        if ((types & TagType.ChatTag) != 0)
            tag.ChatTag = defaultTag.ChatTag;
        if ((types & TagType.NameColor) != 0)
            tag.NameColor = defaultTag.NameColor;
        if ((types & TagType.ChatColor) != 0)
            tag.ChatColor = defaultTag.ChatColor;

        Tags.Api.TagsUpdatedPost(player, tag);
    }

    public static bool GetChatSound(this CCSPlayerController player)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out Tag? tag))
            return tag.ChatSound;

        Tag defaultTag = player.GetTag();
        PlayerTagsList[player.SteamID] = defaultTag;
        return defaultTag.ChatSound;
    }

    public static void SetChatSound(this CCSPlayerController player, bool value)
    {
        Tag tag = GetOrCreatePlayerTag(player, false);

        Tags.Api.TagsUpdatedPre(player, tag);
        tag.ChatSound = value;
        Tags.Api.TagsUpdatedPost(player, tag);
    }

    public static bool GetVisibility(this CCSPlayerController player)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out Tag? tag))
            return tag.Visibility;

        Tag defaultTag = player.GetTag();
        PlayerTagsList[player.SteamID] = defaultTag;
        return defaultTag.Visibility;
    }

    public static void SetVisibility(this CCSPlayerController player, bool value)
    {
        Tag tag = GetOrCreatePlayerTag(player, false);

        Tags.Api.TagsUpdatedPre(player, tag);
        tag.Visibility = value;
        player.SetScoreTag(value ? player.GetAttribute(TagType.ScoreTag) : Instance.Config.Default.ScoreTag);
        Tags.Api.TagsUpdatedPost(player, tag);
    }

    public static void SetScoreTag(this CCSPlayerController player, string? tag)
    {
        if (tag != null && player.Clan != tag)
        {
            player.Clan = tag;
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");
            new EventNextlevelChanged(false).FireEventToClient(player);
        }
    }

    public static void ReloadConfig()
    {
        Instance.Config.Reload();
        Instance.Config.Settings.Init();
    }

    public static void ReloadTags()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        foreach (CCSPlayerController player in players)
        {
            if (player.IsBot)
                continue;

            Tag tag = GetOrCreatePlayerTag(player, true);
            player.SetScoreTag(tag.ScoreTag);

            Console.WriteLine(tag);
        }
    }
}