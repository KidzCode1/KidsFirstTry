
using System;
class Program
{
    async static Task Main(string[] args)
    {
        Puppet puppet = new Puppet("Sally");

        if (ApiKeys.AnyAreMissing)
        {
            Console.WriteLine($"This puppet requires three environment variables to be set on this machine to work. Contact Campbell for details.");
            Console.ReadLine();
            return;
        }

        OpenAiHelper openAiHelper = new OpenAiHelper(puppet);
        SpeechHelper speechHelper = new SpeechHelper(puppet);


        //await speechHelper.WaitUntilWeHearHello();

        string initialGreeting = await openAiHelper.GetReplyAsync(Prompts.GetFirstGreeting);
        string cleanGreeting = openAiHelper.ProcessEmotion(initialGreeting);
        await speechHelper.SpeakAsync(cleanGreeting);
        while (true)
        {
            string spokenWords = await speechHelper.GetSpokenWordsAsync();
            string reply = await openAiHelper.GetReplyAsync(spokenWords);

            await speechHelper.SpeakAsync(reply);
        }
    }
}