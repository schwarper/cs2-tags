using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using TagsApi;
using static Tags.Config_Config;
using static Tags.TagsLibrary;
using static TagsApi.Tags;

namespace Tags;

public partial class Tags : BasePlugin
{
    public override string ModuleName => "Tags";
    public override string ModuleVersion => "1.1";
    public override string ModuleAuthor => "schwarper";

    public static Dictionary<CCSPlayerController, Tag> PlayerTagsList { get; set; } = [];
    public static HashSet<CCSPlayerController> PlayerToggleTagsList { get; set; } = [];
    public static TagsAPI Api { get; set; } = new();

    public override void Load(bool hotReload)
    {
        Capabilities.RegisterPluginCapability(ITagApi.Capability, () => Api);
        AddCommandListener("css_admins_reload", Command_Admins_Reloads, HookMode.Pre);
        HookUserMessage(118, OnMessage, HookMode.Pre);

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
        RemoveCommandListener("css_admins_reload", Command_Admins_Reloads, HookMode.Pre);
    }

    public static HookResult OnMessage(UserMessage um)
    {
        if (Utilities.GetPlayerFromIndex(um.ReadInt("entityindex")) is not CCSPlayerController player || player.IsBot)
        {
            return HookResult.Continue;
        }

        string message = um.ReadString("param2");
        string cleanedMessage = message.RemoveCurlyBraceContent();

        if (string.IsNullOrWhiteSpace(cleanedMessage))
        {
            return HookResult.Handled;
        }

        string playername = um.ReadString("param1");
        bool chatsound = um.ReadBool("chat");
        bool teammessage = !um.ReadString("messagename").Contains("All");

        HookResult result = Api.MessageProcessPre(playername, message, chatsound, teammessage);

        if (result >= HookResult.Handled)
        {
            return result;
        }

        Tag playerData = player.GetToggleTags() ? Config.DefaultTags : PlayerTagsList[player];

        string deadname = player.PawnIsAlive ? string.Empty : Config.Settings.DeadName;
        string teamname = teammessage ? player.Team.Name() : string.Empty;

        CsTeam team = player.Team;
        playername = FormatMessage(team, deadname, teamname, playerData.ChatTag, playerData.NameColor, playername);
        message = FormatMessage(team, playerData.ChatColor, cleanedMessage);

        result = Api.MessageProcess(playername, message, chatsound, teammessage);

        if (result >= HookResult.Handled)
        {
            return result;
        }

        um.SetString("messagename", $"{playername}{ChatColors.White}: {message}");
        um.SetBool("chat", playerData.ChatSound);

        Api.MessageProcessPost(playername, message, chatsound, teammessage);

        return HookResult.Changed;
    }
}