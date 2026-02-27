using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TestApp.Core.Data;
using TestApp.Core.Models;
using TestApp.Core.Services;
using TestApp.Desktop.Services;
using TestApp.Desktop.ViewModels;

namespace TestApp.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        InitializeDatabase();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void InitializeDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        EnsureLocalUserExists(context);
    }

    private static void EnsureLocalUserExists(AppDbContext context)
    {
        var localUser = context.Users.Find(DesktopUserConstants.UserId);
        if (localUser != null) return;

        context.Users.Add(new User
        {
            Id = DesktopUserConstants.UserId,
            UserName = "local",
            NormalizedUserName = "LOCAL",
            Email = "local@testapp.desktop",
            NormalizedEmail = "LOCAL@TESTAPP.DESKTOP",
            EmailConfirmed = true,
            FullName = DesktopUserConstants.FullName,
            SecurityStamp = Guid.NewGuid().ToString()
        });
        context.SaveChanges();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Data
        services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);

        // Services
        services.AddTransient<IDeckService, DeckService>();
        services.AddTransient<IQuestionService, QuestionService>();
        services.AddTransient<IPdfImportService, PdfImportService>();
        services.AddTransient<IStatisticsService, StatisticsService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<DeckListViewModel>(sp =>
            new DeckListViewModel(
                sp.GetRequiredService<IDeckService>(),
                sp.GetRequiredService<IQuestionService>(),
                sp.GetRequiredService<IPdfImportService>(),
                sp.GetRequiredService<IStatisticsService>(),
                DesktopUserConstants.UserId));
        services.AddTransient<Func<Action, ExamViewModel>>(sp => goHomeAction =>
            new ExamViewModel(sp.GetRequiredService<IQuestionService>(), goHomeAction));

        // Views
        services.AddTransient<MainWindow>();
    }
}
