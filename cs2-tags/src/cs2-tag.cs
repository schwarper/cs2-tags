using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using TagsApi;
using static Tags.Menu;
using static Tags.TagsLibrary;
using static TagsApi.Tags;

namespace Tags;

public class Tags : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Tags";
    public override string ModuleVersion => "1.4";
    public override string ModuleAuthor => "schwarper";

    public Config Config { get; set; } = new();
    public static readonly Dictionary<ulong, Tag> PlayerTagsList = [];
    public static readonly TagsAPI Api = new();
    public static Tags Instance { get; set; } = new();

    public override void Load(bool hotReload)
    {
        Instance = this;
        Capabilities.RegisterPluginCapability(ITagApi.Capability, () => Api);
        HookUserMessage(118, OnMessage, HookMode.Pre);

        foreach (string command in Config.Commands.TagsReload)
            AddCommand(command, "Tags Reload", Command_Tags_Reload);

        foreach (string command in Config.Commands.Visibility)
            AddCommand(command, "Visibility", Command_Visibility);

        foreach (string command in Config.Commands.TagsMenu)
            AddCommand(command, "Tags Menu", Command_Tags);

        AddCommandListener("css_admins_reload", Command_Admins_Reloads, HookMode.Pre);

        if (hotReload)
        {
            List<CCSPlayerController> players = Utilities.GetPlayers();
            foreach (CCSPlayerController player in players)
            {
                if (player.IsBot)
                    continue;

                if (!PlayerTagsList.ContainsKey(player.SteamID))
                {
                    Tag defaultTag = player.GetTag();
                    PlayerTagsList[player.SteamID] = defaultTag;
                }

                Database.LoadPlayer(player);
            }
        }
    }

    public override void Unload(bool hotReload)
    {
        UnhookUserMessage(118, OnMessage, HookMode.Pre);
        RemoveCommandListener("css_admins_reload", Command_Admins_Reloads, HookMode.Pre);
    }

    public void OnConfigParsed(Config config)
    {
        UpdateConfig(config);
        Config = config;
    }

    public static void UpdateConfig(Config config)
    {
        if (config.DatabaseConnection.MySQL)
        {
            HashSet<string> keys =
            [
                config.DatabaseConnection.Host,
                config.DatabaseConnection.Name,
                config.DatabaseConnection.Password,
                config.DatabaseConnection.User
            ];

            foreach (string key in keys)
            {
                if (string.IsNullOrEmpty(key))
                    throw new Exception($"Database credentials are missing");
            }
        }

        Database.CreateDatabase(config);

        config.Settings.Tag = config.Settings.Tag.ReplaceColorTags();

        config.Settings.TeamNames[CsTeam.None] = config.Settings.NoneName.ReplaceTags(CsTeam.None);
        config.Settings.TeamNames[CsTeam.Spectator] = config.Settings.SpecName.ReplaceTags(CsTeam.Spectator);
        config.Settings.TeamNames[CsTeam.Terrorist] = config.Settings.TName.ReplaceTags(CsTeam.Terrorist);
        config.Settings.TeamNames[CsTeam.CounterTerrorist] = config.Settings.CTName.ReplaceTags(CsTeam.CounterTerrorist);
    }

    public HookResult Command_Admins_Reloads(CCSPlayerController? player, CommandInfo info)
    {
        Config.Reload();
        UpdateConfig(Config);
        return HookResult.Continue;
    }

    [RequiresPermissions("@css/root")]
    public void Command_Tags_Reload(CCSPlayerController? player, CommandInfo info)
    {
        Config.Reload();
        UpdateConfig(Config);
    }

    [RequiresPermissions("@css/admin")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_Visibility(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            return;
        }

        if (player.GetVisibility())
        {
            player.SetVisibility(false);
            info.ReplyToCommand(Config.Settings.Tag + Localizer.ForPlayer(player, "Tags are now hidden"));
        }
        else
        {
            player.SetVisibility(true);
            info.ReplyToCommand(Config.Settings.Tag + Localizer.ForPlayer(player, "Tags are now visible"));
        }
    }

    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_Tags(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;

        MainMenu(player).Display(player, 0);
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player || player.IsBot)
            return HookResult.Continue;

        Database.LoadPlayer(player);
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Pre)]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player || player.IsBot)
            return HookResult.Continue;

        Database.SavePlayer(player);
        return HookResult.Continue;
    }

    public HookResult OnMessage(UserMessage um)
    {
        if (Utilities.GetPlayerFromIndex(um.ReadInt("entityindex")) is not CCSPlayerController player || player.IsBot)
            return HookResult.Continue;

        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? tag))
        {
            tag = player.GetTag();
            PlayerTagsList[player.SteamID] = tag;

            Database.LoadPlayer(player);
        }

        MessageProcess messageProcess = new()
        {
            Player = player,
            Tag = !player.GetVisibility() ? Config.Default.Clone() : tag.Clone(),
            Message = um.ReadString("param2").RemoveCurlyBraceContent(),
            PlayerName = um.ReadString("param1"),
            ChatSound = um.ReadBool("chat"),
            TeamMessage = !um.ReadString("messagename").Contains("All")
        };

        if (string.IsNullOrEmpty(messageProcess.Message))
            return HookResult.Handled;

        HookResult hookResult = Api.MessageProcessPre(messageProcess);

        if (hookResult >= HookResult.Handled)
            return hookResult;

        string deadname = player.PawnIsAlive ? string.Empty : Config.Settings.DeadName;
        string teamname = messageProcess.TeamMessage ? player.Team.Name() : string.Empty;

        Tag playerData = messageProcess.Tag;

        CsTeam team = player.Team;
        messageProcess.PlayerName = FormatMessage(team, deadname, teamname, playerData.ChatTag ?? string.Empty, playerData.NameColor ?? string.Empty, messageProcess.PlayerName);
        messageProcess.Message = FormatMessage(team, playerData.ChatColor ?? string.Empty, messageProcess.Message);

        hookResult = Api.MessageProcess(messageProcess);

        if (hookResult >= HookResult.Handled)
            return hookResult;

        um.SetString("messagename", $"{messageProcess.PlayerName}{ChatColors.White}: {messageProcess.Message}");
        um.SetBool("chat", playerData.ChatSound);

        Api.MessageProcessPost(messageProcess);

        return HookResult.Changed;
    }

    internal static string? GetTagValue(Tag tag, TagType type)
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

    internal static void SetPlayerTagValue(CCSPlayerController player, Tag playerData, TagType type, object value)
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