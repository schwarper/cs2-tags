using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using TagApi;
using static TagApi.Tag;

namespace Tag;

public partial class Tag : BasePlugin, IPluginConfig<TagConfig>
{
    public override string ModuleName => "Tag";
    public override string ModuleVersion => "0.0.2";
    public override string ModuleAuthor => "schwarper";

    public TagConfig Config { get; set; } = new TagConfig();
    public Dictionary<int, CTag> PlayerTagDatas { get; set; } = [];
    public Dictionary<int, bool> PlayerToggleTags { get; set; } = [];
    public static Tag Instance { get; set; } = new Tag();
    public int GlobalTick { get; set; }

    public override void Load(bool hotReload)
    {
        Capabilities.RegisterPluginCapability(ITagApi.Capability, () => new TagAPI());

        Instance = this;

        if (hotReload)
        {
            UpdatePlayerTags();
        }

        Event.Load();
    }

    public void OnConfigParsed(TagConfig config)
    {
        Config = config;
    }

    public static void UpdatePlayerTags()
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            Instance.PlayerTagDatas.Add(player.Slot, GetTag(player));
            Instance.PlayerToggleTags.Add(player.Slot, true);
        }
    }

    public static CTag GetTag(CCSPlayerController player)
    {
        Dictionary<string, CTag> tags = Instance.Config.Tags;

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