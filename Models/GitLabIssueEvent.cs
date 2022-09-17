namespace IssueSync.Models;

public class GitLabIssueEvent
{
    public string ObjectKind { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public GitLabIssueAttributes ObjectAttributes { get; set; } = null!;
}

public class GitLabIssueAttributes
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public List<GitLabIssueLabel> Labels { get; set; } = new();
    public string? Action { get; set; }
}

public class GitLabIssueLabel
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
}