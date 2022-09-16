using System.Net.Http.Headers;
using IssueSync;
using IssueSync.Extensions;
using IssueSync.Models;
using Microsoft.AspNetCore.Mvc;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var gitlabToken = builder.Configuration["GitLabToken"] ?? "";
var discordToken = builder.Configuration["DiscordToken"] ?? "";
var discordChannelId = builder.Configuration["DiscordChannelId"] ?? "";
var discordLabelMap = builder.Configuration.GetSection("DiscordLabelMap").Get<Dictionary<string, string>>();

builder.Services.AddHttpClient("Discord", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://discord.com/api/v10/");
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", discordToken);
    httpClient.Timeout = TimeSpan.FromSeconds(5);
});

// Services
builder.Services.AddScoped<DiscordService>();

// Configure JSON options
builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy());

var app = builder.Build();

// GitLab issue webhook ingress
app.MapPost("/gitlab", async ([FromHeader(Name = "X-Gitlab-Token")] string token, [FromHeader(Name = "X-Gitlab-Event")] string eventType, [FromBody] GitLabIssueEvent eventData, DiscordService discordService) =>
{
    // Check token and event
    if (token != gitlabToken || eventType != "Issue Hook" || eventData.ObjectKind != "issue")
    {
        return Results.BadRequest();
    }
    
    // Only handle new issues
    if (eventData.ObjectAttributes.Action != "open")
    {
        return Results.NoContent();
    }
    
    // Build components
    var message = eventData.ObjectAttributes.Description?.Truncate(2000 - eventData.ObjectAttributes.Url.Length - 4) + "\n\n" + eventData.ObjectAttributes.Url;
    var labels = eventData.ObjectAttributes.Labels.Where(l => discordLabelMap.ContainsKey(l.Id.ToString())).Select(l => discordLabelMap[l.Id.ToString()]);

    // Send to Discord
    await discordService.CreatePost(discordChannelId, eventData.ObjectAttributes.Title, message, labels);

    return Results.NoContent();
});

app.Run();