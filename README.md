
# cs2-tags

If you want to donate or need a help about plugin, you can contact me in discord private/server

Discord nickname: schwarper

Discord link : [Discord server](https://discord.gg/4zQfUzjk36)

## Credits

[Hextags plugin for CSGO](https://github.com/Hexer10/HexTags)

[@daffyyyy](https://github.com/daffyyyy/)

[@Yarukon](https://github.com/Yarukon)



## Commands
```
css_tags_reload - Reloads tag
css_toggletags - Hide/Show tag
```

## Colors
```
Default
White
TeamColor
DarkRed
Green
LightYellow
LightBlue
Olive
Lime
Red
LightPurple
Purple
Grey
Yellow
Gold
Silver
Blue
DarkBlue
BlueGrey
Magenta
LightRed
Orange
```

## Configuration
```toml
[Settings]
DeadName = "☠"
NoneName = "{White}(NONE)"
SpecName = "{Purple}(SPEC)"
TName = "{Yellow}(T)"
CTName = "{Blue}(CT)"

[Default]
ScoreTag = ""
ChatTag = "{Grey}[Player]"
ChatColor = "{White}"
NameColor = "{TeamColor}"

[76561199165718810]
ScoreTag = "schwarper"
ChatTag = "{Red}[schwarper] "
ChatColor = "{green}"
NameColor = "{TeamColor}"

["#OWNER"]
ScoreTag = "OWNER"
ChatTag = "{Red}[OWNER] "
ChatColor = "{green}"
NameColor = "{TeamColor}"
```

## Screenshots
![tag1](https://github.com/user-attachments/assets/93a333b4-55e0-4582-8f09-8ec3010724d3)

![tag2](https://github.com/user-attachments/assets/9066cb2f-2b6d-4268-9db3-b824de28d05e)

## API
```csharp
public static readonly PluginCapability<ITagApi> Capability = new("tags:api");

/**
 * Triggered before a player sends a chat message.
 */
public event Action<UserMessage>? OnPlayerChatPre;

/**
 * Retrieves the specified tag for a player.
 *
 * @param player    The player whose tag is being retrieved.
 * @param tag      The type of tag to retrieve.
 * @returns        The tag associated with the player.
 */
public string GetPlayerTag(CCSPlayerController player, Tags_Tags tag);

/**
 * Sets a new tag for a player.
 *
 * @param player    The player for whom the tag is being set.
 * @param tag      The type of tag to set.
 * @param newtag   The new tag value to be assigned.
 */
public void SetPlayerTag(CCSPlayerController player, Tags_Tags tag, string newtag);

/**
 * Resets the specified tag for a player to its default value.
 *
 * @param player    The player whose tag is being reset.
 * @param tag      The type of tag to reset.
 */
public void ResetPlayerTag(CCSPlayerController player, Tags_Tags tag);

/**
 * Retrieves the specified color for a player.
 *
 * @param player    The player whose color is being retrieved.
 * @param color     The type of color to retrieve.
 * @returns        The color associated with the player.
 */
public string GetPlayerColor(CCSPlayerController player, Tags_Colors color);

/**
 * Sets a new color for a player.
 *
 * @param player    The player for whom the color is being set.
 * @param color     The type of color to set.
 * @param newcolor  The new color value to be assigned.
 */
public void SetPlayerColor(CCSPlayerController player, Tags_Colors color, string newcolor);

/**
 * Resets the specified color for a player to its default value.
 *
 * @param player    The player whose color is being reset.
 * @param color     The type of color to reset.
 */
public void ResetPlayerColor(CCSPlayerController player, Tags_Colors color);

/**
 * Retrieves the chat sound setting for a player.
 *
 * @param player    The player whose chat sound setting is being retrieved.
 * @returns        True if the chat sound is enabled, otherwise false.
 */
public bool GetPlayerChatSound(CCSPlayerController player);

/**
 * Sets the chat sound setting for a player.
 *
 * @param controller  The player for whom the chat sound setting is being set.
 * @param value       The new chat sound setting value.
 */
public void SetPlayerChatSound(CCSPlayerController controller, bool value);

/**
 * Retrieves the toggle setting for player tags.
 *
 * @param player    The player whose toggle setting is being retrieved.
 * @returns        True if the tags toggle is enabled, otherwise false.
 */
public bool GetPlayerToggleTags(CCSPlayerController player);

/**
 * Sets the toggle setting for player tags.
 *
 * @param player    The player for whom the tags toggle setting is being set.
 * @param value     The new tags toggle setting value.
 */
public void SetPlayerToggleTags(CCSPlayerController player, bool value);

/**
 * Reloads the player tags from the configuration.
 */
public void ReloadTags();
```


