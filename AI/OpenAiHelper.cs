
using System;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

public class OpenAiHelper
{
    OpenAIAPI api;
    readonly Puppet puppet;
    public Emotions Emotion { get; set; }
    public OpenAiHelper(Puppet puppet)
    {
        this.puppet = puppet;
        api = new OpenAIAPI(ApiKeys.OpenAI);
    }

    string GetName()
    {
        return $"Your name is {puppet.Name}! ";
    }

    public ChatRequest GetChatRequest(string prompt)
    {
        ChatRequest chatRequest = new ChatRequest()
        {

            Model = Model.ChatGPTTurbo,
            Temperature = 0.5,
            MaxTokens = 500,
            Messages = new ChatMessage[] {
            new ChatMessage(ChatMessageRole.User, GetName() + Prompts.Initial),
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

    public async Task<string> GetReplyAsync(string prompt)
    {
        var result = await api.Chat.CreateChatCompletionAsync(GetChatRequest(prompt));

        string textToSpeak = result.ToString();
        textToSpeak = ProcessEmotion(textToSpeak);

        return textToSpeak;
    }

    public string ProcessEmotion(string textToSpeak)
    {
        EmotionHelper.GetEmotionFromResult(ref textToSpeak, out string emotion);

        Emotion = GetEmotionFromString(emotion);
        return textToSpeak;
    }
}
