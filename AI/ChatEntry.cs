
using System;
using OpenAI_API.Chat;

public class ChatEntry
{
    public ChatMessageRole Role { get; set; }
    public string Message { get; set; }
    public ChatEntry(ChatMessageRole role, string message)
    {
        Role = role;
        Message = message;
    }

    public override string ToString()
    {
        return $"{Role}: {Message}";
    }
}
