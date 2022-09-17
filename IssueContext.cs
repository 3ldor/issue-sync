using IssueSync.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueSync;

public class IssueContext : DbContext
{
    public DbSet<Issue> Issues { get; set; } = null!;

    public IssueContext(DbContextOptions<IssueContext> options) : base(options)
    {
    }
}