using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private bool _showExitExamDialog;

    public MainViewModel()
    {
        NavigateToDeckList();
    }

    [RelayCommand]
    public void RequestNavigateToDeckList()
    {
        // Si estamos en un examen en curso, mostrar confirmación
        if (CurrentView is ExamViewModel examVm && !examVm.ExamFinished)
        {
            ShowExitExamDialog = true;
        }
        else
        {
            NavigateToDeckList();
        }
    }

    [RelayCommand]
    public void ConfirmExitExam()
    {
        ShowExitExamDialog = false;
        NavigateToDeckList();
    }

    [RelayCommand]
    public void CancelExitExam()
    {
        ShowExitExamDialog = false;
    }

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