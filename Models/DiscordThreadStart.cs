namespace IssueSync.Models;

public class DiscordThreadStart
{
    public string Name { get; set; } = null!;
    public int? AutoArchiveDuration { get; set; }
    public DiscordThreadMessage Message { get; set; } = null!;
    public List<string>? AppliedTags { get; set; }
}

public class DiscordThreadMessage
{
    public string Content { get; set; } = null!;
}