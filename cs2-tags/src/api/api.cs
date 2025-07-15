using CounterStrikeSharp.API.Core;
using TagsApi;
using static TagsApi.Tags;

namespace Tags;

public class TagsAPI : ITagApi
{
    private bool _isProcessingTagsUpdatedPre;
    private bool _isProcessingTagsUpdatedPost;
    private bool _isProcessingMessagePre;
    private bool _isProcessingMessage;
    private bool _isProcessingMessagePost;

    public event Func<MessageProcess, HookResult>? OnMessageProcessPre;
    public event Func<MessageProcess, HookResult>? OnMessageProcess;
    public event Action<MessageProcess>? OnMessageProcessPost;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPre;
    public event Action<CCSPlayerController, Tag>? OnTagsUpdatedPost;

    public HookResult MessageProcessPre(MessageProcess messageProcess)
    {
        if (_isProcessingMessagePre)
            return HookResult.Continue;

        _isProcessingMessagePre = true;

        try
        {
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

        _isProcessingMessage = true;

        try
        {
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

        _isProcessingMessagePost = true;

        try
        {
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

        _isProcessingTagsUpdatedPre = true;

        try
        {
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

        _isProcessingTagsUpdatedPost = true;

        try
        {
            OnTagsUpdatedPost?.Invoke(player, tag);
        }
        finally
        {
            _isProcessingTagsUpdatedPost = false;
        }
    }

    public void AddAttribute(CCSPlayerController player, TagType types, TagPrePost prePost, string newValue)
    {
        player.AddAttribute(types, prePost, newValue);
    }

    public void SetAttribute(CCSPlayerController player, TagType types, string newValue)
    {
        player.SetAttribute(types, newValue);
    }

    public string? GetAttribute(CCSPlayerController player, TagType type)
    {
        return player.GetAttribute(type);
    }

    public void ResetAttribute(CCSPlayerController player, TagType types)
    {
        player.ResetAttribute(types);
    }

    public bool GetPlayerChatSound(CCSPlayerController player)
    {
        return player.GetChatSound();
    }

    public void SetPlayerChatSound(CCSPlayerController player, bool value)
    {
        player.SetChatSound(value);
    }

    public bool GetPlayerVisibility(CCSPlayerController player)
    {
        return player.GetVisibility();
    }

    public void SetPlayerVisibility(CCSPlayerController player, bool value)
    {
        player.SetVisibility(value);
    }

    public void ReloadTags()
    {
        TagExtensions.ReloadTags();
    }
}