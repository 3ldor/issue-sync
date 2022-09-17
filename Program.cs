using System.Net.Http.Headers;
using IssueSync;
using IssueSync.Extensions;
using IssueSync.Models;
using IssueSync.Services;
using IssueSync.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var gitlabToken = builder.Configuration["GitLabToken"];
var discordToken = builder.Configuration["DiscordToken"];
var discordChannelId = builder.Configuration["DiscordChannelId"];
var discordLabelMap = builder.Configuration.GetSection("DiscordLabelMap").Get<Dictionary<string, string>>();

// Local database
builder.Services.AddDbContext<IssueContext>(options => options.UseSqlite("Data Source=issues.db"));

// Http client factories
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

// Database migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IssueContext>();
    context.Database.EnsureCreated();
}

// GitLab issue webhook ingress
app.MapPost("/gitlab", async ([FromHeader(Name = "X-Gitlab-Token")] string token, [FromHeader(Name = "X-Gitlab-Event")] string eventType, [FromBody] GitLabIssueEvent eventData, DiscordService discordService, IssueContext context) =>
{
    // Check token and event
    if (token != gitlabToken || eventType != "Issue Hook" || eventData.ObjectKind != "issue")
    {
        return Results.BadRequest();
    }

    var eventAttributes = eventData.ObjectAttributes;

    if (eventAttributes.Action == "open")
    {
        // Build components
        var message = eventAttributes.Description?.Truncate(2000 - eventAttributes.Url.Length - 4) + "\n\n" + eventAttributes.Url;
        var labels = eventAttributes.Labels.Where(l => discordLabelMap.ContainsKey(l.Id.ToString())).Select(l => discordLabelMap[l.Id.ToString()]);

        // Send to Discord
        var channelId = await discordService.CreatePost(discordChannelId, eventAttributes.Title, message, labels);
        
        // Save in database
        context.Issues.Add(new Issue { Id = eventAttributes.Id, DiscordChannelId = channelId });
        await context.SaveChangesAsync();
    }
    else if (eventAttributes.Action == "close" || eventAttributes.Action == "reopen")
    {
        // Lookup in database
        var issue = await context.Issues.FirstOrDefaultAsync(i => i.Id == eventAttributes.Id);
        if (issue != null)
        {
            await discordService.SetChannelArchived(issue.DiscordChannelId, eventAttributes.Action == "close");
        }
    }

    return Results.NoContent();
});

app.Run();