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
    private readonly IStatisticsService _statisticsService;
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

    [ObservableProperty]
    private QuestionFilter _selectedQuestionFilter = QuestionFilter.All;

    [ObservableProperty]
    private int _availableQuestionsForFilter;

    // Diálogo de confirmación de importación PDF
    [ObservableProperty]
    private bool _showPdfConfirmDialog;

    [ObservableProperty]
    private int _pdfDetectedQuestions;

    [ObservableProperty]
    private string _pendingPdfPath = string.Empty;

    [ObservableProperty]
    private string _pendingPdfFileName = string.Empty;

    // Importación múltiple de PDFs
    [ObservableProperty]
    private ObservableCollection<PdfImportInfo> _pendingPdfImports = [];

    [ObservableProperty]
    private int _totalDetectedQuestions;

    [ObservableProperty]
    private int _duplicateFilesCount;

    // Diálogo de archivo duplicado
    [ObservableProperty]
    private bool _showDuplicateFileDialog;

    [ObservableProperty]
    private string _duplicateFileName = string.Empty;

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

    [ObservableProperty]
    private string _deleteDeckConfirmName = string.Empty;

    [ObservableProperty]
    private string _deleteDeckError = string.Empty;

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

    [ObservableProperty]
    private QuestionFilter _selectedDeckQuestionFilter = QuestionFilter.All;

    [ObservableProperty]
    private int _availableDeckQuestionsForFilter;

    // Estadísticas de archivo
    [ObservableProperty]
    private bool _showFileStatisticsDialog;

    [ObservableProperty]
    private FileStatistics? _currentFileStatistics;

    // Estadísticas de mazo
    [ObservableProperty]
    private bool _showDeckStatisticsDialog;

    [ObservableProperty]
    private DeckStatistics? _currentDeckStatistics;

    // Historial de progreso
    [ObservableProperty]
    private ProgressHistory? _currentFileProgressHistory;

    [ObservableProperty]
    private ProgressHistory? _currentDeckProgressHistory;

    // Diálogo de confirmación de borrado de pregunta
    [ObservableProperty]
    private bool _showDeleteQuestionDialog;

    [ObservableProperty]
    private EditableQuestion? _questionToDelete;

    private CancellationTokenSource? _statusMessageCts;

    public DeckListViewModel(
        IDeckService deckService,
        IQuestionService questionService,
        IPdfImportService pdfImportService,
        IStatisticsService statisticsService)
    {
        _deckService = deckService;
        _questionService = questionService;
        _pdfImportService = pdfImportService;
        _statisticsService = statisticsService;
        LoadDecksCommand.Execute(null);
    }

    public void Initialize(Func<ExamViewModel> createExamViewModelFunc, Action<ExamViewModel> navigateToExamAction)
    {
        _createExamViewModelFunc = createExamViewModelFunc;
        _navigateToExamAction = navigateToExamAction;
    }

    private bool _suppressDeckChangeHandling = false;

    partial void OnSelectedDeckChanged(Deck? value)
    {
        // Si la bandera está activa, no hacer nada
        if (_suppressDeckChangeHandling) return;
        
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
        var trimmedName = NewDeckName.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            StatusMessage = "⚠️ El nombre del mazo no puede estar vacío.";
            return;
        }

        // Verificar si ya existe un mazo con el mismo nombre
        var existingDeck = Decks.FirstOrDefault(d =>
            string.Equals(d.Name, trimmedName, StringComparison.OrdinalIgnoreCase));

        if (existingDeck != null)
        {
            StatusMessage = $"⚠️ Ya existe un mazo con el nombre '{trimmedName}'.";
            return;
        }

        await _deckService.CreateDeckAsync(trimmedName);
        NewDeckName = string.Empty;
        await LoadDecks();
        StatusMessage = "✅ Mazo creado correctamente";
    }

    [RelayCommand]
    private async Task DeleteDeck(Deck deck)
    {
        // Si el mazo tiene archivos, pedir confirmación con validación de nombre
        if (deck.Files.Count > 0)
        {
            DeckToDelete = deck;
            DeleteDeckConfirmName = string.Empty;
            DeleteDeckError = string.Empty;
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

        // Validar que el nombre coincida
        if (!string.Equals(DeleteDeckConfirmName?.Trim(), DeckToDelete.Name, StringComparison.OrdinalIgnoreCase))
        {
            DeleteDeckError = "El nombre no coincide. Escribe el nombre exacto del mazo.";
            return;
        }

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
        DeleteDeckConfirmName = string.Empty;
        DeleteDeckError = string.Empty;
    }

    [RelayCommand]
    private void CancelDeleteDeck()
    {
        ShowDeleteDeckDialog = false;
        DeckToDelete = null;
        DeleteDeckConfirmName = string.Empty;
        DeleteDeckError = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteFile(QuestionFile file)
    {
        if (SelectedDeck == null || file == null) return;

        var deckId = SelectedDeck.Id;
        var fileName = file.Name;
        
        // Borrar en BD
        await _questionService.DeleteFileAsync(file.Id);

        // Recargar todos los datos frescos de la BD
        SelectedDeck = null;
        await LoadDecks();
        
        // Re-seleccionar el mazo
        SelectedDeck = Decks.FirstOrDefault(d => d.Id == deckId);

        StatusMessage = $"🗑️ Archivo '{fileName}' eliminado";
    }

    [RelayCommand]
    private async Task ShowExamOptionsDialog(QuestionFile file)
    {
        ExamTargetFile = file;
        RandomQuestions = false;
        RandomAnswers = false;
        ReviewMode = true;
        SelectedQuestionFilter = QuestionFilter.All;
        AvailableQuestionsForFilter = file.Questions.Count;
        ShowExamOptions = true;
        
        // Cargar conteo inicial
        await UpdateAvailableQuestionsCount();
    }

    partial void OnSelectedQuestionFilterChanged(QuestionFilter value)
    {
        _ = UpdateAvailableQuestionsCount();
    }

    private async Task UpdateAvailableQuestionsCount()
    {
        if (ExamTargetFile == null) return;
        AvailableQuestionsForFilter = await _questionService.CountQuestionsInFileAsync(ExamTargetFile.Id, SelectedQuestionFilter);
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
        if (AvailableQuestionsForFilter == 0)
        {
            StatusMessage = "⚠️ No hay preguntas disponibles con el filtro seleccionado";
            return;
        }

        var examVm = _createExamViewModelFunc();
        await examVm.StartExamFromFileAsync(
            ExamTargetFile.Id, 
            AvailableQuestionsForFilter, 
            SelectedQuestionFilter,
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
    private void DeleteQuestion(EditableQuestion editableQuestion)
    {
        if (editableQuestion == null) return;

        QuestionToDelete = editableQuestion;
        ShowDeleteQuestionDialog = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteQuestion()
    {
        if (QuestionToDelete == null) return;

        var questionNumber = QuestionToDelete.Question.Number;

        await _questionService.DeleteQuestionAsync(QuestionToDelete.Question.Id);

        // Quitar de la lista visual
        QuestionsToShow.Remove(QuestionToDelete);

        ShowDeleteQuestionDialog = false;
        QuestionToDelete = null;

        // Actualizar los conteos del mazo sin salir de la vista de edición
        if (SelectedDeck != null)
        {
            var deckId = SelectedDeck.Id;
            
            // Activar la bandera para evitar que OnSelectedDeckChanged cierre la vista
            _suppressDeckChangeHandling = true;
            
            await LoadDecks();
            
            // Actualizar la selección
            var updatedDeck = Decks.FirstOrDefault(d => d.Id == deckId);
            if (updatedDeck != null)
            {
                SelectedDeck = updatedDeck;
                
                // Actualizar manualmente SelectedDeckFiles para refrescar los contadores
                SelectedDeckFiles = new ObservableCollection<QuestionFile>(updatedDeck.Files);
            }
            
            // Desactivar la bandera
            _suppressDeckChangeHandling = false;
        }

        StatusMessage = $"🗑️ Pregunta {questionNumber} eliminada";
    }

    [RelayCommand]
    private void CancelDeleteQuestion()
    {
        ShowDeleteQuestionDialog = false;
        QuestionToDelete = null;
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
            Title = "Seleccionar archivos PDF de preguntas",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true)
        {
            await AnalyzeMultiplePdfsAndShowConfirmDialog(dialog.FileNames);
        }
    }

    private async Task AnalyzeMultiplePdfsAndShowConfirmDialog(string[] filePaths)
    {
        try
        {
            IsLoading = true;
            PendingPdfImports.Clear();
            TotalDetectedQuestions = 0;
            DuplicateFilesCount = 0;

            for (int i = 0; i < filePaths.Length; i++)
            {
                var filePath = filePaths[i];
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                LoadingMessage = $"Analizando PDF {i + 1} de {filePaths.Length}: {fileName}...";

                var isDuplicate = SelectedDeckFiles.Any(f =>
                    string.Equals(f.Name, fileName, StringComparison.OrdinalIgnoreCase));

                var detectedCount = 0;
                if (!isDuplicate)
                {
                    detectedCount = await _pdfImportService.CountQuestionsInPdfAsync(filePath);
                    TotalDetectedQuestions += detectedCount;
                }
                else
                {
                    DuplicateFilesCount++;
                }

                PendingPdfImports.Add(new PdfImportInfo
                {
                    FilePath = filePath,
                    FileName = fileName,
                    DetectedQuestions = detectedCount,
                    IsDuplicate = isDuplicate
                });
            }

            IsLoading = false;

            // Solo mostrar diálogo si hay archivos válidos para importar
            if (PendingPdfImports.Any(p => !p.IsDuplicate))
            {
                ShowPdfConfirmDialog = true;
            }
            else
            {
                StatusMessage = "⚠️ Todos los archivos seleccionados ya existen en el mazo";
                PendingPdfImports.Clear();
            }
        }
        catch (Exception ex)
        {
            IsLoading = false;
            StatusMessage = $"❌ Error al analizar: {ex.Message}";
            PendingPdfImports.Clear();
        }
    }

    [RelayCommand]
    private void CloseDuplicateFileDialog()
    {
        ShowDuplicateFileDialog = false;
        DuplicateFileName = string.Empty;
    }

    [RelayCommand]
    private async Task ConfirmPdfImport()
    {
        ShowPdfConfirmDialog = false;

        var validImports = PendingPdfImports.Where(p => !p.IsDuplicate).ToList();
        var importedCount = 0;
        var totalQuestions = 0;

        try
        {
            IsLoading = true;

            for (int i = 0; i < validImports.Count; i++)
            {
                var pdfInfo = validImports[i];
                LoadingMessage = $"Importando {i + 1} de {validImports.Count}: {pdfInfo.FileName}...";

                var questions = await _pdfImportService.ExtractQuestionsFromPdfAsync(
                    pdfInfo.FilePath, pdfInfo.DetectedQuestions);

                var count = await _questionService.ImportQuestionsAsync(
                    SelectedDeck!.Id, pdfInfo.FileName, questions);

                totalQuestions += count;
                importedCount++;
            }

            // Recargar UI
            LoadingMessage = "Actualizando interfaz...";
            var deckId = SelectedDeck!.Id;
            SelectedDeck = null;
            await LoadDecks();
            SelectedDeck = Decks.FirstOrDefault(d => d.Id == deckId);

            IsLoading = false;
            StatusMessage = $"✅ {importedCount} archivo(s) importado(s) ({totalQuestions} preguntas en total)";
        }
        catch (Exception ex)
        {
            IsLoading = false;
            StatusMessage = $"❌ Error al importar: {ex.Message}";
        }
        finally
        {
            PendingPdfImports.Clear();
            TotalDetectedQuestions = 0;
            DuplicateFilesCount = 0;
        }
    }

    [RelayCommand]
    private void CancelPdfImport()
    {
        ShowPdfConfirmDialog = false;
        PendingPdfImports.Clear();
        TotalDetectedQuestions = 0;
        DuplicateFilesCount = 0;
        PendingPdfPath = string.Empty;
        PendingPdfFileName = string.Empty;
        StatusMessage = "❌ Importación cancelada";
    }

    [RelayCommand]
    private async Task StartDeckExam()
    {
        if (SelectedDeck == null || !SelectedDeckFiles.Any()) return;
        if (_createExamViewModelFunc == null || _navigateToExamAction == null) return;

        var totalQuestions = SelectedDeckFiles.Sum(f => f.Questions.Count);
        DeckExamQuestionCount = totalQuestions;
        DeckExamReviewMode = true;
        SelectedDeckQuestionFilter = QuestionFilter.All;
        AvailableDeckQuestionsForFilter = totalQuestions;
        ShowDeckExamOptions = true;
        
        // Cargar conteo inicial
        await UpdateAvailableDeckQuestionsCount();
    }

    partial void OnSelectedDeckQuestionFilterChanged(QuestionFilter value)
    {
        _ = UpdateAvailableDeckQuestionsCount();
    }

    private async Task UpdateAvailableDeckQuestionsCount()
    {
        if (SelectedDeck == null) return;
        AvailableDeckQuestionsForFilter = await _questionService.CountQuestionsInDeckAsync(SelectedDeck.Id, SelectedDeckQuestionFilter);
        // Actualizar el conteo máximo si cambia el filtro
        if (DeckExamQuestionCount > AvailableDeckQuestionsForFilter)
        {
            DeckExamQuestionCount = AvailableDeckQuestionsForFilter;
        }
    }

    [RelayCommand]
    private async Task StartDeckExamWithOptions()
    {
        if (SelectedDeck == null || _createExamViewModelFunc == null || _navigateToExamAction == null) return;
        if (AvailableDeckQuestionsForFilter == 0)
        {
            StatusMessage = "⚠️ No hay preguntas disponibles con el filtro seleccionado";
            return;
        }

        ShowDeckExamOptions = false;
        var examVm = _createExamViewModelFunc();
        await examVm.StartExamAsync(
            SelectedDeck.Id,
            Math.Min(DeckExamQuestionCount, AvailableDeckQuestionsForFilter),
            SelectedDeckQuestionFilter,
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

    [RelayCommand]
    private async Task ShowFileStatistics(QuestionFile file)
    {
        if (file == null) return;
        
        IsLoading = true;
        LoadingMessage = "Cargando estadísticas...";
        
        CurrentFileStatistics = await _statisticsService.GetFileStatisticsAsync(file.Id);
        CurrentFileProgressHistory = await _statisticsService.GetFileProgressHistoryAsync(file.Id, 14);
        
        IsLoading = false;
        ShowFileStatisticsDialog = true;
    }

    [RelayCommand]
    private void CloseFileStatistics()
    {
        ShowFileStatisticsDialog = false;
        CurrentFileStatistics = null;
        CurrentFileProgressHistory = null;
    }

    [RelayCommand]
    private async Task ShowDeckStatistics()
    {
        if (SelectedDeck == null) return;
        
        IsLoading = true;
        LoadingMessage = "Cargando estadísticas del mazo...";
        
        CurrentDeckStatistics = await _statisticsService.GetDeckStatisticsAsync(SelectedDeck.Id);
        CurrentDeckProgressHistory = await _statisticsService.GetDeckProgressHistoryAsync(SelectedDeck.Id, 14);
        
        IsLoading = false;
        ShowDeckStatisticsDialog = true;
    }

    [RelayCommand]
    private void CloseDeckStatistics()
    {
        ShowDeckStatisticsDialog = false;
        CurrentDeckStatistics = null;
        CurrentDeckProgressHistory = null;
    }

    partial void OnStatusMessageChanged(string value)
    {
        // Si el mensaje está vacío, no hacemos nada
        if (string.IsNullOrWhiteSpace(value)) return;

        // Cancelar cualquier temporizador anterior
        _statusMessageCts?.Cancel();
        _statusMessageCts = new CancellationTokenSource();
        var token = _statusMessageCts.Token;

        // Limpiar el mensaje después de 5 segundos
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(5000, token);
                StatusMessage = string.Empty;
            }
            catch (TaskCanceledException) { }
        }, token);
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