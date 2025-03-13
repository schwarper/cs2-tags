using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Menu;
using static Tags.ConfigManager;
using static TagsApi.Tags;

namespace Tags;

public partial class Tags
{
    public CenterHtmlMenu MainMenu(CCSPlayerController player)
    {
        Tag playerData = PlayerTagsList[player];

        CenterHtmlMenu menu = new(Localizer.ForPlayer(player, "Main Menu"), this);

        menu.AddMenuOption(Localizer.ForPlayer(player, "Score Tag"), (p, o) => SubMenuFirst(p, playerData, TagType.ScoreTag, o.Text).Open(p));
        menu.AddMenuOption(Localizer.ForPlayer(player, "Chat Tag"), (p, o) => SubMenuFirst(p, playerData, TagType.ChatTag, o.Text).Open(p));
        menu.AddMenuOption(Localizer.ForPlayer(player, "Chat Color"), (p, o) => SubMenuFirst(p, playerData, TagType.ChatColor, o.Text).Open(p));
        menu.AddMenuOption(Localizer.ForPlayer(player, "Name Color"), (p, o) => SubMenuFirst(p, playerData, TagType.NameColor, o.Text).Open(p));

        if (AdminManager.PlayerHasPermissions(player, "@css/admin"))
        {
            bool visibility = playerData.Visibility;
            string visibilityText = $"{Localizer.ForPlayer(player, "Visibility")} [{(visibility ? "✔️" : "❌")}]";

            menu.AddMenuOption(visibilityText, (p, o) =>
            {
                player.SetVisibility(!visibility);
                player.PrintToChat(Config.Settings.Tag + Localizer.ForPlayer(player, visibility ? "Tags are now hidden" : "Tags are now visible"));
                MainMenu(p).Open(p);
            });
        }

        return menu;
    }

    public CenterHtmlMenu SubMenuFirst(CCSPlayerController player, Tag playerData, TagType type, string title)
    {
        CenterHtmlMenu menu = new($"{title} {Localizer.ForPlayer(player, "Selection")}", this);

        string currentValue = GetTagValue(playerData, type)?.ToString() ?? string.Empty;
        menu.AddMenuOption($"{Localizer.ForPlayer(player, "Current")} {currentValue.ConvertToHtml(player.Team)}", (p, o) => { }, true);

        List<Tag> tags = player.GetTags();
        HashSet<string> tagsHash = [];

        foreach (Tag tag in tags)
        {
            if (GetTagValue(tag, type) is not string tagValue || string.IsNullOrEmpty(tagValue))
                continue;

            if (!tagsHash.Add(tagValue))
                continue;

            string tagValueHtml = tagValue.ConvertToHtml(player.Team);
            bool disable = tagValue == currentValue;
            menu.AddMenuOption(tagValueHtml, (p, o) => SubMenuSecond(p, playerData, type, tagValue, tagValueHtml, title).Open(p), disable);
        }

        return menu;
    }

    public CenterHtmlMenu SubMenuSecond(CCSPlayerController player, Tag playerData, TagType type, string selected, string selectedhtml, string title)
    {
        CenterHtmlMenu menu = new($"{Localizer.ForPlayer(player, "Modify")} {title}", this);

        menu.AddMenuOption($"{Localizer.ForPlayer(player, "Selected")} {selectedhtml}", (p, o) => { }, true);
        menu.AddMenuOption(Localizer.ForPlayer(player, "Select"), (p, o) =>
        {
            SetPlayerTagValue(p, playerData, type, selected);

            string value = type == TagType.NameColor || type == TagType.ChatColor ?
                $"{selected.ReplaceTags(player.Team)}{selected.Split(['{', '}'], StringSplitOptions.None)[1]}" :
                selected.ReplaceTags(player.Team);

            player.PrintToChat(Config.Settings.Tag + Localizer.ForPlayer(player, "Selected option", title, value));
            MainMenu(p).Open(p);
        });

        return menu;
    }

    private static string? GetTagValue(Tag tag, TagType type)
    {
        return type switch
        {
            TagType.ScoreTag => tag.ScoreTag,
            TagType.ChatTag => tag.ChatTag,
            TagType.ChatColor => tag.ChatColor,
            TagType.NameColor => tag.NameColor,
            _ => null
        };
    }

    private static void SetPlayerTagValue(CCSPlayerController player, Tag playerData, TagType type, object value)
    {
        switch (type)
        {
            case TagType.ScoreTag:
                playerData.ScoreTag = (string)value;
                player.SetScoreTag(playerData.ScoreTag);
                break;
            case TagType.ChatTag:
                playerData.ChatTag = (string)value;
                break;
            case TagType.ChatColor:
                playerData.ChatColor = (string)value;
                break;
            case TagType.NameColor:
                playerData.NameColor = (string)value;
                break;
        }
    }
}