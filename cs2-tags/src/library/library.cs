using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.RegularExpressions;
using Tomlyn.Model;
using static Tags.ConfigManager;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;


public static partial class TagsLibrary
{
    [GeneratedRegex(@"\{.*?\}|\p{C}")]
    public static partial Regex MyRegex();

    public static string Name(this CsTeam team) => Config.Settings.TeamNames[team];

    public static string RemoveCurlyBraceContent(this string message) =>
        MyRegex().Replace(message, string.Empty);

    public static string ReplaceTags(this string message, CsTeam team) =>
        StringExtensions.ReplaceColorTags(message)
            .Replace("{TeamColor}", ChatColors.ForTeam(team).ToString());

    public static string FormatMessage(CsTeam team, params string[] args) =>
        ReplaceTags(string.Concat(args), team);

    public static string[] GetStringArray(this object tomlArray) =>
        [.. ((TomlArray)tomlArray).Select(item => item!.ToString()!)];

    public static void SetIfPresent<T>(this TomlTable table, string key, Action<T> setter)
    {
        if (table.TryGetValue(key, out object? value) && value is T typedValue)
            setter(typedValue);
    }

    public static Tag GetTag(this CCSPlayerController player) =>
        Config.Tags.TryGetValue(player.SteamID.ToString(), out Tag? steamidTag) ? steamidTag.Clone() :
        Config.Tags.FirstOrDefault(tag => tag.Key[0] == '#' && AdminManager.PlayerInGroup(player, tag.Key)).Value?.Clone() ??
        Config.Tags.FirstOrDefault(tag => tag.Key[0] == '@' && AdminManager.PlayerHasPermissions(player, tag.Key)).Value?.Clone() ??
        Config.DefaultTags.Clone();

    public static List<Tag> GetTags(this CCSPlayerController player)
    {
        string steamID = player.SteamID.ToString();

        return
            [
                Config.DefaultTags.Clone(),
                .. Config.Tags
                    .Where(tag =>
                        tag.Key == steamID ||
                        (tag.Key[0] == '#' && AdminManager.PlayerInGroup(player, tag.Key)) ||
                        (tag.Key[0] == '@' && AdminManager.PlayerHasPermissions(player, tag.Key))
                    )
                    .Select(tag => tag.Value.Clone()),
            ];
    }

    public static void UpdateTag(this CCSPlayerController player, Tag tag)
    {
        Server.NextFrame(() =>
        {
            Tags.Api.TagsUpdatedPre(player, tag);
            PlayerTagsList[player] = tag;
            player.SetScoreTag(tag.ScoreTag);
            Tags.Api.TagsUpdatedPost(player, tag);
        });
    }

    private static string GetPrePostValue(TagPrePost prePost, string? oldValue, string newValue)
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
        Tag playerData = PlayerTagsList[player];
        Tags.Api.TagsUpdatedPre(player, playerData);

        if ((types & TagType.ScoreTag) != 0)
        {
            string value = GetPrePostValue(prePost, playerData.ScoreTag, newValue);
            playerData.ScoreTag = value;
            player.SetScoreTag(value);
        }
        if ((types & TagType.ChatTag) != 0)
        {
            string value = GetPrePostValue(prePost, playerData.ChatTag, newValue);
            playerData.ChatTag = value;
        }
        if ((types & TagType.NameColor) != 0)
        {
            string value = GetPrePostValue(prePost, playerData.NameColor, newValue);
            playerData.NameColor = value;
        }
        if ((types & TagType.ChatColor) != 0)
        {
            string value = GetPrePostValue(prePost, playerData.ChatColor, newValue);
            playerData.ChatColor = value;
        }
        Tags.Api.TagsUpdatedPost(player, playerData);
    }

    public static void SetAttribute(this CCSPlayerController player, TagType types, string newValue)
    {
        Tag playerData = PlayerTagsList[player];
        Tags.Api.TagsUpdatedPre(player, playerData);

        if ((types & TagType.ScoreTag) != 0)
        {
            playerData.ScoreTag = newValue;
            player.SetScoreTag(newValue);
        }
        if ((types & TagType.ChatTag) != 0)
        {
            playerData.ChatTag = newValue;
        }
        if ((types & TagType.NameColor) != 0)
        {
            playerData.NameColor = newValue;
        }
        if ((types & TagType.ChatColor) != 0)
        {
            playerData.ChatColor = newValue;
        }
        Tags.Api.TagsUpdatedPost(player, playerData);
    }

    public static string? GetAttribute(this CCSPlayerController player, TagType type)
    {
        Tag playerData = PlayerTagsList[player];
        return type switch
        {
            TagType.ScoreTag => playerData.ScoreTag,
            TagType.ChatTag => playerData.ChatTag,
            TagType.NameColor => playerData.NameColor,
            TagType.ChatColor => playerData.ChatColor,
            _ => null
        };
    }

    public static void ResetAttribute(this CCSPlayerController player, TagType types)
    {
        Tag playerData = PlayerTagsList[player];
        Tags.Api.TagsUpdatedPre(player, playerData);
        Tag defaultTag = GetTag(player);

        if ((types & TagType.ScoreTag) != 0)
        {
            playerData.ScoreTag = defaultTag.ScoreTag;
            player.SetScoreTag(defaultTag.ScoreTag);
        }
        if ((types & TagType.ChatTag) != 0)
        {
            playerData.ChatTag = defaultTag.ChatTag;
        }
        if ((types & TagType.NameColor) != 0)
        {
            playerData.NameColor = defaultTag.NameColor;
        }
        if ((types & TagType.ChatColor) != 0)
        {
            playerData.ChatColor = defaultTag.ChatColor;
        }

        Tags.Api.TagsUpdatedPost(player, playerData);
    }

    public static bool GetChatSound(this CCSPlayerController player) =>
        PlayerTagsList[player].ChatSound;

    public static void SetChatSound(this CCSPlayerController player, bool value)
    {
        Tag playerData = PlayerTagsList[player];
        Tags.Api.TagsUpdatedPre(player, playerData);
        playerData.ChatSound = value;
        Tags.Api.TagsUpdatedPost(player, playerData);
    }

    public static bool GetVisibility(this CCSPlayerController player) =>
        PlayerTagsList[player].Visibility;

    public static void SetVisibility(this CCSPlayerController player, bool value)
    {
        Tag playerData = PlayerTagsList[player];
        Tags.Api.TagsUpdatedPre(player, playerData);
        playerData.Visibility = value;
        player.SetScoreTag(value ? player.GetAttribute(TagType.ScoreTag) : Config.DefaultTags.ScoreTag);
        Tags.Api.TagsUpdatedPost(player, playerData);
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
}
