
using System;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
public class OpenAiHelper
{
    
    Queue<ChatEntry> chatEntries = new Queue<ChatEntry>();

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

    public ChatRequest GetChatRequest(string userPrompt)
    {
        ChatRequest chatRequest = new ChatRequest()
        {
            Model = Model.GPT4,
            Temperature = 0.8,
            MaxTokens = 500,
            Messages = BuildChatHistory(userPrompt)
        };
        return chatRequest;
    }

    public void AddPuppetResponseToChatHistory(string reply)
    {
        chatEntries.Enqueue(new ChatEntry(ChatMessageRole.Assistant, reply));
    }

    private ChatMessage[] BuildChatHistory(string userPrompt)
    {
        chatEntries.Enqueue(new ChatEntry(ChatMessageRole.User, userPrompt));
        
        while (chatEntries.Count > Constants.MaxChatEntries)
            chatEntries.Dequeue();

        ChatMessage[] result = new ChatMessage[chatEntries.Count + 1];

        result[0] = new ChatMessage(ChatMessageRole.User, GetName() + puppet.GetInitialPrompt());
        int index = 1;

        foreach (ChatEntry chatEntry in chatEntries)
        {
            result[index] = new ChatMessage(chatEntry.Role, chatEntry.Message);
            index++;
        }

        return result;
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

        AddPuppetResponseToChatHistory(textToSpeak);

        return textToSpeak;
    }

    public string ProcessEmotion(string textToSpeak)
    {
        EmotionHelper.GetEmotionFromResult(ref textToSpeak, out string emotion);

        Emotion = GetEmotionFromString(emotion);
        return textToSpeak;
    }
}
