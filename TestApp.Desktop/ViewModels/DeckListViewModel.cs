using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TestApp.Core.Models;
using TestApp.Core.Services;

namespace TestApp.Desktop.ViewModels;

public partial class DeckListViewModel : ObservableObject
{
    private readonly IDeckService _deckService;
    private readonly IQuestionService _questionService;
    private readonly IPdfImportService _pdfImportService;
    private Action<ExamViewModel>? _navigateToExamAction;
    private Func<ExamViewModel>? _createExamViewModelFunc;

    [ObservableProperty]
    private ObservableCollection<Deck> _decks = [];

    [ObservableProperty]
    private Deck? _selectedDeck;

    [ObservableProperty]
    private ObservableCollection<QuestionFile> _selectedDeckFiles = [];

    [ObservableProperty]
    private string _newDeckName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _pdfQuestionCount = 100;

    [ObservableProperty]
    private QuestionFile? _selectedFile;

    [ObservableProperty]
    private bool _showQuestionList;

    [ObservableProperty]
    private ObservableCollection<EditableQuestion> _questionsToShow = [];

    [ObservableProperty]
    private bool _isEditMode;

    // Opciones de examen
    [ObservableProperty]
    private bool _showExamOptions;

    [ObservableProperty]
    private bool _randomQuestions = true;

    [ObservableProperty]
    private bool _randomAnswers;

    [ObservableProperty]
    private bool _reviewMode = true;

    [ObservableProperty]
    private QuestionFile? _examTargetFile;

    // Diálogo de discrepancia de preguntas PDF (cuando hay MÁS preguntas detectadas)
    [ObservableProperty]
    private bool _showPdfMismatchDialog;

    [ObservableProperty]
    private int _pdfDetectedQuestions;

    [ObservableProperty]
    private string _pendingPdfPath = string.Empty;

    [ObservableProperty]
    private string _pendingPdfFileName = string.Empty;

    // Diálogo de aviso (cuando hay MENOS preguntas detectadas)
    [ObservableProperty]
    private bool _showPdfWarningDialog;

    [ObservableProperty]
    private string _warningMessage = string.Empty;

    // Loading
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _loadingMessage = string.Empty;

    // Diálogo de confirmación de borrado de mazo
    [ObservableProperty]
    private bool _showDeleteDeckDialog;

    [ObservableProperty]
    private Deck? _deckToDelete;

    // Opciones de examen del mazo
    [ObservableProperty]
    private bool _showDeckExamOptions;

    [ObservableProperty]
    private int _deckExamQuestionCount;

    [ObservableProperty]
    private bool _deckExamRandomQuestions = true;

    [ObservableProperty]
    private bool _deckExamRandomAnswers;

    [ObservableProperty]
    private bool _deckExamReviewMode = true;

    public DeckListViewModel(
        IDeckService deckService,
        IQuestionService questionService,
        IPdfImportService pdfImportService)
    {
        _deckService = deckService;
        _questionService = questionService;
        _pdfImportService = pdfImportService;
        LoadDecksCommand.Execute(null);
    }

    public void Initialize(Func<ExamViewModel> createExamViewModelFunc, Action<ExamViewModel> navigateToExamAction)
    {
        _createExamViewModelFunc = createExamViewModelFunc;
        _navigateToExamAction = navigateToExamAction;
    }

    partial void OnSelectedDeckChanged(Deck? value)
    {
        if (value != null)
        {
            SelectedDeckFiles = new ObservableCollection<QuestionFile>(value.Files);
        }
        else
        {
            SelectedDeckFiles.Clear();
        }
        ShowQuestionList = false;
        ShowExamOptions = false;
    }

    [RelayCommand]
    private async Task LoadDecks()
    {
        var decks = await _deckService.GetAllDecksAsync();
        Decks = new ObservableCollection<Deck>(decks);
    }

    [RelayCommand]
    private async Task CreateDeck()
    {
        if (string.IsNullOrWhiteSpace(NewDeckName)) return;

        await _deckService.CreateDeckAsync(NewDeckName);
        NewDeckName = string.Empty;
        await LoadDecks();
        StatusMessage = "✅ Mazo creado correctamente";
    }

    [RelayCommand]
    private async Task DeleteDeck(Deck deck)
    {
        // Si el mazo tiene archivos, pedir confirmación
        if (deck.Files.Count > 0)
        {
            DeckToDelete = deck;
            ShowDeleteDeckDialog = true;
        }
        else
        {
            // Eliminar directamente y recargar la lista
            await _deckService.DeleteDeckAsync(deck.Id);

            // Si el mazo eliminado estaba seleccionado, limpiamos la selección
            if (SelectedDeck != null && SelectedDeck.Id == deck.Id)
            {
                SelectedDeck = null;
            }

            await LoadDecks();
            StatusMessage = $"🗑️ Mazo '{deck.Name}' eliminado";
        }
    }

