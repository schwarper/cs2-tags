using CounterStrikeSharp.API.Core;

namespace TagsApi;

public abstract class Tags
{
    public class Tag
    {
        public string ScoreTag { get; set; } = string.Empty;
        public string ChatTag { get; set; } = string.Empty;
        public string ChatColor { get; set; } = string.Empty;
        public string NameColor { get; set; } = string.Empty;
    }

    public enum Tags_Tags
    {
        ScoreTag,
        ChatTag
    }

    public enum Tags_Colors
    {
        ChatColor,
        NameColor
    }
}