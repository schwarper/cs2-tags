using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.UserMessages;
using System.Collections.Concurrent;
using TagsApi;
using static Tags.Config_Config;
using static Tags.TagsLibrary;
using static TagsApi.Tags;

namespace Tags;

public partial class Tags : BasePlugin
{
    public override string ModuleName => "Tags";
    public override string ModuleVersion => "1.0";
    public override string ModuleAuthor => "schwarper";

    public static ConcurrentDictionary<ulong, Tag> PlayerTagsList { get; set; } = [];
    public static HashSet<ulong> PlayerToggleTagsList { get; set; } = [];
    public TagsAPI Api { get; set; } = null!;

    public override void Load(bool hotReload)
    {
        Api = new TagsAPI();
        Capabilities.RegisterPluginCapability(ITagApi.Capability, () => Api);

        AddCommandListener("css_admins_reload", AdminsReload, HookMode.Pre);

        HookUserMessage(118, OnMessage, HookMode.Pre);

        AddTimer(10.0f, UpdateTags, TimerFlags.REPEAT);

        if (hotReload)
        {
            Reload();
        }
        else
        {
            Config_Config.Load();
        }
    }

    public override void Unload(bool hotReload)
    {
        RemoveCommandListener("css_admins_reload", AdminsReload, HookMode.Pre);
    }

    public static HookResult AdminsReload(CCSPlayerController? player, CommandInfo info)
    {
        Reload();
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player || player.IsBot)
        {
            return HookResult.Continue;
        }

        player.LoadTag();
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player || player.IsBot)
        {
            return HookResult.Continue;
        }

        var steamid = player.SteamID;
        PlayerTagsList.TryRemove(steamid, out _);
        PlayerToggleTagsList.Remove(steamid);
        return HookResult.Continue;
    }

    public HookResult OnMessage(UserMessage um)
    {
        int entityIndex = um.ReadInt("entityindex");

        if (Utilities.GetPlayerFromIndex(entityIndex) is not CCSPlayerController player || player.IsBot)
        {
            return HookResult.Continue;
        }

        var steamid = player.SteamID;
        Tag playerData;

        if (PlayerToggleTagsList.Contains(steamid))
        {
            playerData = Config.DefaultTags;
        }
        else
        {
            playerData = PlayerTagsList[steamid];
        }

        Api.PlayerChat(um);

        string msgT = um.ReadString("messagename");
        string playername = um.ReadString("param1");
        string message = um.ReadString("param2");

        string cleanedMessage = message.RemoveCurlyBraceContent();

        if (string.IsNullOrWhiteSpace(cleanedMessage))
        {
            return HookResult.Handled;
        }

        bool isTeamMessage = !msgT.Contains("All");

        string deadname = player.PawnIsAlive ? string.Empty : Config.Settings.DeadName;
        string teamname = isTeamMessage ? player.Team.Name() : string.Empty;
        string tag = playerData.ChatTag;
        string namecolor = playerData.NameColor;
        string chatcolor = playerData.ChatColor;

        string formattedMessage = TagsLibrary.FormatMessage(deadname, teamname, tag, namecolor, chatcolor, playername, cleanedMessage, player.Team);
        um.SetString("messagename", formattedMessage);
        um.SetBool("chat", playerData.ChatSound);

        return HookResult.Changed;
    }

    [ConsoleCommand("css_tags_reload")]
    [RequiresPermissions("@css/root")]
    public void Command_Tags_Reload(CCSPlayerController? player, CommandInfo info)
    {
        Reload();
        info.ReplyToCommand("[cs2-tags] Tags are reloaded.");
    }

    [ConsoleCommand("css_toggletags")]
    [RequiresPermissions("@css/admin")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_Toggletags(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            return;
        }

        bool value = PlayerToggleTagsList.Add(player.SteamID);

        if (value)
        {
            info.ReplyToCommand("[cs2-tags] Tags are now hidden.");
        }
        else
        {
            PlayerToggleTagsList.Remove(player.SteamID);
            info.ReplyToCommand("[cs2-tags] Tags are now visible.");
        }
    }

    public static void UpdateTags()
    {
        var players = GetPlayers();

        foreach ((ulong steamid, Tag tag) in PlayerTagsList)
        {
            if (players.SingleOrDefault(p => p.SteamID == steamid) is not CCSPlayerController player)
            {
                continue;
            }

            string scoretag;

            if (PlayerToggleTagsList.Contains(steamid))
            {
                scoretag = Config.DefaultTags.ScoreTag;
            }
            else
            {
                scoretag = tag.ScoreTag;
            }

            if (player.Clan != scoretag)
            {
                player.SetTag(scoretag);
            }
        }
    }
}