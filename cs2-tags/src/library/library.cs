using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text;
using System.Text.RegularExpressions;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static partial class TagsLibrary
{
    private static readonly Dictionary<string, string> HtmlColorList = new(StringComparer.OrdinalIgnoreCase)
    {
        { "White", "#FFFFFF" },
        { "DarkRed", "#8B0000" },
        { "Green", "#008000" },
        { "LightYellow", "#FFFFE0" },
        { "LightBlue", "#ADD8E6" },
        { "Olive", "#808000" },
        { "Lime", "#00FF00" },
        { "Red", "#FF0000" },
        { "LightPurple", "#9370DB" },
        { "Purple", "#800080" },
        { "Grey", "#808080" },
        { "Yellow", "#FFFF00" },
        { "Gold", "#FFD700" },
        { "Silver", "#C0C0C0" },
        { "Blue", "#0000FF" },
        { "DarkBlue", "#00008B" },
        { "BlueGrey", "#7393B3" },
        { "Magenta", "#FF00FF" },
        { "LightRed", "#FF6347" },
        { "Orange", "#FFA500" }
    };

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

    public static Tag GetTag(this CCSPlayerController player)
    {
        string steamId = player.SteamID.ToString();

        Tag? steamIdTag = Instance.Config.Tags.FirstOrDefault(tag => steamId == tag.Role)?.Clone();
        if (steamIdTag != null)
            return steamIdTag;

        SteamID steamID = new(player.SteamID);

        List<Tag> matchingGroupTags = [.. Instance.Config.Tags
            .Where(tag => tag.Role != null && tag.Role[0] == '#' && AdminManager.PlayerInGroup(steamID, tag.Role))
            .Select(tag => tag.Clone())];

        if (matchingGroupTags.Count > 0)
            return matchingGroupTags[0];

        List<Tag> matchingPermissionTags = [.. Instance.Config.Tags
            .Where(tag => tag.Role != null && tag.Role[0] == '@' && AdminManager.PlayerHasPermissions(steamID, tag.Role))
            .Select(tag => tag.Clone())];

        return matchingPermissionTags.Count > 0 ? matchingPermissionTags[0] : Instance.Config.Default.Clone();
    }

    public static List<Tag> GetTags(this CCSPlayerController player)
    {
        string steamId = player.SteamID.ToString();
        SteamID steamID = new(player.SteamID);

        return
        [
            Instance.Config.Default.Clone(),
            .. Instance.Config.Tags
                .Where(tag =>
                    tag.Role == steamId ||
                    (tag.Role ?[0] == '#' && AdminManager.PlayerInGroup(steamID, tag.Role)) ||
                    (tag.Role ?[0] == '@' && AdminManager.PlayerHasPermissions(steamID, tag.Role))
                )
                .Select(tag => tag.Clone()),
        ];
    }

    public static void UpdateTag(this CCSPlayerController player, Tag tag)
    {
        PlayerTagsList[player.SteamID] = tag;
        player.SetScoreTag(tag.ScoreTag);
        Tags.Api.TagsUpdatedPost(player, tag);
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
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }

        try
        {
            Tags.Api.TagsUpdatedPre(player, playerData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TagsUpdatedPre event: {ex.Message}");
        }

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

        try
        {
            Tags.Api.TagsUpdatedPost(player, playerData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TagsUpdatedPost event: {ex.Message}");
        }
    }

    public static void SetAttribute(this CCSPlayerController player, TagType types, string newValue)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }

        try
        {
            Tags.Api.TagsUpdatedPre(player, playerData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TagsUpdatedPre event: {ex.Message}");
        }

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

        try
        {
            Tags.Api.TagsUpdatedPost(player, playerData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TagsUpdatedPost event: {ex.Message}");
        }
    }

    public static string? GetAttribute(this CCSPlayerController player, TagType type)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }

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
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
            return;
        }

        try
        {
            Tags.Api.TagsUpdatedPre(player, playerData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TagsUpdatedPre event: {ex.Message}");
        }

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

        try
        {
            Tags.Api.TagsUpdatedPost(player, playerData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TagsUpdatedPost event: {ex.Message}");
        }
    }

    public static bool GetChatSound(this CCSPlayerController player)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
            return playerData.ChatSound;

        Tag defaultTag = player.GetTag();
        PlayerTagsList[player.SteamID] = defaultTag;
        return defaultTag.ChatSound;
    }

    public static void SetChatSound(this CCSPlayerController player, bool value)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }

        Tags.Api.TagsUpdatedPre(player, playerData);
        playerData.ChatSound = value;
        Tags.Api.TagsUpdatedPost(player, playerData);
    }

    public static bool GetVisibility(this CCSPlayerController player)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
            return playerData.Visibility;

        Tag defaultTag = player.GetTag();
        PlayerTagsList[player.SteamID] = defaultTag;
        return defaultTag.Visibility;
    }

    public static void SetVisibility(this CCSPlayerController player, bool value)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }

        Tags.Api.TagsUpdatedPre(player, playerData);
        playerData.Visibility = value;
        player.SetScoreTag(value ? player.GetAttribute(TagType.ScoreTag) : Instance.Config.Default.ScoreTag);
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

    public static string ConvertToHtml(this string message, CsTeam team, bool htmlmenu)
    {
        if (!htmlmenu)
            return message;

        string modifiedValue = message.Replace("{TeamColor}", ForTeamHtml(team));
        StringBuilder result = new();
        string[] parts = modifiedValue.Split(['{', '}'], StringSplitOptions.None);

        for (int i = 0; i < parts.Length; i++)
        {
            if (i % 2 == 0)
            {
                result.Append(parts[i]);
            }
            else
            {
                string fieldName = parts[i];
                if (HtmlColorList.TryGetValue(fieldName, out string? value))
                {
                    result.Append($"<font color='{value}'>");

                    if (parts.Length == 3 && string.IsNullOrEmpty(parts[0]) && string.IsNullOrEmpty(parts[2]))
                    {
                        result.Append(fieldName);
                    }
                    else
                    {
                        if (i + 1 < parts.Length)
                        {
                            result.Append(parts[i + 1]);
                        }
                    }

                    result.Append("</font>");
                    i++;
                }
                else
                {
                    result.Append($"{{{parts[i]}}}");
                }
            }
        }
        return result.ToString();
    }

    public static string ForTeamHtml(this CsTeam team)
    {
        return team switch
        {
            CsTeam.Spectator => "{LightPurple}",
            CsTeam.CounterTerrorist => "{LightBlue}",
            CsTeam.Terrorist => "{LightYellow}",
            _ => "{White}"
        };
    }

}