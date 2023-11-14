
using System;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

/// <summary>
/// This is a helper class to assist with speech recognition as well as speech synthesis.
/// </summary>
public class SpeechHelper
{
    const string STR_RecognitionLanguageName = "en-US";
    const string STR_SynthesisVoiceName = "en-US-JennyNeural";

    SpeechConfig speechConfig;
    SpeechRecognizer speechRecognizer;
    SpeechSynthesizer speechSynthesizer;

    public SpeechHelper()
    {
        speechConfig = SpeechConfig.FromSubscription(ApiKeys.Speech, ApiKeys.SpeechRegion);
        speechConfig.SpeechRecognitionLanguage = STR_RecognitionLanguageName;
        speechConfig.SpeechSynthesisVoiceName = STR_SynthesisVoiceName;
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
        speechSynthesizer = CreateSynthesizer();
        speechSynthesizer.WordBoundary += (s, e) =>
        {
            Console.WriteLine($"\t\"{e.Text}\" ({e.Duration})");
            //Console.WriteLine($"\r\nWordBoundary event:" +
            //    // Word, Punctuation, or Sentence
            //    $"\r\n\tBoundaryType: {e.BoundaryType}" +
            //    $"\r\n\tAudioOffset: {(e.AudioOffset + 5000) / 10000}ms" +
            //    $"\r\n\tDuration: {e.Duration}" +
            //    $"\r\n\tText: \"{e.Text}\"" +
            //    $"\r\n\tTextOffset: {e.TextOffset}" +
            //    $"\r\n\tWordLength: {e.WordLength}\r\n");
        };
    }
    public bool ConversationActive { get; set; }

    public async Task WaitUntilWeHearHello()
    {
        ConsoleHelper.GiveStartingInstructions();

        while (!ConversationActive)
        {
            SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
            ConsoleHelper.OutputSpeechRecognitionResult(speechRecognitionResult);
            string recognitionResult = speechRecognitionResult.ToString();
            ConversationActive = recognitionResult.Contains("hello", StringComparison.InvariantCultureIgnoreCase);
        }

        ConsoleHelper.StartConversation();
    }

    SpeechSynthesizer CreateSynthesizer()
    {
        return new SpeechSynthesizer(speechConfig);
    }

    public async Task<string> GetSpokenWordsAsync()
    {
        SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
        ConsoleHelper.OutputSpeechRecognitionResult(speechRecognitionResult);
        return speechRecognitionResult.Text;
    }

    public async Task<SpeechSynthesisResult> SpeakAsync(string reply)
    {
        SpeechSynthesisResult speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(reply);
        ConsoleHelper.OutputSpeechSynthesisResult(speechSynthesisResult, reply);
        return speechSynthesisResult;
    }

}
