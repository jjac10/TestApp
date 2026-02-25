using System.Text;
using System.Text.RegularExpressions;
using TestApp.Core.DTOs;
using UglyToad.PdfPig;

namespace TestApp.Core.Services;

public partial class PdfImportService : IPdfImportService
{
    public Task<List<QuestionImportDto>> ExtractQuestionsFromPdfAsync(string pdfPath, int numberOfQuestions)
    {
        var text = ExtractTextFromPdf(pdfPath);
        var questions = ParseQuestions(text, numberOfQuestions);
        return Task.FromResult(questions);
    }

    public Task<int> CountQuestionsInPdfAsync(string pdfPath)
    {
        var text = ExtractTextFromPdf(pdfPath);
        var count = CountQuestions(text);
        return Task.FromResult(count);
    }

    private static int CountQuestions(string text)
    {
        var count = 0;
        var questionNumber = 1;

        while (true)
        {
            var numberPattern = $"{questionNumber}.";
            var numberIndex = text.IndexOf(numberPattern, StringComparison.Ordinal);
            
            if (numberIndex == -1) break;

            // Verificar que tiene estructura de pregunta (al menos A) y B))
            var optionAIndex = text.IndexOf("A)", numberIndex, StringComparison.Ordinal);
            var nextQuestionPattern = $"{questionNumber + 1}.";
            var nextQuestionIndex = text.IndexOf(nextQuestionPattern, numberIndex + numberPattern.Length, StringComparison.Ordinal);
            
            // Verificar que A) está antes de la siguiente pregunta
            if (optionAIndex != -1 && (nextQuestionIndex == -1 || optionAIndex < nextQuestionIndex))
            {
                count++;
            }
            
            questionNumber++;
        }

        return count;
    }

    private static string ExtractTextFromPdf(string pdfPath)
    {
        var sb = new StringBuilder();

        using var document = PdfDocument.Open(pdfPath);
        foreach (var page in document.GetPages())
        {
            sb.Append(page.Text);
        }

        return sb.ToString();
    }

    private static List<QuestionImportDto> ParseQuestions(string text, int numberOfQuestions)
    {
        var questions = new List<QuestionImportDto>();
        var currentPosition = 0;

        for (int questionNumber = 1; questionNumber <= numberOfQuestions; questionNumber++)
        {
            var question = ParseSingleQuestion(text, ref currentPosition, questionNumber);
            if (question != null)
            {
                questions.Add(question);
            }
        }

        return questions;
    }

    private static QuestionImportDto? ParseSingleQuestion(string text, ref int position, int expectedNumber)
    {
        // Buscar el inicio de la pregunta: "N."
        var numberPattern = $"{expectedNumber}.";
        var numberIndex = text.IndexOf(numberPattern, position, StringComparison.Ordinal);
        if (numberIndex == -1) return null;

        // Mover posición después del número
        position = numberIndex + numberPattern.Length;

        // Buscar A) para obtener el Statement
        var optionAIndex = text.IndexOf("A)", position, StringComparison.Ordinal);
        if (optionAIndex == -1) return null;

        var statement = CleanText(text[position..optionAIndex]);
        position = optionAIndex + 2;

        // Buscar B) para obtener Option A
        var optionBIndex = text.IndexOf("B)", position, StringComparison.Ordinal);
        if (optionBIndex == -1) return null;

        var optionA = CleanText(text[position..optionBIndex]);
        position = optionBIndex + 2;

        // Buscar C) para obtener Option B
        var optionCIndex = text.IndexOf("C)", position, StringComparison.Ordinal);
        if (optionCIndex == -1) return null;

        var optionB = CleanText(text[position..optionCIndex]);
        position = optionCIndex + 2;

        // Buscar D) para obtener Option C
        var optionDIndex = text.IndexOf("D)", position, StringComparison.Ordinal);
        if (optionDIndex == -1) return null;

        var optionC = CleanText(text[position..optionDIndex]);
        position = optionDIndex + 2;

        // Buscar "Respuesta correcta:" para obtener Option D
        var respuestaIndex = text.IndexOf("Respuesta correcta:", position, StringComparison.OrdinalIgnoreCase);
        if (respuestaIndex == -1) return null;

        var optionD = CleanText(text[position..respuestaIndex]);
        position = respuestaIndex + "Respuesta correcta:".Length;

        // Obtener la letra de respuesta correcta (siguiente carácter no-espacio)
        while (position < text.Length && char.IsWhiteSpace(text[position]))
            position++;

        if (position >= text.Length) return null;

        var correctAnswer = text[position].ToString().ToUpper();
        position++;

        // Buscar "Fuente:" (opcional) - termina en el siguiente número de pregunta o fin de texto
        string? source = null;
        var fuenteIndex = text.IndexOf("Fuente:", position, StringComparison.OrdinalIgnoreCase);
        var nextQuestionPattern = $"{expectedNumber + 1}.";
        var nextQuestionIndex = text.IndexOf(nextQuestionPattern, position, StringComparison.Ordinal);

        // Si encontramos "Fuente:" y está antes de la siguiente pregunta (o no hay siguiente pregunta)
        if (fuenteIndex != -1 && (nextQuestionIndex == -1 || fuenteIndex < nextQuestionIndex))
        {
            var fuenteStart = fuenteIndex + "Fuente:".Length;
            var fuenteEnd = nextQuestionIndex != -1 ? nextQuestionIndex : text.Length;
            source = CleanText(text[fuenteStart..fuenteEnd]);
            
            if (string.IsNullOrWhiteSpace(source))
                source = null;
        }

        return new QuestionImportDto
        {
            Number = expectedNumber,
            Statement = statement,
            OptionA = optionA,
            OptionB = optionB,
            OptionC = optionC,
            OptionD = optionD,
            CorrectAnswer = correctAnswer,
            Source = source
        };
    }

    private static string CleanText(string text)
    {
        // Remove extra whitespace and trim
        return Regex.Replace(text.Trim(), @"\s+", " ");
    }
}