using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TestApp.Core.Models;
using TestApp.Core.Services;

namespace TestApp.Desktop.ViewModels;

public partial class ExamViewModel : ObservableObject
{
    private readonly IQuestionService _questionService;
    private readonly Action _goHomeAction;
    private List<Question> _questions = [];
    private List<Question> _failedQuestions = [];
    private List<(Question Question, char UserAnswer)> _allAnswers = [];
    private int _currentIndex;
    private bool _shuffleAnswers;
    private bool _isReviewModeEnabled;
    
    // Mapeo de respuestas barajadas para la pregunta actual
    // Clave: letra mostrada (A,B,C,D) -> Valor: letra original
    private Dictionary<char, char> _displayToOriginalMapping = [];
    // Clave: letra original -> Valor: letra mostrada (A,B,C,D)
    private Dictionary<char, char> _originalToDisplayMapping = [];

    [ObservableProperty]
    private Question? _currentQuestion;

    [ObservableProperty]
    private ShuffledQuestion? _displayQuestion;

    [ObservableProperty]
    private char? _selectedAnswer;

    [ObservableProperty]
    private bool _showResult;

    [ObservableProperty]
    private bool _isCorrect;

    [ObservableProperty]
    private int _correctCount;

    [ObservableProperty]
    private int _totalAnswered;

    [ObservableProperty]
    private bool _examFinished;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private int _questionsCount;

    [ObservableProperty]
    private bool _showReviewMode;

    [ObservableProperty]
    private ObservableCollection<ReviewQuestion> _reviewQuestions = [];

    // Letra correcta como se muestra en pantalla (después del barajado)
    [ObservableProperty]
    private char _displayCorrectAnswer;

    // Option selection properties
    [ObservableProperty]
    private bool _isOptionASelected;

    [ObservableProperty]
    private bool _isOptionBSelected;

    [ObservableProperty]
    private bool _isOptionCSelected;

    [ObservableProperty]
    private bool _isOptionDSelected;

    public string ResultText => $"{CorrectCount} de {QuestionsCount} correctas";
    public string PercentageText => QuestionsCount > 0 
        ? $"{(CorrectCount * 100 / QuestionsCount)}% de aciertos" 
        : "0%";
    
    public int FailedCount => _failedQuestions.Count;
    public bool HasFailedQuestions => _failedQuestions.Count > 0;

    public ExamViewModel(IQuestionService questionService, Action goHomeAction)
    {
        _questionService = questionService;
        _goHomeAction = goHomeAction;
    }

    partial void OnIsOptionASelectedChanged(bool value) { if (value) SelectedAnswer = 'A'; }
    partial void OnIsOptionBSelectedChanged(bool value) { if (value) SelectedAnswer = 'B'; }
    partial void OnIsOptionCSelectedChanged(bool value) { if (value) SelectedAnswer = 'C'; }
    partial void OnIsOptionDSelectedChanged(bool value) { if (value) SelectedAnswer = 'D'; }

    public async Task StartExamAsync(int deckId, int questionCount, QuestionFilter filter, 
        bool randomQuestions = true, bool randomAnswers = false, bool reviewMode = true)
    {
        _questions = await _questionService.GetQuestionsForExamAsync(deckId, questionCount, filter);
        
        if (!randomQuestions)
        {
            _questions = _questions.OrderBy(q => q.Number).ToList();
        }
        
        _shuffleAnswers = randomAnswers;
        _isReviewModeEnabled = reviewMode;
        InitializeExam();
    }

    public async Task StartExamFromFileAsync(int fileId, int questionCount, QuestionFilter filter,
        bool randomQuestions = true, bool randomAnswers = false, bool reviewMode = true)
    {
        _questions = await _questionService.GetQuestionsFromFileAsync(fileId, questionCount, filter);
        
        if (!randomQuestions)
        {
            _questions = _questions.OrderBy(q => q.Number).ToList();
        }
        
        _shuffleAnswers = randomAnswers;
        _isReviewModeEnabled = reviewMode;
        InitializeExam();
    }

    private void InitializeExam()
    {
        _currentIndex = 0;
        CorrectCount = 0;
        TotalAnswered = 0;
        ExamFinished = false;
        ShowReviewMode = false;
        _failedQuestions.Clear();
        _allAnswers.Clear();
        QuestionsCount = _questions.Count;
        LoadCurrentQuestion();
    }

