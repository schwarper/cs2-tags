using CounterStrikeSharp.API.Core;

namespace TagsApi;

public abstract class Tags
{
    public class Tag
    {
        public string? ScoreTag { get; set; }
        public string? ChatTag { get; set; }
        public string? ChatColor { get; set; }
        public string? NameColor { get; set; }
        public bool Visibility { get; set; } = true;
        public bool ChatSound { get; set; } = true;

        public Tag Clone()
        {
            return new Tag
            {
                ScoreTag = ScoreTag,
                ChatTag = ChatTag,
                ChatColor = ChatColor,
                NameColor = NameColor,
                Visibility = Visibility,
                ChatSound = ChatSound
            };
        }
    }

    public class MessageProcess
    {
        public required CCSPlayerController Player { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool ChatSound { get; set; } = true;
        public bool TeamMessage { get; set; } = false;
    }

    [Flags]
    public enum TagType
    {
        None = 0,
        ScoreTag = 1 << 0,
        ChatTag = 1 << 1,
        NameColor = 1 << 2,
        ChatColor = 1 << 3
    }

    public enum TagPrePost
    {
        Pre,
        Post
    }
}