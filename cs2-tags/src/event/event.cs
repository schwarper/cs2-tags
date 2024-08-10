using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static class Event
{
    public static void Load()
    {
        Instance.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        Instance.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        Instance.RegisterListener<OnTick>(OnTick);

        VirtualFunctions.UTIL_SayText2FilterFunc.Hook(OnSayText2Filter, HookMode.Pre);
    }

    public static void Unload()
    {
        Instance.RemoveListener<OnTick>(OnTick);
        VirtualFunctions.UTIL_SayText2FilterFunc.Unhook(OnSayText2Filter, HookMode.Pre);
    }

    public static HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        Instance.PlayerTagDatas.Add(player.Slot, GetTag(player));
        Instance.PlayerToggleTags.Add(player.Slot, true);

        return HookResult.Continue;
    }

    public static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        if (Instance.PlayerTagDatas.TryGetValue(player.Slot, out _))
        {
            Instance.PlayerTagDatas.Remove(player.Slot);
            Instance.PlayerToggleTags.Remove(player.Slot);
        }

        return HookResult.Continue;
    }

    public static HookResult OnSayText2Filter(DynamicHook hook)
    {
        static string FormatMessage(string deadIcon, string teamname, string tag, string namecolor, string chatcolor, string playername, string message, CsTeam team)
        {
            return Lib.ReplaceTags($" {deadIcon}{teamname}{tag}{namecolor}{playername}{ChatColors.Default}: {chatcolor}{message}", team);
        }

        CCSPlayerController player = hook.GetParam<CCSPlayerController>(1);

        if (!Instance.PlayerTagDatas.TryGetValue(player.Slot, out Tag? playerData))
        {
            return HookResult.Continue;
        }

        string msgT = hook.GetParam<string>(3);
        string playername = hook.GetParam<string>(4);
        string message = hook.GetParam<string>(5);

        bool isTeamMessage = !msgT.Contains("All");

        string deadname = player.PawnIsAlive ? string.Empty : Instance.Config.Settings["deadname"];
        string teamname = isTeamMessage ? Lib.TeamName(player.Team) : string.Empty;
        string tag = playerData.ChatTag;
        string namecolor = playerData.NameColor;
        string chatcolor = playerData.ChatColor;

        string formattedMessage = FormatMessage(deadname, teamname, tag, namecolor, chatcolor, playername, message, player.Team);

        hook.SetParam(3, formattedMessage);

        return HookResult.Continue;
    }

    public static void OnTick()
    {
        if (++Instance.GlobalTick != 200)
        {
            return;
        }

        Instance.GlobalTick = 0;

        var players = Utilities.GetPlayers();

        foreach (CCSPlayerController player in players)
        {
            if (!Instance.PlayerTagDatas.TryGetValue(player.Slot, out Tag? tag) || tag == null || tag.ScoreTag == string.Empty)
            {
                continue;
            }

            player.Clan = tag.ScoreTag;

            //string playername = player.PlayerName;
            //player.PlayerName = playername + ' ';

            //Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");
            //Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
        }
    }
}
