using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using System.Collections.Concurrent;
using TagApi;
using static TagApi.Tag;

namespace Tag;

public partial class Tag : BasePlugin, IPluginConfig<TagConfig>
{
    public override string ModuleName => "Tag";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";

    public TagConfig Config { get; set; } = new TagConfig();
    public ConcurrentDictionary<int, CTag> PlayerDatas { get; set; } = new ConcurrentDictionary<int, CTag>();
    public bool[] PlayerToggleTags { get; set; } = new bool[64];
    public static Tag Instance { get; set; } = new Tag();
    public int GlobalTick { get; set; }

    public override void Load(bool hotReload)
    {
        Capabilities.RegisterPluginCapability(ITagApi.Capability, () => new TagAPI());

        Instance = this;

        for (int i = 0; i < 64; i++)
        {
            PlayerDatas[i] = new();
            PlayerToggleTags[i] = new();
        }

        if (hotReload)
        {
            UpdatePlayerTags();
        }

        Event.Load();
    }

    public void OnConfigParsed(TagConfig config)
    {
        Json.ReadCore();
        Config = config;
    }

    public static void UpdatePlayerTags()
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            Instance.PlayerDatas[player.Slot] = GetTag(player);
        }
    }

    public static CTag GetTag(CCSPlayerController player)
    {
        ConcurrentDictionary<string, CTag> tags = Instance.Config.Tags;

        CTag steamidTag = tags.FirstOrDefault(tag => tag.Key == player.SteamID.ToString()).Value;

        if (steamidTag != null)
        {
            return steamidTag;
        }

        CTag groupTag = tags.FirstOrDefault(tag => tag.Key.StartsWith('#') && AdminManager.PlayerInGroup(player, tag.Key)).Value;

        if (groupTag != null)
        {
            return groupTag;
        }

        CTag permissionTag = tags.FirstOrDefault(tag => tag.Key.StartsWith('@') && AdminManager.PlayerHasPermissions(player, tag.Key)).Value;

        if (permissionTag != null)
        {
            return permissionTag;
        }

        return tags.FirstOrDefault(tag => tag.Key == "default").Value ?? new CTag();
    }
}