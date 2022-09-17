namespace IssueSync.Models;

public class Issue
{
    public int Id { get; set; }
    public string DiscordChannelId { get; set; } = null!;
}