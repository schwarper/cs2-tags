using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using CS2MenuManager.API.Menu;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static class Menu
{
    private readonly static Type _menuType;

    static Menu()
    {
        Dictionary<string, Type> menuTypes = new()
        {
            { "ChatMenu", typeof(ChatMenu) },
            { "ConsoleMenu", typeof(ConsoleMenu) },
            { "CenterHtmlMenu", typeof(CenterHtmlMenu) },
            { "WasdMenu", typeof(WasdMenu) },
            { "ScreenMenu", typeof(ScreenMenu) },
        };

        _menuType = menuTypes.TryGetValue(Instance.Config.Settings.MenuType, out Type? menuType) ? menuType : typeof(CenterHtmlMenu);
    }

    public static IMenu MenuByType(string title)
    {
        return _menuType switch
        {
            Type t when t == typeof(ChatMenu) => new ChatMenu(title, Instance),
            Type t when t == typeof(ConsoleMenu) => new ConsoleMenu(title, Instance),
            Type t when t == typeof(CenterHtmlMenu) => new CenterHtmlMenu(title, Instance),
            Type t when t == typeof(WasdMenu) => new WasdMenu(title, Instance),
            Type t when t == typeof(ScreenMenu) => new ScreenMenu(title, Instance) { ShowResolutionsOption = false },
            _ => new CenterHtmlMenu(title, Instance)
        };
    }

    public static IMenu MainMenu(CCSPlayerController player)
    {
        Tag playerData = PlayerTagsList[player.SteamID];

        IMenu menu = MenuByType(Instance.Localizer.ForPlayer(player, "Main Menu"));

        menu.AddItem(Instance.Localizer.ForPlayer(player, "Score Tag"), (p, o) => SubMenuFirst(p, playerData, TagType.ScoreTag, o.Text).Display(p, 0));
        menu.AddItem(Instance.Localizer.ForPlayer(player, "Chat Tag"), (p, o) => SubMenuFirst(p, playerData, TagType.ChatTag, o.Text).Display(p, 0));
        menu.AddItem(Instance.Localizer.ForPlayer(player, "Chat Color"), (p, o) => SubMenuFirst(p, playerData, TagType.ChatColor, o.Text).Display(p, 0));
        menu.AddItem(Instance.Localizer.ForPlayer(player, "Name Color"), (p, o) => SubMenuFirst(p, playerData, TagType.NameColor, o.Text).Display(p, 0));

        if (AdminManager.PlayerHasPermissions(new SteamID(player.SteamID), "@css/admin"))
        {
            bool visibility = playerData.Visibility;
            string visibilityText = $"{Instance.Localizer.ForPlayer(player, "Visibility")} [{(visibility ? "✔️" : "❌")}]";

            menu.AddItem(visibilityText, (p, o) =>
            {
                player.SetVisibility(!visibility);
                player.PrintToChat(Instance.Config.Settings.Tag + Instance.Localizer.ForPlayer(player, visibility ? "Tags are now hidden" : "Tags are now visible"));
                MainMenu(p).Display(p, 0);
            });
        }

        return menu;
    }

    public static IMenu SubMenuFirst(CCSPlayerController player, Tag playerData, TagType type, string title)
    {
        IMenu menu = MenuByType($"{title} {Instance.Localizer.ForPlayer(player, "Selection")}");

        string currentValue = GetTagValue(playerData, type)?.ToString() ?? string.Empty;
        menu.AddItem($"{Instance.Localizer.ForPlayer(player, "Current")} {currentValue.ConvertToHtml(player.Team, _menuType == typeof(CenterHtmlMenu))}", DisableOption.DisableHideNumber);

        List<Tag> tags = player.GetTags();
        HashSet<string> tagsHash = [];

        foreach (Tag tag in tags)
        {
            if (GetTagValue(tag, type) is not string tagValue || string.IsNullOrEmpty(tagValue))
                continue;

            if (!tagsHash.Add(tagValue))
                continue;

            string tagValueHtml = tagValue.ConvertToHtml(player.Team, _menuType == typeof(CenterHtmlMenu));
            DisableOption option = tagValue == currentValue ? DisableOption.DisableHideNumber : DisableOption.None;
            menu.AddItem(tagValueHtml, (p, o) => SubMenuSecond(p, playerData, type, tagValue, tagValueHtml, title).Display(p, 0), option);
        }

        menu.PrevMenu = MainMenu(player);
        return menu;
    }

    public static IMenu SubMenuSecond(CCSPlayerController player, Tag playerData, TagType type, string selected, string selectedhtml, string title)
    {
        IMenu menu = MenuByType($"{Instance.Localizer.ForPlayer(player, "Modify")} {title}");

        menu.AddItem($"{Instance.Localizer.ForPlayer(player, "Selected")} {selectedhtml}", DisableOption.DisableHideNumber);
        menu.AddItem(Instance.Localizer.ForPlayer(player, "Select"), (p, o) =>
        {
            SetPlayerTagValue(p, playerData, type, selected);

            string value = type == TagType.NameColor || type == TagType.ChatColor ?
                $"{selected.ReplaceTags(player.Team)}{selected.Split(['{', '}'], StringSplitOptions.None)[1]}" :
                selected.ReplaceTags(player.Team);

            player.PrintToChat(Instance.Config.Settings.Tag + Instance.Localizer.ForPlayer(player, "Selected option", title, value));
            MainMenu(p).Display(p, 0);
        });

        menu.PrevMenu = MainMenu(player);
        return menu;
    }
}