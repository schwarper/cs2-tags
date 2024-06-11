using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using TagsApi;
using static TagsApi.Tags;

namespace Tags;

public partial class Tags : BasePlugin, IPluginConfig<TagsConfig>
{
    public override string ModuleName => "Tag";
    public override string ModuleVersion => "0.0.4";
    public override string ModuleAuthor => "schwarper";

    public TagsConfig Config { get; set; } = new TagsConfig();
    public Dictionary<int, Tag> PlayerTagDatas { get; set; } = [];
    public Dictionary<int, bool> PlayerToggleTags { get; set; } = [];
    public static Tags Instance { get; set; } = new Tags();
    public int GlobalTick { get; set; }
    public static HashSet<ulong> PlayerGagList { get; set; } = [];

    public override void Load(bool hotReload)
    {
        Capabilities.RegisterPluginCapability(ITagApi.Capability, () => new TagsAPI());

        Instance = this;

        Event.Load();

        if (hotReload)
        {
            UpdatePlayerTags();
        }
    }

    public override void Unload(bool hotReload)
    {
        Event.Unload();
    }

    public void OnConfigParsed(TagsConfig config)
    {
        Config = config;
    }

    public static void UpdatePlayerTags()
    {
        Instance.PlayerTagDatas.Clear();
        Instance.PlayerToggleTags.Clear();

        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            Instance.PlayerTagDatas.Add(player.Slot, GetTag(player));
            Instance.PlayerToggleTags.Add(player.Slot, true);
        }
    }

    public static Tag GetTag(CCSPlayerController player)
    {
        Dictionary<string, Tag> tags = Instance.Config.Tags;

        Tag steamidTag = tags.FirstOrDefault(tag => tag.Key == player.SteamID.ToString()).Value;

        if (steamidTag != null)
        {
            return steamidTag;
        }

        Tag groupTag = tags.FirstOrDefault(tag => tag.Key.StartsWith('#') && AdminManager.PlayerInGroup(player, tag.Key)).Value;

        if (groupTag != null)
        {
            return groupTag;
        }

        Tag permissionTag = tags.FirstOrDefault(tag => tag.Key.StartsWith('@') && AdminManager.PlayerHasPermissions(player, tag.Key)).Value;

        if (permissionTag != null)
        {
            return permissionTag;
        }

        return tags.FirstOrDefault(tag => tag.Key == "default").Value ?? new Tag();
    }
}
