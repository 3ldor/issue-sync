using System.Text.Json;
using System.Text.Json.Serialization;
using IssueSync.Models;
using IssueSync.Utils;

namespace IssueSync.Services;

public class DiscordService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = new SnakeCaseNamingPolicy(), DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull};
    
    private readonly HttpClient _httpClient;
    
    public DiscordService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Discord");
    }

    public async Task<string> CreatePost(string channelId, string title, string message, IEnumerable<string> tags)
    {
        // Build body
        var body = new DiscordThreadStart
        {
            Name = title,
            Message = new DiscordThreadMessage
            {
                Content = message
            },
            AppliedTags = tags.ToList()
        };

        var response = await _httpClient.PostAsJsonAsync($"channels/{channelId}/threads", body, JsonOptions);
        response.EnsureSuccessStatusCode();

        var channel = await response.Content.ReadFromJsonAsync<DiscordChannel>(JsonOptions);
        if (channel == null)
        {
            throw new HttpRequestException("Failed to parse response.");
        }

        return channel.Id;
    }

    public async Task SetChannelArchived(string channelId, bool archived)
    {
        // Build body
        var body = new DiscordModifyChannel
        {
            Archived = archived
        };
        
        var response = await _httpClient.PatchAsync($"channels/{channelId}", JsonContent.Create(body, null, JsonOptions));
        response.EnsureSuccessStatusCode();
    }
}