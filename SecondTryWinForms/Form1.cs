using System.Windows.Forms;
using System.Runtime.Versioning;
using Microsoft.CognitiveServices.Speech;

namespace SecondTryWinForms
{
    [SupportedOSPlatform("windows")]
    public partial class FrmSpeechTry2 : Form
    {
        public FrmSpeechTry2()
        {
            InitializeComponent();
            // Main will go here.
            Puppet puppet = new Puppet("Sally");

            motorController = new ServoMotorController();

            if (ApiKeys.AnyAreMissing)
            {
                WriteLine($"This puppet requires three environment variables to be set on this machine to work. Contact Campbell for details.");
                return;
            }

            InitializeMotorController();

            OpenAiHelper openAiHelper = new OpenAiHelper(puppet);
            SpeechHelper speechHelper = new SpeechHelper(puppet);


            //await speechHelper.WaitUntilWeHearHello();

            _ = StartSpeaking(openAiHelper, speechHelper).ConfigureAwait(false);
        }

        private static async Task StartSpeaking(OpenAiHelper openAiHelper, SpeechHelper speechHelper)
        {
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

        static ServoMotorController? motorController;
        static string? lastErrorMessage;

        static void MoveToAngle(int value)
        {
            if (motorController == null)
                return;
            motorController.MoveToAngle(value);
        }

        private void InitializeMotorController()
        {
            if (motorController == null)
                return;
            motorController.BoardIsOffline += MotorController_BoardIsOffline;
            motorController.BoardIsOnline += MotorController_BoardIsOnline;
            motorController.CommunicationSuccess += MotorController_CommunicationSuccess;
            motorController.CommunicationError += MotorController_CommunicationError;
            motorController.ExceptionThrown += MotorController_ExceptionThrown;
        }

        private void MotorController_ExceptionThrown(object? sender, Exception e)
        {
            if (e.Message == lastErrorMessage)
                return;

            lastErrorMessage = e.Message;
            ShowError($"Exception: {e.Message}");
        }

        private void MotorController_CommunicationError(object? sender, string e)
        {
            ShowError(e);
        }

        private void ShowError(string e)
        {
            WriteLine($"Error: {e}");
        }

        private void MotorController_CommunicationSuccess(object? sender, string e)
        {
            WriteLine($"Success: {e}");
        }

        private void MotorController_BoardIsOnline(object? sender, EventArgs e)
        {
            WriteLine($"Motor controller is online. Setting rotational speed to 1440 degrees per second.");
            motorController?.SetSpeed(1440);
        }

        private void MotorController_BoardIsOffline(object? sender, EventArgs e)
        {
            WriteLine($"Motor controller is offline.");
        }

        void WriteLine(string str)
        {
            this.Invoke(() => { console.Text += str + Environment.NewLine; });
        }
    }
}
