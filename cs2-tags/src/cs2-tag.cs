using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using System.Runtime.InteropServices;
using System.Text;
using static Tags.Config_Config;
using static TagsApi.Tags;
using static CounterStrikeSharp.API.Core.Listeners;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;

namespace Tags;

public class Tags : BasePlugin
{
    public override string ModuleName => "Tags";
    public override string ModuleVersion => "0.0.5";
    public override string ModuleAuthor => "schwarper";

    public static Dictionary<ulong, Tag> PlayerTags { get; set; } = [];
    public static Dictionary<ulong, bool> PlayerToggleTags { get; set; } = [];

    public List<nint> strPtrs = [];

    public override void Load(bool hotReload)
    {
        VirtualFunctions.UTIL_SayText2FilterFunc.Hook(OnSayTextPre, HookMode.Pre);
        VirtualFunctions.UTIL_SayText2FilterFunc.Hook(OnSayTextPost, HookMode.Post);

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

    public override void Unload(bool hotReload)
    {
        VirtualFunctions.UTIL_SayText2FilterFunc.Unhook(OnSayTextPre, HookMode.Pre);
        VirtualFunctions.UTIL_SayText2FilterFunc.Unhook(OnSayTextPost, HookMode.Post);
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        PlayerTags.Add(player.SteamID, GetTag(player));
        PlayerToggleTags.Add(player.SteamID, true);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null || !PlayerTags.ContainsKey(player.SteamID))
        {
            return HookResult.Continue;
        }

        PlayerTags.Remove(player.SteamID);
        PlayerToggleTags.Remove(player.SteamID);
        return HookResult.Continue;
    }

    public HookResult OnSayTextPre(DynamicHook hook)
    {
        CCSPlayerController player = hook.GetParam<CCSPlayerController>(1);

        if (!PlayerTags.TryGetValue(player.SteamID, out Tag? playerTag) || playerTag == null)
        {
            return HookResult.Continue;
        }

        string msgT = hook.GetParam<string>(3);
        string playername = hook.GetParam<string>(4);
        string message = hook.GetParam<string>(5);

        bool isTeamMessage = !msgT.Contains("All");

        string deadname = player.PawnIsAlive ? string.Empty : Config.Settings.DeadName;
        string teamname = isTeamMessage ? TeamName(player.Team) : string.Empty;
        string tag = playerTag.ChatTag;
        string namecolor = playerTag.NameColor;
        string chatcolor = playerTag.ChatColor;

        string formattedMessage = FormatMessage(deadname, teamname, tag, namecolor, chatcolor, playername, message, player.Team);
        hook.SetParam(3, AllocString(formattedMessage));
        return HookResult.Changed;

        static string ReplaceTags(string message, CsTeam team)
        {
            string modifiedValue = StringExtensions.ReplaceColorTags(message)
                .Replace("{TeamColor}", ChatColors.ForTeam(team).ToString());

            return modifiedValue;
        }

        static string TeamName(CsTeam team)
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

        static string FormatMessage(string deadIcon, string teamname, string tag, string namecolor, string chatcolor, string playername, string message, CsTeam team)
        {
            return ReplaceTags($" {deadIcon}{teamname}{tag}{namecolor}{playername}{ChatColors.Default}: {chatcolor}{message}", team);
        }
    }

    public HookResult OnSayTextPost(DynamicHook hook)
    {
        foreach (nint ptr in strPtrs)
        {
            Marshal.FreeHGlobal(ptr);
        }

        strPtrs.Clear();
        return HookResult.Continue;
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

        if (!PlayerToggleTags.TryGetValue(player.SteamID, out bool value))
        {
            return;
        }

        if (value)
        {
            PlayerTags[player.SteamID] = Config.DefaultTags;
            PlayerToggleTags[player.SteamID] = false;

            info.ReplyToCommand("[cs2-tags] Toggletags is false");
        }
        else
        {
            PlayerTags[player.SteamID] = GetTag(player);
            PlayerToggleTags[player.SteamID] = true;

            info.ReplyToCommand("[cs2-tags] Toggletags is true");
        }
    }

    public static void UpdateTags()
    {
        foreach (var kvp in PlayerTags)
        {
            var player = Utilities.GetPlayerFromSteamId(kvp.Key)!;
            var tag = kvp.Value;

            if (string.IsNullOrEmpty(tag.ScoreTag))
            {
                continue;
            }

            player.Clan = tag.ScoreTag;
        }
    }

    public nint AllocString(string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str + "\0");
        nint stringPtr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, stringPtr, bytes.Length);

        strPtrs.Add(stringPtr);

        return stringPtr;
    }
}
