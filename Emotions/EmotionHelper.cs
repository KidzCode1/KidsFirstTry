
using System;
public static class EmotionHelper
{
    // TODO: Add a method to convert a string emotion (like "angry" or "sad") into an Emotions enum element.
    // TODO: Replace the string "emotion" with an emotions enum.
    public static void GetEmotionFromResult(ref string textToSpeak, out string emotion)
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
}
