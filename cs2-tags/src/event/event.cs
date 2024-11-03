using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace Tags;

public partial class Tags
{
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

        PlayerTagsList.Remove(player);
        PlayerToggleTagsList.Remove(player);
        return HookResult.Continue;
    }
}