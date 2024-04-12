using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using static Tag.Tag;
using static TagApi.Tag;

namespace Tag;

public partial class Tag
{
    [ConsoleCommand("css_tags_reload")]
    [RequiresPermissions("@css/root")]
    public void Command_Tags_Reload(CCSPlayerController? player, CommandInfo info)
    {
        Json.ReadConfig();
        UpdatePlayerTags();

        info.ReplyToCommand("[cs2-tags] Tags are reloaded.");
    }

    [ConsoleCommand("css_toggletags")]
    [RequiresPermissions("@css/admin")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_Toggletags(CCSPlayerController? player, CommandInfo info)
    {
        if (Instance.PlayerToggleTags[player!.Slot])
        {
            Instance.PlayerDatas[player.Slot] = Instance.Config.Tags.FirstOrDefault(tag => tag.Key == "default").Value ?? new CTag();

            Instance.PlayerToggleTags[player.Slot] = false;

            info.ReplyToCommand("[cs2-tags] Toggletags is false");
        }
        else
        {
            Instance.PlayerDatas[player.Slot] = GetTag(player);

            Instance.PlayerToggleTags[player.Slot] = true;

            info.ReplyToCommand("[cs2-tags] Toggletags is true");
        }
    }
}