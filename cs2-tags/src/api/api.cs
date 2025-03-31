// Modified TagsAPI with recursion protection
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using TagsApi;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public class TagsAPI : ITagApi
{
    private bool _isProcessingTagsUpdatedPre = false;
    private bool _isProcessingTagsUpdatedPost = false;
    private bool _isProcessingMessagePre = false;
    private bool _isProcessingMessage = false;
    private bool _isProcessingMessagePost = false;

    public event Func<MessageProcess, HookResult>? OnMessageProcessPre;
    public event Func<MessageProcess, HookResult>? OnMessageProcess;
    public event Action<MessageProcess>? OnMessageProcessPost;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPre;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPost;

    public HookResult MessageProcessPre(MessageProcess messageProcess)
    {
        if (_isProcessingMessagePre)
            return HookResult.Continue;

        try
        {
            _isProcessingMessagePre = true;
            return OnMessageProcessPre?.Invoke(messageProcess) ?? HookResult.Continue;
        }
        finally
        {
            _isProcessingMessagePre = false;
        }
    }

    public HookResult MessageProcess(MessageProcess messageProcess)
    {
        if (_isProcessingMessage)
            return HookResult.Continue;

        try
        {
            _isProcessingMessage = true;
            return OnMessageProcess?.Invoke(messageProcess) ?? HookResult.Continue;
        }
        finally
        {
            _isProcessingMessage = false;
        }
    }

    public void MessageProcessPost(MessageProcess messageProcess)
    {
        if (_isProcessingMessagePost)
            return;

        try
        {
            _isProcessingMessagePost = true;
            OnMessageProcessPost?.Invoke(messageProcess);
        }
        finally
        {
            _isProcessingMessagePost = false;
        }
    }

    public void TagsUpdatedPre(CCSPlayerController player, Tag tag)
    {

        if (_isProcessingTagsUpdatedPre)
            return;

        try
        {
            _isProcessingTagsUpdatedPre = true;
            OnTagsUpdatedPre?.Invoke(player, tag);
        }
        finally
        {
            _isProcessingTagsUpdatedPre = false;
        }
    }

    public void TagsUpdatedPost(CCSPlayerController player, Tag tag)
    {
        if (_isProcessingTagsUpdatedPost)
            return;

        try
        {
            _isProcessingTagsUpdatedPost = true;
            OnTagsUpdatedPost?.Invoke(player, tag);
        }
        finally
        {
            _isProcessingTagsUpdatedPost = false;
        }
    }

    public void AddAttribute(CCSPlayerController player, TagType types, TagPrePost prePost, string newValue)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }

        if ((types & TagType.ScoreTag) != 0)
        {
            string value = GetPrePostValue(prePost, playerData.ScoreTag, newValue);
            playerData.ScoreTag = value;
            player.SetScoreTag(value);
        }
        if ((types & TagType.ChatTag) != 0)
        {
            string value = GetPrePostValue(prePost, playerData.ChatTag, newValue);
            playerData.ChatTag = value;
        }
        if ((types & TagType.NameColor) != 0)
        {
            string value = GetPrePostValue(prePost, playerData.NameColor, newValue);
            playerData.NameColor = value;
        }
        if ((types & TagType.ChatColor) != 0)
        {
            string value = GetPrePostValue(prePost, playerData.ChatColor, newValue);
            playerData.ChatColor = value;
        }
    }

    private static string GetPrePostValue(TagPrePost prePost, string? oldValue, string newValue)
    {
        return prePost switch
        {
            TagPrePost.Pre => newValue + oldValue,
            TagPrePost.Post => oldValue + newValue,
            _ => newValue
        };
    }

    public void SetAttribute(CCSPlayerController player, TagType types, string newValue)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }

        if ((types & TagType.ScoreTag) != 0)
        {
            playerData.ScoreTag = newValue;
            player.SetScoreTag(newValue);
        }
        if ((types & TagType.ChatTag) != 0)
        {
            playerData.ChatTag = newValue;
        }
        if ((types & TagType.NameColor) != 0)
        {
            playerData.NameColor = newValue;
        }
        if ((types & TagType.ChatColor) != 0)
        {
            playerData.ChatColor = newValue;
        }
    }

    public string? GetAttribute(CCSPlayerController player, TagType type)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }

        return type switch
        {
            TagType.ScoreTag => playerData.ScoreTag,
            TagType.ChatTag => playerData.ChatTag,
            TagType.NameColor => playerData.NameColor,
            TagType.ChatColor => playerData.ChatColor,
            _ => null
        };
    }

    public void ResetAttribute(CCSPlayerController player, TagType types)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
            return;
        }

        Tag defaultTag = player.GetTag();

        if ((types & TagType.ScoreTag) != 0)
        {
            playerData.ScoreTag = defaultTag.ScoreTag;
            player.SetScoreTag(defaultTag.ScoreTag);
        }
        if ((types & TagType.ChatTag) != 0)
        {
            playerData.ChatTag = defaultTag.ChatTag;
        }
        if ((types & TagType.NameColor) != 0)
        {
            playerData.NameColor = defaultTag.NameColor;
        }
        if ((types & TagType.ChatColor) != 0)
        {
            playerData.ChatColor = defaultTag.ChatColor;
        }
    }

    public bool GetPlayerChatSound(CCSPlayerController player)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }
        return playerData.ChatSound;
    }

    public void SetPlayerChatSound(CCSPlayerController player, bool value)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }
        playerData.ChatSound = value;
    }

    public bool GetPlayerVisibility(CCSPlayerController player)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }
        return playerData.Visibility;
    }

    public void SetPlayerVisibility(CCSPlayerController player, bool value)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }
        playerData.Visibility = value;
        player.SetScoreTag(value ? playerData.ScoreTag : Instance.Config.Default.ScoreTag);
    }

    public void ReloadTags()
    {
        Instance.Config.Reload();
        UpdateConfig(Instance.Config);
    }

    public void SetExternalTag(CCSPlayerController player, TagType types, string newValue, bool persistent = true)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }
        
        playerData.IsExternal = true;
        
        if ((types & TagType.ScoreTag) != 0)
        {
            playerData.ScoreTag = newValue;
            player.SetScoreTag(newValue);
        }
        if ((types & TagType.ChatTag) != 0)
        {
            playerData.ChatTag = newValue;
        }
        if ((types & TagType.NameColor) != 0)
        {
            playerData.NameColor = newValue;
        }
        if ((types & TagType.ChatColor) != 0)
        {
            playerData.ChatColor = newValue;
        }
        
        if (persistent)
        {
            Database.SavePlayer(player);
        }
    }

    public void SetPlayerTagExternal(CCSPlayerController player, bool isExternal)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }
        
        playerData.IsExternal = isExternal;
        
        Database.SavePlayer(player);
    }

    public bool IsPlayerTagExternal(CCSPlayerController player)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            return false;
        }
        
        return playerData.IsExternal;
    }

    public void ClearExternalTag(CCSPlayerController player, bool resetToDefaultPermission = true)
    {
        if (!PlayerTagsList.TryGetValue(player.SteamID, out Tag? playerData))
        {
            playerData = player.GetTag();
            PlayerTagsList[player.SteamID] = playerData;
        }
        
        playerData.IsExternal = false;
        
        if (resetToDefaultPermission)
        {
            Tag permissionTag = player.GetTag();
            
            permissionTag.Visibility = playerData.Visibility;
            permissionTag.ChatSound = playerData.ChatSound;
            permissionTag.IsExternal = false;
            
            PlayerTagsList[player.SteamID] = permissionTag;
            player.SetScoreTag(permissionTag.ScoreTag);
        }
        
        Database.SavePlayer(player);
    }
}