    [RelayCommand]
    private async Task ConfirmDeleteDeck()
    {
        if (DeckToDelete == null) return;

        ShowDeleteDeckDialog = false;
        await _deckService.DeleteDeckAsync(DeckToDelete.Id);

        // Si el mazo eliminado estaba seleccionado, limpiamos la selección
        if (SelectedDeck != null && SelectedDeck.Id == DeckToDelete.Id)
        {
            SelectedDeck = null;
        }

        await LoadDecks();
        StatusMessage = $"🗑️ Mazo '{DeckToDelete.Name}' eliminado";
        DeckToDelete = null;
    }

    [RelayCommand]
    private void CancelDeleteDeck()
    {
        ShowDeleteDeckDialog = false;
        DeckToDelete = null;
    }

    [RelayCommand]
    private async Task DeleteFile(QuestionFile file)
    {
        if (SelectedDeck == null || file == null) return;

        // Borrar en BD
        await _questionService.DeleteFileAsync(file.Id);

        // Quitar de la lista mostrada a la derecha (SelectedDeckFiles)
        var toRemove = SelectedDeckFiles?.FirstOrDefault(f => f.Id == file.Id);
        if (toRemove != null)
        {
            SelectedDeckFiles.Remove(toRemove);
        }

        // Forzar actualización del mazo en la lista principal:
        // reemplazamos la instancia del Deck en 'Decks' por una nueva copia sin el archivo eliminado.
        var deckIndex = Decks?.Select((d, i) => (deck: d, index: i)).FirstOrDefault(x => x.deck.Id == SelectedDeck.Id).index ?? -1;
        if (deckIndex >= 0)
        {
            var old = Decks[deckIndex];
            var newDeck = new Deck
            {
                Id = old.Id,
                Name = old.Name,
                CreatedAt = old.CreatedAt,
                Files = old.Files.Where(f => f.Id != file.Id).ToList()
            };

            Decks[deckIndex] = newDeck;

            // Si el mazo actual era el seleccionado, actualizamos la referencia para que se reevalúen bindings relacionados
            SelectedDeck = newDeck;
        }

        StatusMessage = $"🗑️ Archivo '{file.Name}' eliminado";
    }

    [RelayCommand]
    private void ShowExamOptionsDialog(QuestionFile file)
    {
        ExamTargetFile = file;
        RandomQuestions = true;
        RandomAnswers = false;
        ReviewMode = true;
        ShowExamOptions = true;
    }

    [RelayCommand]
    private void CancelExamOptions()
    {
        ShowExamOptions = false;
        ExamTargetFile = null;
    }

    [RelayCommand]
    private async Task StartExamWithOptions()
    {
        if (ExamTargetFile == null || _createExamViewModelFunc == null || _navigateToExamAction == null) return;

        var examVm = _createExamViewModelFunc();
        await examVm.StartExamFromFileAsync(
            ExamTargetFile.Id, 
            ExamTargetFile.Questions.Count, 
            QuestionFilter.All,
            RandomQuestions,
            RandomAnswers,
            ReviewMode);
        
        ShowExamOptions = false;
        ExamTargetFile = null;
        _navigateToExamAction(examVm);
    }

    [RelayCommand]
    private async Task ShowFileQuestions(QuestionFile file)
    {
        SelectedFile = file;
        IsEditMode = false;
        var questions = await _questionService.GetAllQuestionsFromFileOrderedAsync(file.Id);
        QuestionsToShow = new ObservableCollection<EditableQuestion>(
            questions.Select(q => new EditableQuestion(q)));
        ShowQuestionList = true;
    }

