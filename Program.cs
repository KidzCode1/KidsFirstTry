
using System;
using Microsoft.CognitiveServices.Speech;

class Program
{
    static ServoMotorController? motorController;
    static string? lastErrorMessage;

    [STAThread]
    async static Task Main(string[] args)
    {
        Puppet puppet = new Puppet("Sally");

        motorController = new ServoMotorController();

        if (ApiKeys.AnyAreMissing)
        {
            Console.WriteLine($"This puppet requires three environment variables to be set on this machine to work. Contact Campbell for details.");
            Console.ReadLine();
            return;
        }

        InitializeMotorController();

        OpenAiHelper openAiHelper = new OpenAiHelper(puppet);
        SpeechHelper speechHelper = new SpeechHelper(puppet);


        //await speechHelper.WaitUntilWeHearHello();

        string initialGreeting = await openAiHelper.GetReplyAsync(Prompts.GetFirstGreeting).ConfigureAwait(false);
        string cleanGreeting = openAiHelper.ProcessEmotion(initialGreeting);

        await speechHelper.SpeakAsync(cleanGreeting);
        while (true)
        {
            string spokenWords = await speechHelper.GetSpokenWordsAsync();
            if (string.IsNullOrEmpty(spokenWords))
                continue;

            string reply = await openAiHelper.GetReplyAsync(spokenWords);

            MoveToAngle(90);
            SpeechSynthesisResult speechSynthesisResult = await speechHelper.SpeakAsync(reply);
            await Task.Delay((int)Math.Round(speechSynthesisResult.AudioDuration.TotalMilliseconds));
            MoveToAngle(0);
        }
    }

    static void MoveToAngle(int value)
    {
        if (motorController == null)
            return;
        motorController.MoveToAngle(value);
    }

    private static void InitializeMotorController()
    {
        if (motorController == null)
            return;
        motorController.BoardIsOffline += MotorController_BoardIsOffline;
        motorController.BoardIsOnline += MotorController_BoardIsOnline;
        motorController.CommunicationSuccess += MotorController_CommunicationSuccess;
        motorController.CommunicationError += MotorController_CommunicationError;
        motorController.ExceptionThrown += MotorController_ExceptionThrown;
    }

    private static void MotorController_ExceptionThrown(object? sender, Exception e)
    {
        if (e.Message == lastErrorMessage)
            return;

        lastErrorMessage = e.Message;
        ShowError($"Exception: {e.Message}");
    }

    private static void MotorController_CommunicationError(object? sender, string e)
    {
        ShowError(e);
    }

    private static void ShowError(string e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void MotorController_CommunicationSuccess(object? sender, string e)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(e);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void MotorController_BoardIsOnline(object? sender, EventArgs e)
    {
        Console.WriteLine($"Motor controller is online. Setting rotational speed to 1440 degrees per second.");
        motorController.SetSpeed(1440);
    }

    private static void MotorController_BoardIsOffline(object? sender, EventArgs e)
    {
        Console.WriteLine($"Motor controller is offline.");
    }
}