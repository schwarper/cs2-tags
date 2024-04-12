namespace TagApi;

public abstract class Tag
{
    public class CTag
    {
        public string ScoreTag { get; set; } = string.Empty;
        public string ChatTag { get; set; } = string.Empty;
        public string ChatColor { get; set; } = string.Empty;
        public string NameColor { get; set; } = string.Empty;
    }

    public enum Tags
    {
        ScoreTag,
        ChatTag
    }

    public enum Colors
    {
        ChatColor,
        NameColor
    }
}