using System;

namespace MotorControllerTest
{
    internal class Program
    {
        static ServoMotorController motorController = new ServoMotorController();
        static string? lastErrorMessage;
        static async Task Main(string[] args)
        {
            motorController.BoardIsOffline += MotorController_BoardIsOffline;
            motorController.BoardIsOnline += MotorController_BoardIsOnline;
            motorController.CommunicationSuccess += MotorController_CommunicationSuccess;
            motorController.CommunicationError += MotorController_CommunicationError;
            motorController.ExceptionThrown += MotorController_ExceptionThrown;
            Console.WriteLine("Hello, World!");
            while (true)
            {
                const string speedPrefix = "speed ";
                string? readLine = Console.ReadLine();
                if (readLine?.StartsWith(speedPrefix, StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    string speedStr = readLine!.Substring(speedPrefix.Length);
                    if (short.TryParse(speedStr, out short speedValue))
                    {
                        motorController.SetSpeed(speedValue);
                    }
                }
                else if (short.TryParse(readLine, out short angle))
                {
                    motorController.MoveToAngle(angle);
                }
                if (readLine == "exit")
                    break;
            }
            motorController.Close();
            //await Task.Delay(TimeSpan.FromHours(3));
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
}
