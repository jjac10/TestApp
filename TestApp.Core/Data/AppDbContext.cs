using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestApp.Core.Models;

namespace TestApp.Core.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<QuestionFile> QuestionFiles => Set<QuestionFile>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();

    public string DbPath { get; }

    public AppDbContext()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TestApp");
        Directory.CreateDirectory(folder);
        DbPath = Path.Combine(folder, "examenes.db");
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TestApp");
        Directory.CreateDirectory(folder);
        DbPath = Path.Combine(folder, "examenes.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // ¡Importante para Identity!

        modelBuilder.Entity<Question>()
            .HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuestionFile>()
            .HasMany(f => f.Questions)
            .WithOne(q => q.File)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Deck>()
            .HasMany(d => d.Files)
            .WithOne(f => f.Deck)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Decks)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}