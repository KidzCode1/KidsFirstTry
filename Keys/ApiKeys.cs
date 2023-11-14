
using System;

/// <summary>
/// Retrieves and stores the API keys for Azure Cognitive Services and Open AI.
/// </summary>
public static class ApiKeys
{
    static string? speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
    static string? openAIKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
    static string? speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");

    public static bool AnyAreMissing => speechKey == null || speechRegion == null || openAIKey == null;
    public static string? Speech => speechKey;
    public static string? OpenAI => openAIKey;
    public static string? SpeechRegion => speechRegion;


}
