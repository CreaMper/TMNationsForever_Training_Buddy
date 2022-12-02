using LogicStorage.Utils;
using System;

namespace Executor
{
    public static class Logger
    {
        public static void LogSeparator()
        {
            Console.WriteLine();
            Console.WriteLine();
        }

        public static void Log(string msg, LogTypeEnum type)
        {
            if (type.Equals(LogTypeEnum.Info))
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("[INFO] ");
            }
            else if (type.Equals(LogTypeEnum.Error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR] ");
            }
            else if (type.Equals(LogTypeEnum.CRITICAL))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("[CRITICAL] ");
            }
            else if (type.Equals(LogTypeEnum.Success))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[OK] ");
            }

            Console.ResetColor();
            Console.WriteLine(msg);
        }
    }
}
