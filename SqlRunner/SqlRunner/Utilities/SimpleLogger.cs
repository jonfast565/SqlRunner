using System;

namespace SqlRunner.Utilities
{
    internal static class SimpleLogger
    {
        public static void LogThisReallyCrappyMessage(string type, string message)
        {
            Console.WriteLine("[" + type + "] " + message);
        }
    }
}