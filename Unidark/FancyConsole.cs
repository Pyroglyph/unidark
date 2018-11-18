using System;

namespace Unidark
{
    public static class FancyConsole
    {
        public static void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }

        public static void LogSuccess(string message)
        {
            Log(message, ConsoleColor.Green);
        }

        public static void LogError(string message)
        {
            Log(message, ConsoleColor.Red);
        }
    }
}
