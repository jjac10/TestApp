using System.Text.Json.Serialization;

namespace TestApp.Core.DTOs;

public class QuestionImportDto
{
    [JsonPropertyName("numero")]
    public int Number { get; set; }

    [JsonPropertyName("enunciado")]
    public string Statement { get; set; } = string.Empty;

    [JsonPropertyName("opcionA")]
    public string OptionA { get; set; } = string.Empty;

    [JsonPropertyName("opcionB")]
    public string OptionB { get; set; } = string.Empty;

    [JsonPropertyName("opcionC")]
    public string OptionC { get; set; } = string.Empty;

    [JsonPropertyName("opcionD")]
    public string OptionD { get; set; } = string.Empty;

    [JsonPropertyName("respuestaCorrecta")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [JsonPropertyName("fuente")]
    public string? Source { get; set; }
}

public class QuestionFileImportDto
{
    [JsonPropertyName("preguntas")]
    public List<QuestionImportDto> Questions { get; set; } = [];
}