
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
    readonly Puppet puppet;

    public event EventHandler<SpeechSynthesisWordBoundaryEventArgs>? WordBoundaryReached;
    public event EventHandler<SpeechSynthesisEventArgs>? SpeechSynthesisCompleted;
    public event EventHandler<SpeechSynthesisEventArgs>? SpeechSynthesisCanceled;

    public SpeechHelper(Puppet puppet)
    {
        this.puppet = puppet;
        speechConfig = SpeechConfig.FromSubscription(ApiKeys.Speech, ApiKeys.SpeechRegion);
        speechConfig.SpeechRecognitionLanguage = STR_RecognitionLanguageName;
        speechConfig.SpeechSynthesisVoiceName = STR_SynthesisVoiceName;
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
        speechSynthesizer = CreateSynthesizer();
        speechSynthesizer.WordBoundary += SpeechSynthesizer_WordBoundary;
        speechSynthesizer.SynthesisCompleted += SpeechSynthesizer_SynthesisCompleted;
        speechSynthesizer.SynthesisCanceled += SpeechSynthesizer_SynthesisCanceled;
    }

    protected virtual void OnWordBoundaryReached(object? sender, SpeechSynthesisWordBoundaryEventArgs e)
    {
        WordBoundaryReached?.Invoke(sender, e);
    }

    private void SpeechSynthesizer_WordBoundary(object? sender, SpeechSynthesisWordBoundaryEventArgs e)
    {
        OnWordBoundaryReached(sender, e);   
    }

    public bool ConversationActive { get; set; }

    public async Task WaitUntilWeHear(string phrase)
    {
        Console.WriteLine($"Say \"{phrase}\" to resume.");

        while (true)
        {
            SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
            ConsoleHelper.OutputSpeechRecognitionResult(speechRecognitionResult);
            string recognitionResult = speechRecognitionResult.ToString();
            if (recognitionResult.Contains(phrase, StringComparison.InvariantCultureIgnoreCase))
                return;
        }
    }

    SpeechSynthesizer CreateSynthesizer()
    {
        return new SpeechSynthesizer(speechConfig);
    }

    public async Task<string> GetSpokenWordsAsync()
    {
        SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
        if (speechRecognitionResult.Reason == ResultReason.NoMatch)
            return string.Empty;
        ConsoleHelper.OutputSpeechRecognitionResult(speechRecognitionResult);
        return speechRecognitionResult.Text;
    }

    public async Task<SpeechSynthesisResult> SpeakAsync(string reply)
    {
        ConsoleHelper.PuppetSays(puppet, reply);
        SpeechSynthesisResult speechSynthesisResult = await speechSynthesizer.StartSpeakingTextAsync(reply);
        ConsoleHelper.OutputSpeechSynthesisResult(speechSynthesisResult);
        return speechSynthesisResult;
    }

    void SpeechSynthesizer_SynthesisCompleted(object? sender, SpeechSynthesisEventArgs e)
    {
        SpeechSynthesisCompleted?.Invoke(sender, e);
    }

    void SpeechSynthesizer_SynthesisCanceled(object? sender, SpeechSynthesisEventArgs e)
    {
        SpeechSynthesisCanceled?.Invoke(sender, e);
    }
}
