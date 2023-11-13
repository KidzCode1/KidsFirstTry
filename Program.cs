
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI_API.Moderation;
class Program
{
	private const string initialPrompt = @"
Your name is Frank! At the start of every sentence say the emotion you are feeling, sad, 
angry or neutral, only one of those three. The emotion should precede your response and be placed 
in brackets, like [sad] or [angry]. Here is an example:

User: I lost my puppy today.
You: [sad] Oh no! That's terrible news.

When it seems like I am finished with the conversation (or when it seems like I need to leave) 
write the word ""[Exit]"" on a separate line at the end of your response.";
	// This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
	static string? speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
	static string? openAIKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
	static string? speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");


		static void OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
	{
		switch (speechRecognitionResult.Reason)
		{
			case ResultReason.RecognizedSpeech:
				Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
				break;
			case ResultReason.NoMatch:
				Console.WriteLine($"NOMATCH: Speech could not be recognized.");
				break;
			case ResultReason.Canceled:
				var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
				Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

				if (cancellation.Reason == CancellationReason.Error)
				{
					Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
					Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
					Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
				}
				break;
		}
	}


	static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
	{
		switch (speechSynthesisResult.Reason)
		{
			case ResultReason.SynthesizingAudioCompleted:
				Console.WriteLine($"Speech synthesized for textToSpeak: [{text}]");
				break;
			case ResultReason.Canceled:
				var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
				Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

				if (cancellation.Reason == CancellationReason.Error)
				{
					Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
					Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
					Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
				}
				break;
			default:
				break;
		}
	}

    static void GetEmotionFromResult(ref string textToSpeak, out string emotion)
    {
        emotion = "";
        textToSpeak = textToSpeak.Trim();
        if (textToSpeak.StartsWith("["))
        {
            int indexOfClosingBracket = textToSpeak.IndexOf(']');
            if (indexOfClosingBracket > 0 && indexOfClosingBracket < textToSpeak.Length - 1)
            {
                emotion = textToSpeak.Substring(1, indexOfClosingBracket - 1);
                textToSpeak = textToSpeak.Substring(indexOfClosingBracket + 1).Trim();
            }
        }
    }

    async static Task Main(string[] args)
	{
        if (speechKey == null || speechRegion == null || openAIKey == null)
        {
            Console.WriteLine($"This puppet requires three environment variables to be set on this machine to work. Contact Campbell for details.");
            Console.ReadLine();
            return;
        }
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
		speechConfig.SpeechRecognitionLanguage = "en-US";
		bool conversationActive = false;
		using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
		using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
		Console.WriteLine("Say hello to start.");
		while (conversationActive == false)
		{
			SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
			OutputSpeechRecognitionResult(speechRecognitionResult);
			string stringSpeechRecognitionResult = speechRecognitionResult.ToString();
			if (stringSpeechRecognitionResult.Contains("Hello"))
			{
				conversationActive = true;
			}
		}

		Console.WriteLine("Starting conversation...");

		speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";
		var api = new OpenAI_API.OpenAIAPI(openAIKey);

		using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
			while (conversationActive == true)
			{
				SpeechRecognitionResult secondSpeechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
				OutputSpeechRecognitionResult(secondSpeechRecognitionResult);
				var result = await api.Chat.CreateChatCompletionAsync(GetChatRequest(secondSpeechRecognitionResult));

				var reply = result.ToString();
				Console.WriteLine(result);
				
				string textToSpeak = reply;

                GetEmotionFromResult(ref textToSpeak, out string emotion);
                // TODO: Do something with the emotion.

                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(textToSpeak);
                OutputSpeechSynthesisResult(speechSynthesisResult, textToSpeak);

				if (reply.Contains("[Exit]"))
					conversationActive = false;
			}

	}

	private static ChatRequest GetChatRequest(SpeechRecognitionResult secondSpeechRecognitionResult)
	{
		ChatRequest chatRequest = new ChatRequest()
		{

			Model = Model.ChatGPTTurbo,
			Temperature = 0.5,
			MaxTokens = 500,
			Messages = new ChatMessage[] {
			new ChatMessage(ChatMessageRole.User, initialPrompt),
			new ChatMessage(ChatMessageRole.User, secondSpeechRecognitionResult.Text),
			}
		};
		return chatRequest;
	}
}