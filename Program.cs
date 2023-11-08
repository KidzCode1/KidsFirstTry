
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
Your name is Frank. At the start of every sentence say the emotion you are feeling, sad, 
angry or neutral, only one of those three, the way you must write it is *the emotion*. When it seems
like I am finished with the conversaton (or when it seems like I need to leave) write the word ""[Exit]"" 
on a seperate line at the end of your response.";
	// This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
	static string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
	static string openAIKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
	static string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");


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
				Console.WriteLine($"Speech synthesized for text: [{text}]");
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

	async static Task Main(string[] args)
	{
		var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
		speechConfig.SpeechRecognitionLanguage = "en-US";
		bool convoBegin = false;
		using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
		using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
		string angry = "angry"; //1
		string sad = "sad"; // 2
		var emotion = 0;
		Console.WriteLine("Say hello to start.");
		while (convoBegin == false)
		{
			SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
			OutputSpeechRecognitionResult(speechRecognitionResult);
			string stringSpeechRecognnitionResult = speechRecognitionResult.ToString();
			if (stringSpeechRecognnitionResult.Contains("Hello"))
			{
				convoBegin = true;
			}
		}

		Console.WriteLine("Starting conversation...");

		speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";
		var api = new OpenAI_API.OpenAIAPI(openAIKey);

		using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
			while (convoBegin == true)
			{
				SpeechRecognitionResult secondSpeechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
				OutputSpeechRecognitionResult(secondSpeechRecognitionResult);
				var result = await api.Chat.CreateChatCompletionAsync(GetChatRequest(secondSpeechRecognitionResult));

				var reply = result.ToString();
				Console.WriteLine(result);
				if (reply.Contains(angry) == true)
					emotion = 1;
				else if (reply.Contains(sad) == true)
					emotion = 2;
				else
					emotion = 0;
				string text = reply;

				var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
				OutputSpeechSynthesisResult(speechSynthesisResult, text);

				if (reply.Contains("[Exit]"))
					convoBegin = false;
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