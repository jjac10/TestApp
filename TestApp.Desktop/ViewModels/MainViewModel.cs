using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    public MainViewModel()
    {
        NavigateToDeckList();
    }

    [RelayCommand]
    public void NavigateToDeckList()
    {
        var deckListVm = App.Services.GetRequiredService<DeckListViewModel>();
        deckListVm.Initialize(CreateExamViewModel, NavigateToExam);
        CurrentView = deckListVm;
    }

    public void NavigateToExam(ExamViewModel examVm)
    {
        CurrentView = examVm;
    }

    public ExamViewModel CreateExamViewModel()
    {
        var factory = App.Services.GetRequiredService<Func<Action, ExamViewModel>>();
        return factory(() => NavigateToDeckList());
    }
}