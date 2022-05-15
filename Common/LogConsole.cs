// If a warning appeared with CS0649 already disabled, just enable this
//#define __IDE_BUG_WARNING_FIX__
// For debug only
#define __DIAGNOSTIC_AS_DEFAULT__

//using Terminal.Gui
using DateTime = System.DateTime;
using System.Diagnostics;

namespace Common
{
    internal static class LogConsole
    {
        // Usage: using Console = Common.LogConsole
//#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//        public static TextView Display;
//#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS0649 // Never assigned to value.
#if __DIAGNOSTIC_AS_DEFAULT__
        public static LogLevel LogLevel = Common.LogLevel.Diagnostic;
#else
        public static LogLevel LogLevel = Common.LogLevel.Information;
#endif
#pragma warning restore CS0649 // Never assigned to value.

// If a warning appeared with CS0649 already disabled, just enable this
#if __IDE_BUG_WARNING_FIX__
        public static void SetLogLevel(LogLevel lv) => LogLevel = lv;
#endif

        public static void Log(LogLevel level, string info)
        {
            if (LogLevel <= level)
            {
                var fc = System.Console.ForegroundColor;
                System.Console.ForegroundColor = level switch
                {
                    LogLevel.Diagnostic => System.ConsoleColor.Blue,
                    LogLevel.Information => System.ConsoleColor.White,
                    LogLevel.Warning => System.ConsoleColor.Yellow,
                    LogLevel.Error => System.ConsoleColor.Red,
                    _ => fc
                };

                //WriteLine($"[{LogLevel2Str(level)}] [{DateTime.Now}] {info}");
                System.Console.Write($"[{LogLevel2Str(level)}]");

                System.Console.ForegroundColor = fc;

                WriteLine($"[{DateTime.Now}] {info}");
            }

            Debug.WriteLine($"[{DateTime.Now}] {info}");
        }

        private static string LogLevel2Str(LogLevel lev) => lev switch
        {
            LogLevel.Diagnostic =>  "   DEBUG   ",
            LogLevel.Information => "INFORMATION",
            LogLevel.Warning =>     "  WARNING  ",
            LogLevel.Error =>       "   ERROR   ",
            _ => ""
        };

        public static void WriteLine()
        {
            System.Console.WriteLine();
        }

        public static void WriteLine(string str)
        {
            System.Console.WriteLine(str);
        }

        public static void WriteLine(object obj)
        {
            if (obj != null)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                WriteLine(obj.ToString());
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else
            {
                WriteLine();
            }
        }

        public static void Beep()
        {
            System.Console.Beep();
        }

        public static string ReadLine()
        {
//            Display.ReadOnly = false;
//            Display.AllowsReturn = true;
//            Display.Multiline = false;
//            int lines = Display.Lines;
//            int startFrom = Display.Text.Length;

//            while (Display.Lines <= lines + 1) { }

//            Display.Multiline = true;
//            Display.AllowsReturn = false;
//            Display.ReadOnly = true;
//            if (Display.Text.ToString() == null)
//            {
//                return "";
//            }

////#pragma warning disable CS8602 // Dereference of a possibly null reference.
//            return Display.Text.ToString()[startFrom..].Trim();
////#pragma warning restore CS8602 // Dereference of a possibly null reference.

            string? rl = System.Console.ReadLine();
            if (rl == null)
            {
                return "";
            }
            else
            {
                return rl;
            }
        }
    }

    internal enum LogLevel
    {
        Diagnostic = 0,
        Information = 1,
        Warning = 2,
        Error = 3
    };
}
