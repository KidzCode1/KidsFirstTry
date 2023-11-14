
using System;
class Program
{
    async static Task Main(string[] args)
    {
        if (ApiKeys.AnyAreMissing)
        {
            Console.WriteLine($"This puppet requires three environment variables to be set on this machine to work. Contact Campbell for details.");
            Console.ReadLine();
            return;
        }

        OpenAiHelper openAiHelper = new OpenAiHelper();
        SpeechHelper speechHelper = new SpeechHelper();


        await speechHelper.WaitUntilWeHearHello();

        while (speechHelper.ConversationActive == true)
        {
            string spokenWords = await speechHelper.GetSpokenWordsAsync();
            string reply = await openAiHelper.GetReplyAsync(openAiHelper, spokenWords);

            // TODO: Do something with the openAiHelper.Emotion.

            await speechHelper.SpeakAsync(reply);

            if (reply.Contains("[Exit]"))
                speechHelper.ConversationActive = false;
        }

        ConsoleHelper.SayGoodbye();
    }
}