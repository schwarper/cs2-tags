using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using static TagApi.Tag;

namespace TagApi;

public interface ITagApi
{
    public static readonly PluginCapability<ITagApi?> Capability = new("tag:api");

    public string GetClientTag(CCSPlayerController player, Tags tag);
    public void SetClientTag(CCSPlayerController player, Tags tag, string newtag);
    public void ResetClientTag(CCSPlayerController player, Tags tag);
    public string GetClientColor(CCSPlayerController player, Colors color);
    public void SetClientColor(CCSPlayerController player, Colors color, string newcolor);
    public void ResetClientColor(CCSPlayerController player, Colors color);
    public void ReloadTags();
}