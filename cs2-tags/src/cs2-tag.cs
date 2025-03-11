using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using TagsApi;
using static TagsApi.Tags;

namespace Tags;

public partial class Tags : BasePlugin
{
    public override string ModuleName => "Tags";
    public override string ModuleVersion => "1.1";
    public override string ModuleAuthor => "schwarper";

    public static Dictionary<CCSPlayerController, Tag> PlayerTagsList { get; set; } = [];
    public static TagsAPI Api { get; set; } = new();

    public override void Load(bool hotReload)
    {
        Capabilities.RegisterPluginCapability(ITagApi.Capability, () => Api);
        HookUserMessage(118, OnMessage, HookMode.Pre);
        ConfigManager.LoadConfig(hotReload);
        AddCommands();
    }

    public override void Unload(bool hotReload)
    {
        RemoveCommandListener("css_admins_reload", Command_Admins_Reloads, HookMode.Pre);
    }
}

/*
 * Fix OnTagsUpdatePre and OnTagsUpdatePost
 * Fix css_admins_reload
 */