    [RelayCommand]
    private void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;
    }

    [RelayCommand]
    private async Task SaveCorrectAnswer(EditableQuestion editableQuestion)
    {
        if (editableQuestion.SelectedAnswer == null) return;

        await _questionService.UpdateCorrectAnswerAsync(
            editableQuestion.Question.Id,
            editableQuestion.SelectedAnswer.Value);

        editableQuestion.Question.CorrectAnswer = editableQuestion.SelectedAnswer.Value;
        editableQuestion.NotifyQuestionChanged();

        StatusMessage = $"✅ Pregunta {editableQuestion.Question.Number} actualizada";
    }

    [RelayCommand]
    private void CloseQuestionList()
    {
        ShowQuestionList = false;
        SelectedFile = null;
        IsEditMode = false;
    }

    [RelayCommand]
    private async Task ImportFile()
    {
        if (SelectedDeck == null)
        {
            StatusMessage = "⚠️ Selecciona un mazo primero";
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Archivos PDF (*.pdf)|*.pdf",
            Title = "Seleccionar archivo PDF de preguntas"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Analizando PDF...";
                StatusMessage = "⏳ Analizando PDF...";
                
                var detectedCount = await _pdfImportService.CountQuestionsInPdfAsync(dialog.FileName);

                IsLoading = false;

                if (detectedCount < PdfQuestionCount)
                {
                    // Caso 1: Detectadas MENOS de las configuradas -> Importar las detectadas y avisar
                    PendingPdfPath = dialog.FileName;
                    PendingPdfFileName = Path.GetFileNameWithoutExtension(dialog.FileName);
                    PdfDetectedQuestions = detectedCount;
                    WarningMessage = $"Se han detectado solo {detectedCount} preguntas de las {PdfQuestionCount} configuradas. Se importarán las {detectedCount} preguntas encontradas.";
                    ShowPdfWarningDialog = true;
                }
                else if (detectedCount > PdfQuestionCount)
                {
                    // Caso 2: Detectadas MÁS de las configuradas -> Mostrar diálogo de elección
                    PendingPdfPath = dialog.FileName;
                    PendingPdfFileName = Path.GetFileNameWithoutExtension(dialog.FileName);
                    PdfDetectedQuestions = detectedCount;
                    ShowPdfMismatchDialog = true;
                    StatusMessage = $"⚠️ Se detectaron {detectedCount} preguntas, pero configuraste {PdfQuestionCount}";
                }
                else
                {
                    // Caso 3: Coinciden exactamente -> Importar directamente
                    await ImportPdfWithCount(dialog.FileName, PdfQuestionCount);
                }
            }
            catch (Exception ex)
            {
                IsLoading = false;
                StatusMessage = $"❌ Error al importar: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task ImportDetectedQuestions()
    {
        ShowPdfMismatchDialog = false;
        await ImportPdfWithCount(PendingPdfPath, PdfDetectedQuestions);
    }

    [RelayCommand]
    private async Task ImportConfiguredQuestions()
    {
        ShowPdfMismatchDialog = false;
        await ImportPdfWithCount(PendingPdfPath, PdfQuestionCount);
    }

    [RelayCommand]
    private void CancelPdfImport()
    {
        ShowPdfMismatchDialog = false;
        PendingPdfPath = string.Empty;
        PendingPdfFileName = string.Empty;
        StatusMessage = "❌ Importación cancelada";
    }

    [RelayCommand]
    private async Task AcceptWarningAndImport()
    {
        ShowPdfWarningDialog = false;
        await ImportPdfWithCount(PendingPdfPath, PdfDetectedQuestions);
    }

    [RelayCommand]
    private void CancelWarning()
    {
        ShowPdfWarningDialog = false;
        PendingPdfPath = string.Empty;
        PendingPdfFileName = string.Empty;
        StatusMessage = "❌ Importación cancelada";
    }

    private async Task ImportPdfWithCount(string pdfPath, int questionCount)
    {
        try
        {
            IsLoading = true;
            LoadingMessage = $"Importando {questionCount} preguntas...";
            
            var fileName = Path.GetFileNameWithoutExtension(pdfPath);
            StatusMessage = $"⏳ Procesando PDF ({questionCount} preguntas)...";
            
            var questions = await _pdfImportService.ExtractQuestionsFromPdfAsync(pdfPath, questionCount);
            
            LoadingMessage = "Guardando en la base de datos...";
            var count = await _questionService.ImportQuestionsAsync(SelectedDeck!.Id, fileName, questions);
            
            LoadingMessage = "Actualizando interfaz...";
            var deckId = SelectedDeck.Id;
            SelectedDeck = null;
            await LoadDecks();
            SelectedDeck = Decks.FirstOrDefault(d => d.Id == deckId);
            
            IsLoading = false;
            StatusMessage = $"✅ Archivo '{fileName}' importado correctamente ({count} preguntas)";
        }
        catch (Exception ex)
        {
            IsLoading = false;
            StatusMessage = $"❌ Error al importar: {ex.Message}";
        }
        finally
        {
            PendingPdfPath = string.Empty;
            PendingPdfFileName = string.Empty;
        }
    }

    [RelayCommand]
    private void StartDeckExam()
    {
        if (SelectedDeck == null || !SelectedDeckFiles.Any()) return;
        if (_createExamViewModelFunc == null || _navigateToExamAction == null) return;

        var totalQuestions = SelectedDeckFiles.Sum(f => f.Questions.Count);
        DeckExamQuestionCount = totalQuestions;
        DeckExamReviewMode = true;
        ShowDeckExamOptions = true;
    }

    [RelayCommand]
    private async Task StartDeckExamWithOptions()
    {
        if (SelectedDeck == null || _createExamViewModelFunc == null || _navigateToExamAction == null) return;

        ShowDeckExamOptions = false;
        var examVm = _createExamViewModelFunc();
        // El examen del mazo siempre tiene preguntas y respuestas aleatorias
        await examVm.StartExamAsync(
            SelectedDeck.Id,
            DeckExamQuestionCount,
            QuestionFilter.All,
            randomQuestions: true,
            randomAnswers: true,
            DeckExamReviewMode);

        _navigateToExamAction(examVm);
    }

    [RelayCommand]
    private void CancelDeckExamOptions()
    {
        ShowDeckExamOptions = false;
    }
}

public partial class EditableQuestion : ObservableObject
{
    public Question Question { get; }

    [ObservableProperty]
    private char? _selectedAnswer;

    public EditableQuestion(Question question)
    {
        Question = question;
        SelectedAnswer = question.CorrectAnswer;
    }

    public void NotifyQuestionChanged()
    {
        OnPropertyChanged(nameof(Question));
    }
}