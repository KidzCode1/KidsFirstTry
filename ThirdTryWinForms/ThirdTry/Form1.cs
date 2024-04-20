using System.Text;
using Microsoft.CognitiveServices.Speech;

namespace ThirdTry
{
    public partial class FrmThirdTry : Form
    {
        public FrmThirdTry()
        {
            InitializeComponent();
            HandleCreated += FrmThirdTry_HandleCreated;
            // Main will go here.
            Puppet puppet = new Puppet("Sally");

            motorController = new ServoMotorController();

            speechHelper = new SpeechHelper(puppet);
            if (ApiKeys.AnyAreMissing)
            {
                WriteLine($"This puppet requires three environment variables to be set on this machine to work. Contact Campbell for details.");
                return;
            }

            InitializeMotorController();

            OpenAiHelper openAiHelper = new OpenAiHelper(puppet);
            


            //await speechHelper.WaitUntilWeHearHello();

            speechHelper.WordBoundaryReached += SpeechHelper_WordBoundaryReached;
            speechHelper.SpeechSynthesisCompleted += SpeechHelper_SpeechSynthesisCompleted;
            speechHelper.SpeechSynthesisCanceled += SpeechHelper_SpeechSynthesisCanceled;

            closeMouthTimer.Elapsed += CloseMouthTimer_Elapsed;
            MoveToAngle(0);
            _ = StartSpeaking(openAiHelper).ConfigureAwait(false);
        }

        SpeechHelper speechHelper;
        System.Timers.Timer closeMouthTimer = new System.Timers.Timer();


        private void SpeechHelper_WordBoundaryReached(object? sender, SpeechSynthesisWordBoundaryEventArgs e)
        {
            if (e.BoundaryType != SpeechSynthesisBoundaryType.Word)
                return;

            closeMouthTimer.Start();
            closeMouthTimer.Interval = 0.75 * e.Duration.TotalMilliseconds;

            // TODO: Move the mouth in sync with the spoken words.


            //Console.WriteLine($"\r\nWordBoundary event:" +
            //    // Word, Punctuation, or Sentence
            //    $"\r\n\tBoundaryType: {e.BoundaryType}" +
            //    $"\r\n\tAudioOffset: {(e.AudioOffset + 5000) / 10000}ms" +
            //    $"\r\n\tDuration: {e.Duration}" +
            //    $"\r\n\tText: \"{e.Text}\"" +
            //    $"\r\n\tTextOffset: {e.TextOffset}" +
            //    $"\r\n\tWordLength: {e.WordLength}\r\n");

            MoveToAngle(45);
        }

        private void CloseMouthTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            closeMouthTimer.Stop();
            MoveToAngle(0);
        }



        private void FrmThirdTry_HandleCreated(object? sender, EventArgs e)
        {
            handleHasBeenCreated = true;
            if (savedMessagesThatWereSentBeforeTheHandleWasCreated.Length > 0)
            {
                WriteLine(savedMessagesThatWereSentBeforeTheHandleWasCreated.ToString());
                savedMessagesThatWereSentBeforeTheHandleWasCreated.Clear();
            }
        }

        void StartedListening()
        {
            listening = true;
            listeningFeedbackIndicator.Invalidate();
        }

        void DoneListening()
        {
            listening = false;
            listeningFeedbackIndicator.Invalidate();
        }

        private async Task StartSpeaking(OpenAiHelper openAiHelper)
        {
            string initialGreeting = await openAiHelper.GetReplyAsync(Prompts.GetFirstGreeting).ConfigureAwait(false);
            string cleanGreeting = openAiHelper.ProcessEmotion(initialGreeting);

            await Speak(cleanGreeting).ConfigureAwait(false);
            while (true)
            {
                StartedListening();
                string spokenWords = await speechHelper.GetSpokenWordsAsync();
                DoneListening();
                if (string.IsNullOrEmpty(spokenWords))
                    continue;

                WriteLine($"Heard: {spokenWords}");

                string reply = await openAiHelper.GetReplyAsync(spokenWords);
                WriteLine($"Response: {reply}");
                await Speak(reply).ConfigureAwait(false);
            }
        }

        private async Task Speak(string reply)
        {
            readyToListen = false;
            await speechHelper.SpeakAsync(reply).ConfigureAwait(false);

            while (!readyToListen)
            {
                await Task.Delay(25);
            }
        }

        static ServoMotorController? motorController;
        static string? lastErrorMessage;
        bool handleHasBeenCreated;
        StringBuilder savedMessagesThatWereSentBeforeTheHandleWasCreated = new StringBuilder();
        bool listening;
        bool readyToListen;

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
            //WriteLine($"Success: {e}");
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
            if (!handleHasBeenCreated)
            {
                savedMessagesThatWereSentBeforeTheHandleWasCreated.AppendLine(str);
                return;
            }
            this.Invoke(() => { console.Text += str + Environment.NewLine; });
        }

        private void listeningFeedbackIndicator_Paint(object sender, PaintEventArgs e)
        {
                Graphics graphics = e.Graphics;
            if (listening)
                graphics.FillEllipse(Brushes.Red, 0, 0, 25, 25);
            else
                graphics.Clear(BackColor);
        }

        void SpeechHelper_SpeechSynthesisCompleted(object? sender, SpeechSynthesisEventArgs e)
        {
            WriteLine("Ready to listen!");
            readyToListen = true;
        }

        void SpeechHelper_SpeechSynthesisCanceled(object? sender, SpeechSynthesisEventArgs e)
        {
            WriteLine("Ready to listen! (CANCELED!!!)");
            readyToListen = true;
        }
    }
}
