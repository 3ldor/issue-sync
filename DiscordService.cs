using System.Text.Json;
using IssueSync.Models;

namespace IssueSync;

public class DiscordService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = new SnakeCaseNamingPolicy() };
    
    private readonly HttpClient _httpClient;
    
    public DiscordService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Discord");
    }

    public async Task CreatePost(string channelId, string title, string message, IEnumerable<string> tags)
    {
        // Build body
        var body = new DiscordThreadStart
        {
            Name = title,
            AutoArchiveDuration = 10080,
            Message = new DiscordThreadMessage
            {
                Content = message
            },
            AppliedTags = tags.ToList()
        };

        var response = await _httpClient.PostAsJsonAsync($"channels/{channelId}/threads", body, JsonOptions);
        response.EnsureSuccessStatusCode();
    }
}