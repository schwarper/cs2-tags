using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static Tag.Tag;
using static TagApi.Tag;

namespace Tag;

public static class Event
{
    public static void Load()
    {
        Instance.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        Instance.AddCommandListener("say", OnPlayerChat);
        Instance.AddCommandListener("say_team", OnPlayerChat);
        Instance.RegisterListener<OnTick>(OnTick);
    }

    public static HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;

        if (player == null || !player.IsValid)
        {
            return HookResult.Continue;
        }

        Instance.PlayerToggleTags[player.Slot] = true;
        Instance.PlayerDatas[player.Slot] = GetTag(player);

        return HookResult.Continue;
    }

    public static HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || info.GetArg(1).Length == 0)
        {
            return HookResult.Continue;
        }

        string command = info.GetArg(1);

        if (Json.PublicChatTrigger.Any(i => command.StartsWith(i)))
        {
            player.ExecuteClientCommandFromServer($"css_{command.Substring(1)}");
        }

        if (Json.SilentChatTrigger.Any(i => command.StartsWith(i)))
        {
            return HookResult.Handled;
        }

        CTag playerData = Instance.PlayerDatas[player.Slot]!;

        string deadname = player.PawnIsAlive ? string.Empty : Instance.Config.Settings["deadname"];
        bool teammessage = info.GetArg(0) == "say_team";
        string tag = playerData.ChatTag;
        string namecolor = playerData.NameColor;
        string chatcolor = playerData.ChatColor;

        string message = FormatMessage(deadname, teammessage ? Lib.TeamName(player.Team) : string.Empty, tag, namecolor, chatcolor, player, command);

        static string FormatMessage(string deadIcon, string teamname, string tag, string namecolor, string chatcolor, CCSPlayerController player, string text)
        {
            return Lib.ReplaceTags($" {deadIcon}{teamname}{tag}{namecolor}{player.PlayerName}{ChatColors.Default}: {chatcolor}{text}", player.Team);
        }

        if (info.GetArg(0) == "say_team")
        {
            foreach (CCSPlayerController target in Utilities.GetPlayers().Where(target => target.Team == player.Team && target.IsValid && !target.IsBot))
            {
                player.PrintToChat(message);
            }
        }
        else
        {
            Server.PrintToChatAll(message);
        }

        return HookResult.Handled;
    }

    public static void OnTick()
    {
        Instance.GlobalTick++;

        if (Instance.GlobalTick % 200 != 0)
        {
            return;
        }

        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            string playerclan = Instance.PlayerDatas[player.Slot].ScoreTag;

            if (playerclan == string.Empty)
            {
                continue;
            }

            player.Clan = playerclan;

            //string playername = player.PlayerName;
            //player.PlayerName = playername + ' ';

            Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");
            Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
        }
    }
}