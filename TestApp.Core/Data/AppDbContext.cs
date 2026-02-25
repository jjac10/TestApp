using Microsoft.EntityFrameworkCore;
using TestApp.Core.Models;

namespace TestApp.Core.Data;

public class AppDbContext : DbContext
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

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}