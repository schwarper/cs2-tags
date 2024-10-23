namespace TagsApi;

public abstract class Tags
{
    public class Tag
    {
        public string ScoreTag { get; set; } = string.Empty;
        public string ChatTag { get; set; } = string.Empty;
        public string ChatColor { get; set; } = string.Empty;
        public string NameColor { get; set; } = string.Empty;
        public bool ChatSound { get; set; } = true;

        public Tag Clone()
        {
            return new Tag
            {
                ScoreTag = ScoreTag,
                ChatTag = ChatTag,
                ChatColor = ChatColor,
                NameColor = NameColor,
                ChatSound = ChatSound
            };
        }
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