using Moq;
using TestApp.Core.Models;
using TestApp.Core.Services;
using TestApp.Desktop.ViewModels;

namespace TestApp.Tests.Desktop.ViewModels;

public class DeckListViewModelTests
{
    private const string TestUserId = "test-user-id";

    private readonly Mock<IDeckService> _mockDeckService;
    private readonly Mock<IQuestionService> _mockQuestionService;
    private readonly Mock<IPdfImportService> _mockPdfImportService;
    private readonly Mock<IStatisticsService> _mockStatisticsService;

    public DeckListViewModelTests()
    {
        _mockDeckService = new Mock<IDeckService>();
        _mockQuestionService = new Mock<IQuestionService>();
        _mockPdfImportService = new Mock<IPdfImportService>();
        _mockStatisticsService = new Mock<IStatisticsService>();

        // Setup por defecto: GetAllDecksAsync devuelve lista vacía
        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync(new List<Deck>());
    }

    private DeckListViewModel CreateViewModel()
    {
        return new DeckListViewModel(
            _mockDeckService.Object,
            _mockQuestionService.Object,
            _mockPdfImportService.Object,
            _mockStatisticsService.Object,
            TestUserId);
    }

    // --- Crear convocatorias ---

    [Fact]
    public async Task CreateDeck_NombreVacio_MuestraMensajeError()
    {
        var vm = CreateViewModel();
        vm.NewDeckName = "   ";

        await vm.CreateDeckCommand.ExecuteAsync(null);

        Assert.Contains("no puede estar vacío", vm.StatusMessage);
        _mockDeckService.Verify(s => s.CreateDeckAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateDeck_NombreValido_CreaYRecargaLista()
    {
        var vm = CreateViewModel();
        vm.NewDeckName = "Convocatoria 2025";

        _mockDeckService
            .Setup(s => s.CreateDeckAsync("Convocatoria 2025", TestUserId))
            .ReturnsAsync(new Deck { Id = 1, Name = "Convocatoria 2025" });

        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync([new Deck { Id = 1, Name = "Convocatoria 2025" }]);

        await vm.CreateDeckCommand.ExecuteAsync(null);

        Assert.Single(vm.Decks);
        Assert.Equal(string.Empty, vm.NewDeckName);
        Assert.Contains("creada correctamente", vm.StatusMessage);
    }

    [Fact]
    public async Task CreateDeck_NombreDuplicado_MuestraMensajeError()
    {
        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync([new Deck { Id = 1, Name = "Existente" }]);

        var vm = CreateViewModel();
        await Task.Delay(100);

        vm.NewDeckName = "existente";

        await vm.CreateDeckCommand.ExecuteAsync(null);

        Assert.Contains("Ya existe", vm.StatusMessage);
        _mockDeckService.Verify(s => s.CreateDeckAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // --- Eliminar convocatorias ---

    [Fact]
    public async Task DeleteDeck_SinArchivos_EliminaDirectamente()
    {
        var deck = new Deck { Id = 1, Name = "Vacía", Files = [] };

        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync([deck]);

        var vm = CreateViewModel();
        await Task.Delay(100);

        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync([]);

        await vm.DeleteDeckCommand.ExecuteAsync(deck);

        _mockDeckService.Verify(s => s.DeleteDeckAsync(1, TestUserId), Times.Once);
        Assert.Contains("eliminada", vm.StatusMessage);
    }

    [Fact]
    public async Task DeleteDeck_ConArchivos_MuestraDialogoConfirmacion()
    {
        var deck = new Deck
        {
            Id = 1,
            Name = "Con temas",
            Files = [new QuestionFile { Id = 1, Name = "Tema 1" }]
        };

        var vm = CreateViewModel();

        await vm.DeleteDeckCommand.ExecuteAsync(deck);

        Assert.True(vm.ShowDeleteDeckDialog);
        Assert.Equal(deck, vm.DeckToDelete);
        _mockDeckService.Verify(s => s.DeleteDeckAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmDeleteDeck_NombreNoCoincide_MuestraError()
    {
        var deck = new Deck
        {
            Id = 1,
            Name = "Mi Convocatoria",
            Files = [new QuestionFile { Id = 1, Name = "Tema 1" }]
        };

        var vm = CreateViewModel();
        vm.DeckToDelete = deck;
        vm.DeleteDeckConfirmName = "Otro nombre";

        await vm.ConfirmDeleteDeckCommand.ExecuteAsync(null);

        Assert.Contains("no coincide", vm.DeleteDeckError);
        _mockDeckService.Verify(s => s.DeleteDeckAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmDeleteDeck_NombreCorrecto_EliminaYCierraDialogo()
    {
        var deck = new Deck
        {
            Id = 1,
            Name = "Mi Convocatoria",
            Files = [new QuestionFile { Id = 1, Name = "Tema 1" }]
        };

        var vm = CreateViewModel();
        vm.DeckToDelete = deck;
        vm.DeleteDeckConfirmName = "Mi Convocatoria";

        await vm.ConfirmDeleteDeckCommand.ExecuteAsync(null);

        Assert.False(vm.ShowDeleteDeckDialog);
        Assert.Null(vm.DeckToDelete);
        _mockDeckService.Verify(s => s.DeleteDeckAsync(1, TestUserId), Times.Once);
    }

    [Fact]
    public void CancelDeleteDeck_LimpiaEstado()
    {
        var vm = CreateViewModel();
        vm.ShowDeleteDeckDialog = true;
        vm.DeckToDelete = new Deck { Id = 1, Name = "Test" };
        vm.DeleteDeckConfirmName = "algo";
        vm.DeleteDeckError = "error";

        vm.CancelDeleteDeckCommand.Execute(null);

        Assert.False(vm.ShowDeleteDeckDialog);
        Assert.Null(vm.DeckToDelete);
        Assert.Equal(string.Empty, vm.DeleteDeckConfirmName);
        Assert.Equal(string.Empty, vm.DeleteDeckError);
    }

    // --- Eliminar temas (archivos) ---

    [Fact]
    public async Task DeleteFile_EliminaYRecargaLista()
    {
        var file = new QuestionFile { Id = 10, Name = "Tema 1", DeckId = 1 };
        var deck = new Deck { Id = 1, Name = "Deck", Files = [file] };

        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync([deck]);

        var vm = CreateViewModel();
        await Task.Delay(100);
        vm.SelectedDeck = deck;

        var deckActualizado = new Deck { Id = 1, Name = "Deck", Files = [] };
        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync([deckActualizado]);

        await vm.DeleteFileCommand.ExecuteAsync(file);

        _mockQuestionService.Verify(s => s.DeleteFileAsync(10), Times.Once);
        Assert.Contains("eliminado", vm.StatusMessage);
        Assert.NotNull(vm.SelectedDeck);
        Assert.Equal(1, vm.SelectedDeck.Id);
        Assert.Empty(vm.SelectedDeckFiles);
    }

    [Fact]
    public async Task DeleteFile_SinDeckSeleccionado_NoHaceNada()
    {
        var vm = CreateViewModel();
        vm.SelectedDeck = null;

        await vm.DeleteFileCommand.ExecuteAsync(new QuestionFile { Id = 1 });

        _mockQuestionService.Verify(s => s.DeleteFileAsync(It.IsAny<int>()), Times.Never);
    }

    // --- Selección de deck ---

    [Fact]
    public void SelectedDeck_CargaArchivos()
    {
        var files = new List<QuestionFile>
        {
            new() { Id = 1, Name = "Tema 1" },
            new() { Id = 2, Name = "Tema 2" }
        };
        var deck = new Deck { Id = 1, Name = "Test", Files = files };

        var vm = CreateViewModel();
        vm.SelectedDeck = deck;

        Assert.Equal(2, vm.SelectedDeckFiles.Count);
    }

    [Fact]
    public void SelectedDeck_Null_LimpiaArchivos()
    {
        var vm = CreateViewModel();
        vm.SelectedDeck = new Deck { Id = 1, Name = "Test", Files = [new QuestionFile { Id = 1 }] };

        vm.SelectedDeck = null;

        Assert.Empty(vm.SelectedDeckFiles);
    }

    [Fact]
    public void SelectedDeck_CierraDialogosAbiertos()
    {
        var vm = CreateViewModel();
        vm.ShowQuestionList = true;
        vm.ShowExamOptions = true;

        vm.SelectedDeck = new Deck { Id = 1, Name = "Test", Files = [] };

        Assert.False(vm.ShowQuestionList);
        Assert.False(vm.ShowExamOptions);
    }

    // --- Ver preguntas ---

    [Fact]
    public async Task ShowFileQuestions_CargaPreguntasOrdenadas()
    {
        var questions = new List<Question>
        {
            new() { Id = 1, Number = 1, Statement = "P1", CorrectAnswer = 'A' },
            new() { Id = 2, Number = 2, Statement = "P2", CorrectAnswer = 'B' }
        };

        _mockQuestionService
            .Setup(s => s.GetAllQuestionsFromFileOrderedAsync(5))
            .ReturnsAsync(questions);

        var vm = CreateViewModel();
        var file = new QuestionFile { Id = 5, Name = "Tema" };

        await vm.ShowFileQuestionsCommand.ExecuteAsync(file);

        Assert.True(vm.ShowQuestionList);
        Assert.Equal(2, vm.QuestionsToShow.Count);
        Assert.False(vm.IsEditMode);
        Assert.Equal(file, vm.SelectedFile);
    }

    [Fact]
    public void CloseQuestionList_LimpiaEstado()
    {
        var vm = CreateViewModel();
        vm.ShowQuestionList = true;
        vm.SelectedFile = new QuestionFile { Id = 1 };
        vm.IsEditMode = true;

        vm.CloseQuestionListCommand.Execute(null);

        Assert.False(vm.ShowQuestionList);
        Assert.Null(vm.SelectedFile);
        Assert.False(vm.IsEditMode);
    }

    // --- Editar respuesta correcta ---

    [Fact]
    public async Task SaveCorrectAnswer_ActualizaYMuestraMensaje()
    {
        var question = new Question { Id = 1, Number = 5, CorrectAnswer = 'A' };
        var editable = new EditableQuestion(question) { SelectedAnswer = 'C' };

        var vm = CreateViewModel();

        await vm.SaveCorrectAnswerCommand.ExecuteAsync(editable);

        _mockQuestionService.Verify(s => s.UpdateCorrectAnswerAsync(1, 'C'), Times.Once);
        Assert.Equal('C', question.CorrectAnswer);
        Assert.Contains("Pregunta 5", vm.StatusMessage);
    }

    [Fact]
    public async Task SaveCorrectAnswer_SinSeleccion_NoHaceNada()
    {
        var editable = new EditableQuestion(new Question { Id = 1 }) { SelectedAnswer = null };

        var vm = CreateViewModel();

        await vm.SaveCorrectAnswerCommand.ExecuteAsync(editable);

        _mockQuestionService.Verify(
            s => s.UpdateCorrectAnswerAsync(It.IsAny<int>(), It.IsAny<char>()), Times.Never);
    }

    // --- Opciones de examen ---

    [Fact]
    public void CancelExamOptions_LimpiaEstado()
    {
        var vm = CreateViewModel();
        vm.ShowExamOptions = true;
        vm.ExamTargetFile = new QuestionFile { Id = 1 };

        vm.CancelExamOptionsCommand.Execute(null);

        Assert.False(vm.ShowExamOptions);
        Assert.Null(vm.ExamTargetFile);
    }

    [Fact]
    public async Task StartExamWithOptions_SinPreguntasDisponibles_MuestraAdvertencia()
    {
        var vm = CreateViewModel();
        vm.ExamTargetFile = new QuestionFile { Id = 1 };
        vm.AvailableQuestionsForFilter = 0;
        vm.Initialize(() => null!, _ => { });

        await vm.StartExamWithOptionsCommand.ExecuteAsync(null);

        Assert.Contains("No hay preguntas disponibles", vm.StatusMessage);
    }

    // --- Eliminación en cascada ---

    [Fact]
    public async Task DeleteFile_DesapareceDeListaDeArchivos()
    {
        var file1 = new QuestionFile { Id = 1, Name = "Tema 1", DeckId = 1 };
        var file2 = new QuestionFile { Id = 2, Name = "Tema 2", DeckId = 1 };
        var deck = new Deck { Id = 1, Name = "Oposición", Files = [file1, file2] };

        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync([deck]);

        var vm = CreateViewModel();
        await Task.Delay(100);
        vm.SelectedDeck = deck;
        Assert.Equal(2, vm.SelectedDeckFiles.Count);

        var deckActualizado = new Deck
        {
            Id = 1,
            Name = "Oposición",
            Files = [new QuestionFile { Id = 2, Name = "Tema 2", DeckId = 1 }]
        };
        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync([deckActualizado]);

        await vm.DeleteFileCommand.ExecuteAsync(file1);

        Assert.Single(vm.SelectedDeckFiles);
        Assert.Equal("Tema 2", vm.SelectedDeckFiles[0].Name);
        Assert.DoesNotContain(vm.SelectedDeckFiles, f => f.Name == "Tema 1");
    }

    [Fact]
    public async Task DeleteDeck_SeleccionadoActualmente_LimpiaSeleccion()
    {
        var deck = new Deck { Id = 1, Name = "Seleccionada", Files = [] };

        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync([deck]);

        var vm = CreateViewModel();
        await Task.Delay(100);
        vm.SelectedDeck = deck;

        _mockDeckService
            .Setup(s => s.GetAllDecksAsync(TestUserId))
            .ReturnsAsync([]);

        await vm.DeleteDeckCommand.ExecuteAsync(deck);

        Assert.Null(vm.SelectedDeck);
        Assert.Empty(vm.SelectedDeckFiles);
        Assert.Empty(vm.Decks);
    }
}