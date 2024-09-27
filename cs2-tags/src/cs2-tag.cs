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
using static TagsApi.Tags;

namespace Tags;

public partial class Tags : BasePlugin
{
    public override string ModuleName => "Tags";
    public override string ModuleVersion => "0.0.8";
    public override string ModuleAuthor => "schwarper";

    public class PlayerData
    {
        public Tag PlayerTag { get; set; } = null!;
        public bool ToggleTags { get; set; }
    }

    public static ConcurrentDictionary<ulong, PlayerData> PlayerDataList { get; set; } = [];
    public TagsAPI Api { get; set; } = null!;

    public override void Load(bool hotReload)
    {
        Api = new TagsAPI();
        Capabilities.RegisterPluginCapability(ITagApi.Capability, () => Api);

        HookUserMessage(118, OnMessage, HookMode.Pre);

        AddTimer(5.0f, UpdateTags, TimerFlags.REPEAT);

        if (hotReload)
        {
            Reload();
        }
        else
        {
            Config_Config.Load();
        }
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

        PlayerDataList.TryRemove(player.SteamID, out _);
        return HookResult.Continue;
    }

    public HookResult OnMessage(UserMessage um)
    {
        int entityIndex = um.ReadInt("entityindex");

        CCSPlayerController? player = Utilities.GetPlayerFromIndex(entityIndex);

        if (player == null || player.IsBot)
        {
            return HookResult.Continue;
        }

        if (!PlayerDataList.TryGetValue(player.SteamID, out PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        if (!playerData.ToggleTags)
        {
            playerData.PlayerTag = Config.DefaultTags;
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
        string tag = playerData.PlayerTag.ChatTag;
        string namecolor = playerData.PlayerTag.NameColor;
        string chatcolor = playerData.PlayerTag.ChatColor;

        string formattedMessage = TagsLibrary.FormatMessage(deadname, teamname, tag, namecolor, chatcolor, playername, cleanedMessage, player.Team);
        um.SetString("messagename", formattedMessage);
        um.SetBool("chat", playerData.PlayerTag.ChatSound);

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

        if (!PlayerDataList.TryGetValue(player.SteamID, out PlayerData? playerData))
        {
            return;
        }

        bool value = playerData.ToggleTags;

        if (value)
        {
            playerData.ToggleTags = false;
            info.ReplyToCommand("[cs2-tags] Toggletags is false");
        }
        else
        {
            playerData.ToggleTags = true;
            info.ReplyToCommand("[cs2-tags] Toggletags is true");
        }
    }

    public static void UpdateTags()
    {
        foreach (KeyValuePair<ulong, PlayerData> kvp in PlayerDataList)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSteamId(kvp.Key);

            if (player == null)
            {
                continue;
            }

            string scoretag = kvp.Value.ToggleTags ? Config.DefaultTags.ScoreTag :
                kvp.Value.PlayerTag.ScoreTag;

            if (!string.IsNullOrEmpty(scoretag) && player.Clan != scoretag)
            {
                player.SetTag(scoretag);
            }
        }
    }
}