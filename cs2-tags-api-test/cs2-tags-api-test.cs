using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using TagsApi;
using static TagsApi.Tags;

namespace TagsApiTest;

public class TagsApiTest : BasePlugin
{
    public override string ModuleName => "Tags Api Test";
    public override string ModuleVersion => "1.1";
    public override string ModuleAuthor => "schwarper";

    public ITagApi tagApi = null!;

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        tagApi = ITagApi.Capability.Get() ?? throw new Exception("Tags Api not found!");
    }

    [ConsoleCommand("css_testgettag")]
    public void Command_TestGetTag(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;

        info.ReplyToCommand(@$"
            ScoreTag => {tagApi.GetAttribute(player, TagType.ScoreTag)},
            ChatTag => {tagApi.GetAttribute(player, TagType.ChatTag)},
            ChatColor => {tagApi.GetAttribute(player, TagType.ChatColor)},
            NameColor => {tagApi.GetAttribute(player, TagType.NameColor)},
            ChatSound => {tagApi.GetPlayerChatSound(player)},
            Visibility => {tagApi.GetPlayerVisibility(player)}
        ");
    }

    [ConsoleCommand("css_testsettag")]
    public void Command_TestSetTag(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;

        tagApi.SetAttribute(player, TagType.ScoreTag | TagType.ChatTag, "[Test]");
        tagApi.SetAttribute(player, TagType.ChatColor, "{red}");
        tagApi.SetAttribute(player, TagType.NameColor, "{blue}");
        tagApi.SetPlayerChatSound(player, false);

        tagApi.AddAttribute(player, TagType.ScoreTag, TagPrePost.Pre, "[Test3]");
        tagApi.AddAttribute(player, TagType.ChatTag, TagPrePost.Post, "[Test4]");

        info.ReplyToCommand("Tags set!");
    }

    [ConsoleCommand("css_testresettag")]
    public void Command_TestResetTag(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;

        tagApi.ResetAttribute(player, TagType.ScoreTag | TagType.ChatTag | TagType.ChatColor | TagType.NameColor);
        info.ReplyToCommand("Tags reset!");
    }

    [ConsoleCommand("css_testvisibility")]
    public void Command_TestVisibility(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;

        bool currentVisibility = tagApi.GetPlayerVisibility(player);
        tagApi.SetPlayerVisibility(player, !currentVisibility);

        info.ReplyToCommand($"Tags are now {(currentVisibility ? "hidden" : "visible")}");
    }

    [ConsoleCommand("css_testreloadtags")]
    public void Command_TestReloadTags(CCSPlayerController? player, CommandInfo info)
    {
        tagApi.ReloadTags();
        info.ReplyToCommand("Tags reloaded!");
    }

    [ConsoleCommand("css_testmessageprocesspre")]
    public void Command_TestMessageProcessPre(CCSPlayerController? player, CommandInfo info)
    {
        tagApi!.OnMessageProcessPre += (messageProcess) =>
        {
            Console.WriteLine($@"
                OnMessageProcessPre
                Player => {messageProcess.Player.PlayerName},
                Message => {messageProcess.Message},
                PlayerName => {messageProcess.PlayerName},
                ChatSound => {messageProcess.ChatSound},
                TeamMessage => {messageProcess.TeamMessage}
            ");
            return HookResult.Continue;
        };

        tagApi!.OnMessageProcess += (messageProcess) =>
        {
            messageProcess.PlayerName = "{red}" + $"[OMP] {messageProcess.PlayerName}";
            Console.WriteLine($@"
                OnMessageProcess
                Player => {messageProcess.Player.PlayerName},
                Message => {messageProcess.Message},
                PlayerName => {messageProcess.PlayerName},
                ChatSound => {messageProcess.ChatSound},
                TeamMessage => {messageProcess.TeamMessage}
            ");
            return HookResult.Continue;
        };

        tagApi!.OnMessageProcessPost += (messageProcess) =>
        {
            Console.WriteLine($@"
                OnMessageProcessPost
                Player => {messageProcess.Player.PlayerName},
                Message => {messageProcess.Message},
                PlayerName => {messageProcess.PlayerName},
                ChatSound => {messageProcess.ChatSound},
                TeamMessage => {messageProcess.TeamMessage}
            ");
        };

        tagApi!.OnTagsUpdatedPre += (controller, tag) =>
        {
            info.ReplyToCommand($@"
                OnTagsUpdatedPre
                Player => {controller.PlayerName},
                ScoreTag => {tag.ScoreTag},
                ChatTag => {tag.ChatTag},
                ChatColor => {tag.ChatColor},
                NameColor => {tag.NameColor},
            ");
        };

        tagApi!.OnTagsUpdatedPost += (controller, tag) =>
        {
            info.ReplyToCommand($@"
                OnTagsUpdatedPost
                Player => {controller.PlayerName},
                ScoreTag => {tag.ScoreTag},
                ChatTag => {tag.ChatTag},
                ChatColor => {tag.ChatColor},
                NameColor => {tag.NameColor},
            ");
        };
    }
}