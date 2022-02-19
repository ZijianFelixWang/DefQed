// If a warning appeared with CS0649 already disabled, just enable this
//#define __IDE_BUG_WARNING_FIX__
// For debug only
#define __DIAGNOSTIC_AS_DEFAULT__

using Terminal.Gui;
using DateTime = System.DateTime;

namespace DefQed
{
    internal static class LogConsole
    {
        // Usage: using Console = DefQed.LogConsole
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static TextView Display;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS0649 // Never assigned to value.
#if __DIAGNOSTIC_AS_DEFAULT__
        public static LogLevel LogLevel = DefQed.LogLevel.Diagnostic;
#else
        public static LogLevel LogLevel = DefQed.LogLevel.Information;
#endif
#pragma warning restore CS0649 // Never assigned to value.

// If a warning appeared with CS0649 already disabled, just enable this
#if __IDE_BUG_WARNING_FIX__
        public static void SetLogLevel(LogLevel lv) => LogLevel = lv;
#endif

        public static void Initialize(int X, int Y, int width, int height)
        {
            //SetLogLevel(LogLevel.Information);
            Display = new(new Rect(X, Y, width, height));
            Display.ReadOnly = true;
            Display.AllowsTab = false;
            Display.DesiredCursorVisibility = CursorVisibility.Box;
            Display.WordWrap = true;
            //Display.AllowsReturn = false;
            //Display.ColorNormal();

            if (Display.Text == null)
            {
                Display.Text = "";
            }
            Display.Text += "LogConsole initialized successfully.\n";
            //WriteLine("This is a writeline call.");
        }

        public static void Log(LogLevel level, string info)
        {
            if (LogLevel <= level)
            {
                WriteLine($"[{LogLevel2Str(level)}] [{DateTime.Now}] {info}");
            }

            if (level == LogLevel.Error)
            {
                _ = MessageBox.ErrorQuery("Error.", info, "Ok");
            }
        }

        private static string LogLevel2Str(LogLevel lev) => lev switch
        {
            LogLevel.Diagnostic => "DIAGNOSTIC",
            LogLevel.Information => "INFORMATION",
            LogLevel.Warning => "WARNING",
            LogLevel.Error => "ERROR",
            _ => ""
        };

        public static void WriteLine()
        {
            Display.Text += "\n";
            Display.MoveEnd();
        }

        public static void WriteLine(string str)
        {
            Display.Text += str;
            WriteLine();
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

        public static string ReadLine()
        {
            Display.ReadOnly = false;
            Display.AllowsReturn = true;
            Display.Multiline = false;
            int lines = Display.Lines;
            int startFrom = Display.Text.Length;

            while (Display.Lines <= lines + 1) { }

            Display.Multiline = true;
            Display.AllowsReturn = false;
            Display.ReadOnly = true;
            if (Display.Text.ToString() == null)
            {
                return "";
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            return Display.Text.ToString()[startFrom..].Trim();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public static void Beep() => System.Console.Beep();
    }

    internal enum LogLevel
    {
        Diagnostic = 0,
        Information = 1,
        Warning = 2,
        Error = 3
    };
}
