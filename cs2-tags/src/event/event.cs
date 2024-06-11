using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static Tags.Tags;
using static Tags.TagsAPI;
using static TagsApi.Tags;

namespace Tags;

public static class Event
{
    public static void Load()
    {
        Instance.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        Instance.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        Instance.AddCommandListener("say", OnPlayerChat, HookMode.Pre);
        Instance.AddCommandListener("say_team", OnPlayerChat, HookMode.Pre);
        Instance.RegisterListener<OnTick>(OnTick);
    }

    public static void Unload()
    {
        Instance.RemoveCommandListener("say", OnPlayerChat, HookMode.Pre);
        Instance.RemoveCommandListener("say_team", OnPlayerChat, HookMode.Pre);
        Instance.RemoveListener<OnTick>(OnTick);
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

    public static HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || info.GetArg(1).Length == 0)
        {
            return HookResult.Continue;
        }

        string command = info.GetArg(1);

        if (CoreConfig.SilentChatTrigger.Any(i => command.StartsWith(i)))
        {
            return HookResult.Continue;
        }

        if (PlayerGagList.Contains(player.SteamID))
        {
            return HookResult.Handled;
        }

        if (CoreConfig.PublicChatTrigger.Any(i => command.StartsWith(i)))
        {
            return HookResult.Continue;
        }

        if (!Instance.PlayerTagDatas.TryGetValue(player.Slot, out Tag? playerData) || playerData == null)
        {
            return HookResult.Continue;
        }

        bool teammessage = info.GetArg(0) == "say_team";
        string deadname = player.PawnIsAlive ? string.Empty : Instance.Config.Settings["deadname"];
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
            foreach (CCSPlayerController target in Utilities.GetPlayers().Where(target => target.Team == player.Team && !target.IsBot))
            {
                target.PrintToChat(message);
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
        if (++Instance.GlobalTick != 200)
        {
            return;
        }

        Instance.GlobalTick = 0;

        foreach (CCSPlayerController player in Utilities.GetPlayers())
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