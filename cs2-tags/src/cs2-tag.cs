using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using static Tags.Config_Config;
using static TagsApi.Tags;

namespace Tags;

public class Tags : BasePlugin
{
    public override string ModuleName => "Tags";
    public override string ModuleVersion => "0.0.6";
    public override string ModuleAuthor => "schwarper";

    public class PlayerData
    {
        public Tag PlayerTag { get; set; } = null!;
        public bool ToggleTags { get; set; }
    }

    public static Dictionary<ulong, PlayerData> PlayerDataList { get; set; } = [];

    public override void Load(bool hotReload)
    {
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
        CCSPlayerController? player = @event.Userid;

        if (player == null || player.IsBot)
        {
            return HookResult.Continue;
        }

        PlayerDataList.Add(player.SteamID, new PlayerData
        {
            PlayerTag = GetTag(player),
            ToggleTags = true
        });

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null || player.IsBot)
        {
            return HookResult.Continue;
        }

        PlayerDataList.Remove(player.SteamID);
        return HookResult.Continue;
    }

    public static HookResult OnMessage(UserMessage um)
    {
        int entityIndex = um.ReadInt("entityindex");

        CCSPlayerController? player = Utilities.GetPlayerFromIndex(entityIndex);

        if (player == null)
        {
            return HookResult.Continue;
        }

        if (!PlayerDataList.TryGetValue(player.SteamID, out var playerData))
        {
            return HookResult.Continue;
        }

        if (!playerData.ToggleTags)
        {
            playerData.PlayerTag = Config.DefaultTags;
        }

        string msgT = um.ReadString("messagename");
        string playername = um.ReadString("param1");
        string message = um.ReadString("param2");

        string cleanedMessage = RemoveCurlyBraceContent(message);

        if (string.IsNullOrWhiteSpace(cleanedMessage))
        {
            return HookResult.Handled;
        }

        bool isTeamMessage = !msgT.Contains("All");

        string deadname = player.PawnIsAlive ? string.Empty : Config.Settings.DeadName;
        string teamname = isTeamMessage ? TeamName(player.Team) : string.Empty;
        string tag = playerData.PlayerTag.ChatTag;
        string namecolor = playerData.PlayerTag.NameColor;
        string chatcolor = playerData.PlayerTag.ChatColor;

        string formattedMessage = FormatMessage(deadname, teamname, tag, namecolor, chatcolor, playername, cleanedMessage, player.Team);
        um.SetString("messagename", formattedMessage);
        return HookResult.Changed;
    }

    private static string RemoveCurlyBraceContent(string message)
    {
        return System.Text.RegularExpressions.Regex.Replace(message, @"\{.*?\}", string.Empty);
    }

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

        if (!PlayerDataList.TryGetValue(player.SteamID, out var playerData))
        {
            return;
        }

        var value = playerData.ToggleTags;

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
        foreach (var kvp in PlayerDataList)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSteamId(kvp.Key);

            if (player == null)
            {
                continue;
            }

            var scoretag = kvp.Value.PlayerTag.ScoreTag;

            if (string.IsNullOrEmpty(scoretag) || player.Clan == scoretag)
            {
                continue;
            }

            player.Clan = scoretag;
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");

            var fakeEvent = new EventNextlevelChanged(false);
            fakeEvent.FireEventToClient(player);
        }
    }
}
