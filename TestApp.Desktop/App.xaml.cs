using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TestApp.Core.Data;
using TestApp.Core.Services;
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
        services.AddTransient<DeckListViewModel>();
        services.AddTransient<Func<Action, ExamViewModel>>(sp => goHomeAction =>
            new ExamViewModel(sp.GetRequiredService<IQuestionService>(), goHomeAction));

        // Views
        services.AddTransient<MainWindow>();
    }
}
