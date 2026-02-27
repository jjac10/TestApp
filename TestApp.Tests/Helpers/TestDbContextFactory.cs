using Microsoft.EntityFrameworkCore;
using TestApp.Core.Data;
using TestApp.Core.Models;

namespace TestApp.Tests.Helpers;

public static class TestDbContextFactory
{
    public const string TestUserId = "test-user";

    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        SeedTestUser(context);
        return context;
    }

    private static void SeedTestUser(AppDbContext context)
    {
        if (context.Users.Find(TestUserId) != null) return;

        context.Users.Add(new User
        {
            Id = TestUserId,
            UserName = "test",
            NormalizedUserName = "TEST",
            Email = "test@test.com",
            NormalizedEmail = "TEST@TEST.COM",
            EmailConfirmed = true,
            FullName = "Test User",
            SecurityStamp = System.Guid.NewGuid().ToString()
        });
        context.SaveChanges();
    }
}