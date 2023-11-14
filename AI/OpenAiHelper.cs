
using System;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

public class OpenAiHelper
{
    OpenAIAPI api;
    public Emotions Emotion { get; set; }
    public OpenAiHelper()
    {
        api = new OpenAIAPI(ApiKeys.OpenAI);
    }

    public ChatRequest GetChatRequest(string prompt)
    {
        ChatRequest chatRequest = new ChatRequest()
        {

            Model = Model.ChatGPTTurbo,
            Temperature = 0.5,
            MaxTokens = 500,
            Messages = new ChatMessage[] {
            new ChatMessage(ChatMessageRole.User, Prompts.Initial),
            new ChatMessage(ChatMessageRole.User, prompt),
            }
        };
        return chatRequest;
    }

    Emotions GetEmotionFromString(string emotion)
    {
        switch (emotion)
        {
            case "sad": return Emotions.Sad;
            case "happy": return Emotions.Happy;
            case "angry": return Emotions.Angry;
            default: return Emotions.Neutral;
        }
    }

    public async Task<string> GetReplyAsync(OpenAiHelper openAiHelper, string prompt)
    {
        var result = await api.Chat.CreateChatCompletionAsync(openAiHelper.GetChatRequest(prompt));
        
        Console.WriteLine(result);

        string textToSpeak = result.ToString();

        EmotionHelper.GetEmotionFromResult(ref textToSpeak, out string emotion);

        Emotion = GetEmotionFromString(emotion);

        return textToSpeak;
    }
}