    private void LoadCurrentQuestion()
    {
        if (_currentIndex < _questions.Count)
        {
            CurrentQuestion = _questions[_currentIndex];
            
            if (_shuffleAnswers)
            {
                DisplayQuestion = CreateShuffledQuestion(CurrentQuestion);
            }
            else
            {
                DisplayQuestion = new ShuffledQuestion
                {
                    Number = CurrentQuestion.Number,
                    Statement = CurrentQuestion.Statement,
                    OptionA = CurrentQuestion.OptionA,
                    OptionB = CurrentQuestion.OptionB,
                    OptionC = CurrentQuestion.OptionC,
                    OptionD = CurrentQuestion.OptionD,
                    Source = CurrentQuestion.Source
                };
                _displayToOriginalMapping = new Dictionary<char, char>
                {
                    { 'A', 'A' }, { 'B', 'B' }, { 'C', 'C' }, { 'D', 'D' }
                };
                _originalToDisplayMapping = new Dictionary<char, char>
                {
                    { 'A', 'A' }, { 'B', 'B' }, { 'C', 'C' }, { 'D', 'D' }
                };
            }
            
            // Calcular qué letra se muestra como correcta
            DisplayCorrectAnswer = _originalToDisplayMapping[CurrentQuestion.CorrectAnswer];
            
            SelectedAnswer = null;
            IsOptionASelected = false;
            IsOptionBSelected = false;
            IsOptionCSelected = false;
            IsOptionDSelected = false;
            ShowResult = false;
            ProgressText = $"Pregunta {_currentIndex + 1} de {_questions.Count}";
        }
        else
        {
            ExamFinished = true;
            OnPropertyChanged(nameof(ResultText));
            OnPropertyChanged(nameof(PercentageText));
            OnPropertyChanged(nameof(FailedCount));
            OnPropertyChanged(nameof(HasFailedQuestions));
        }
    }

    private ShuffledQuestion CreateShuffledQuestion(Question question)
    {
        var options = new List<(char Original, string Text)>
        {
            ('A', question.OptionA),
            ('B', question.OptionB),
            ('C', question.OptionC),
            ('D', question.OptionD)
        };
        
        // Barajar opciones
        var shuffled = options.OrderBy(_ => Random.Shared.Next()).ToList();
        
        var displayLetters = new[] { 'A', 'B', 'C', 'D' };
        
        // Crear mapeo: letra mostrada -> letra original
        _displayToOriginalMapping = new Dictionary<char, char>();
        _originalToDisplayMapping = new Dictionary<char, char>();
        
        for (int i = 0; i < 4; i++)
        {
            _displayToOriginalMapping[displayLetters[i]] = shuffled[i].Original;
            _originalToDisplayMapping[shuffled[i].Original] = displayLetters[i];
        }
        
        return new ShuffledQuestion
        {
            Number = question.Number,
            Statement = question.Statement,
            OptionA = shuffled[0].Text,
            OptionB = shuffled[1].Text,
            OptionC = shuffled[2].Text,
            OptionD = shuffled[3].Text,
            Source = question.Source
        };
    }

    [RelayCommand]
    private async Task ConfirmAnswer()
    {
        if (SelectedAnswer == null || CurrentQuestion == null) return;

        // Convertir respuesta mostrada a respuesta original
        var originalAnswer = _displayToOriginalMapping[SelectedAnswer.Value];
        
        await _questionService.RecordAnswerAsync(CurrentQuestion.Id, originalAnswer);

        IsCorrect = originalAnswer == CurrentQuestion.CorrectAnswer;
        
        // Guardar respuesta para repaso (guardamos la letra original)
        _allAnswers.Add((CurrentQuestion, originalAnswer));
        
        if (IsCorrect)
        {
            CorrectCount++;
        }
        else
        {
            _failedQuestions.Add(CurrentQuestion);
        }
        
        TotalAnswered++;
        
        // Si está en modo revisión, mostrar resultado
        // Si está en modo examen final, pasar directamente a la siguiente
        if (_isReviewModeEnabled)
        {
            ShowResult = true;
        }
        else
        {
            // Modo examen final: pasar a la siguiente pregunta automáticamente
            _currentIndex++;
            LoadCurrentQuestion();
        }
    }

    [RelayCommand]
    private void NextQuestion()
    {
        _currentIndex++;
        LoadCurrentQuestion();
    }

    [RelayCommand]
    private void ReviewAllQuestions()
    {
        ReviewQuestions = new ObservableCollection<ReviewQuestion>(
            _allAnswers.Select(a => new ReviewQuestion
            {
                Question = a.Question,
                UserAnswer = a.UserAnswer,
                IsCorrect = a.UserAnswer == a.Question.CorrectAnswer
            }));
        ShowReviewMode = true;
    }

    [RelayCommand]
    private void RetryFailedQuestions()
    {
        if (_failedQuestions.Count == 0) return;
        
        _questions = new List<Question>(_failedQuestions);
        InitializeExam();
    }

    [RelayCommand]
    private void CloseReview()
    {
        ShowReviewMode = false;
    }

    [RelayCommand]
    private void GoHome()
    {
        _goHomeAction?.Invoke();
    }
}

public class ShuffledQuestion
{
    public int Number { get; set; }
    public string Statement { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string? Source { get; set; }
}

public class ReviewQuestion
{
    public Question Question { get; set; } = null!;
    public char UserAnswer { get; set; }
    public bool IsCorrect { get; set; }
}