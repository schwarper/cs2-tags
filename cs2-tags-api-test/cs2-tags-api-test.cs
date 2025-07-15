using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using TagsApi;
using static TagsApi.Tags;

namespace TagsApiTest;

public class TagsApiTest : BasePlugin
{
    public override string ModuleName => "Tags API Test";
    public override string ModuleVersion => "1.1";
    public override string ModuleAuthor => "schwarper";

    private ITagApi _tagApi = null!;

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _tagApi = ITagApi.Capability.Get() ?? throw new Exception("Tags API not found!");
    }

    [ConsoleCommand("css_tag_get")]
    public void Cmd_GetTag(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null) return;

        string output = @$"
            ScoreTag     => {_tagApi.GetAttribute(player, TagType.ScoreTag)}
            ChatTag      => {_tagApi.GetAttribute(player, TagType.ChatTag)}
            ChatColor    => {_tagApi.GetAttribute(player, TagType.ChatColor)}
            NameColor    => {_tagApi.GetAttribute(player, TagType.NameColor)}
            ChatSound    => {_tagApi.GetPlayerChatSound(player)}
            Visibility   => {_tagApi.GetPlayerVisibility(player)}
            ";

        info.ReplyToCommand(output);
    }

    [ConsoleCommand("css_tag_set")]
    public void Cmd_SetTag(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null) return;

        _tagApi.SetAttribute(player, TagType.ScoreTag | TagType.ChatTag, "[Test]");
        _tagApi.SetAttribute(player, TagType.ChatColor, "{red}");
        _tagApi.SetAttribute(player, TagType.NameColor, "{blue}");
        _tagApi.SetPlayerChatSound(player, false);

        _tagApi.AddAttribute(player, TagType.ScoreTag, TagPrePost.Pre, "[Pre]");
        _tagApi.AddAttribute(player, TagType.ChatTag, TagPrePost.Post, "[Post]");

        info.ReplyToCommand("Attributes set.");
    }

    [ConsoleCommand("css_tag_reset")]
    public void Cmd_ResetTag(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null) return;

        _tagApi.ResetAttribute(player, TagType.ScoreTag | TagType.ChatTag | TagType.ChatColor | TagType.NameColor);
        info.ReplyToCommand("Attributes reset.");
    }

    [ConsoleCommand("css_tag_toggle_visibility")]
    public void Cmd_ToggleVisibility(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null) return;

        bool current = _tagApi.GetPlayerVisibility(player);
        _tagApi.SetPlayerVisibility(player, !current);

        info.ReplyToCommand($"Visibility set to {!current}.");
    }

    [ConsoleCommand("css_tag_reload")]
    public void Cmd_ReloadTags(CCSPlayerController? player, CommandInfo info)
    {
        _tagApi.ReloadTags();
        info.ReplyToCommand("Tags reloaded.");
    }

    [ConsoleCommand("css_tag_hook_events")]
    public void Cmd_HookEvents(CCSPlayerController? player, CommandInfo info)
    {
        _tagApi.OnMessageProcessPre += OnMessageProcessPre;
        _tagApi.OnMessageProcess += OnMessageProcess;
        _tagApi.OnMessageProcessPost += OnMessageProcessPost;

        _tagApi.OnTagsUpdatedPre += OnTagsUpdatedPre;
        _tagApi.OnTagsUpdatedPost += OnTagsUpdatedPost;

        info.ReplyToCommand("Tag events hooked.");
    }

    private HookResult OnMessageProcessPre(MessageProcess ctx)
    {
        Console.WriteLine($"""
            [PRE] Chat
            Player       => {ctx.Player.PlayerName}
            Message      => {ctx.Message}
            PlayerName   => {ctx.PlayerName}
            ChatSound    => {ctx.ChatSound}
            TeamMessage  => {ctx.TeamMessage}
            """);
        return HookResult.Continue;
    }

    private HookResult OnMessageProcess(MessageProcess ctx)
    {
        ctx.PlayerName = $"{ChatColors.Red}[Hooked] " + ctx.PlayerName;
        Console.WriteLine($"""
            [MID] Chat
            Player       => {ctx.Player.PlayerName}
            Message      => {ctx.Message}
            PlayerName   => {ctx.PlayerName}
            ChatSound    => {ctx.ChatSound}
            TeamMessage  => {ctx.TeamMessage}
            """);
        return HookResult.Continue;
    }

    private void OnMessageProcessPost(MessageProcess ctx)
    {
        Console.WriteLine($"""
            [POST] Chat
            Player       => {ctx.Player.PlayerName}
            Message      => {ctx.Message}
            PlayerName   => {ctx.PlayerName}
            ChatSound    => {ctx.ChatSound}
            TeamMessage  => {ctx.TeamMessage}
            """);
    }

    private void OnTagsUpdatedPre(CCSPlayerController player, Tag tag)
    {
        Console.WriteLine($"""
            [PRE] Tags Updated
            Player       => {player.PlayerName}
            ScoreTag     => {tag.ScoreTag}
            ChatTag      => {tag.ChatTag}
            ChatColor    => {tag.ChatColor}
            NameColor    => {tag.NameColor}
            """);
    }

    private void OnTagsUpdatedPost(CCSPlayerController player, Tag tag)
    {
        Console.WriteLine($"""
            [POST] Tags Updated
            Player       => {player.PlayerName}
            ScoreTag     => {tag.ScoreTag}
            ChatTag      => {tag.ChatTag}
            ChatColor    => {tag.ChatColor}
            NameColor    => {tag.NameColor}
            """);
    }

    [ConsoleCommand("css_tag_donor")]
    public void Cmd_Donor(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null) return;
        _tagApi.OnMessageProcessPre += AddDonorTag;
        info.ReplyToCommand("Donor tag hook added.");
    }

    private static HookResult AddDonorTag(MessageProcess ctx)
    {
        Server.PrintToChatAll($"Donor tag added.");
        ctx.Tag.ChatTag = "DONOR";
        ctx.Tag.ChatColor = ChatColors.Red.ToString();
        return HookResult.Continue;
    }
}