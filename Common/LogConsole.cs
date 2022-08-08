// If a warning appeared with CS0649 already disabled, just enable this
//#define __IDE_BUG_WARNING_FIX__
// For debug only
#define __DIAGNOSTIC_AS_DEFAULT__

using DateTime = System.DateTime;
using System.Diagnostics;

namespace Common
{
    /// <summary>
    /// The <c>LogConsole</c> class provides the program a way to display logs into files and screens.
    /// </summary>
    /// <remarks>
    /// Currently it only supports to show logs on the console. However, feature enabling logging to 
    /// other terminals will be added later.
    /// </remarks>
    internal static class LogConsole
    {
        // Usage: using Console = Common.LogConsole
#pragma warning disable CS0649 // Never assigned to value.
#if __DIAGNOSTIC_AS_DEFAULT__
        /// <summary>
        /// (field) This field stores logLevel, with default level Diagnostic.
        /// </summary>
        private static LogLevel logLevel = Common.LogLevel.Diagnostic;

#else
        /// <summary>
        /// (field) This field stores logLevel, with default level Diagnostic.
        /// </summary>
        private static LogLevel LogLevel = Common.LogLevel.Information;
#endif

        /// <summary>
        /// The <c>LogLevel</c> property is the enterance and interface to configure and check
        /// logLevel, controlling how many logs will the displayed.
        /// </summary>
        /// <remarks>
        /// If the build is executed under <c>DEBUG</c> profile, the default is <c>Diagnostic</c>. On the
        /// other hand, if the build is executed under <c>RELEASE</c> profile, <c>Information</c> is default.
        /// </remarks>
        /// <value>
        /// Controls how many logs are shown to the user.
        /// </value>
        public static LogLevel LogLevel { get => logLevel; set => logLevel = value; }
#pragma warning restore CS0649 // Never assigned to value.

        // If a warning appeared with CS0649 already disabled, just enable this
#if __IDE_BUG_WARNING_FIX__
        /// <summary>
        /// This function should not be enabled until the IDE has a strange bug.
        /// </summary>
        /// <param name="lv">Log level to set as.</param>
        public static void SetLogLevel(LogLevel lv) => LogLevel = lv;
#endif

        /// <summary>
        /// To display a line of log on the console to the user, in various levels.
        /// </summary>
        /// <remarks>
        /// If the information to log has level lower than the level configured before, the log will
        /// be ignored. Otherwise, it will be displayed.
        /// </remarks>
        /// <param name="level">The level to log.</param>
        /// <param name="info">The string to log.</param>
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

                System.Console.Write($"[{LogLevel2Str(level)}]");

                System.Console.ForegroundColor = fc;

                WriteLine($"[{DateTime.Now}] {info}");
            }

            Debug.WriteLine($"[{DateTime.Now}] {info}");
        }

        /// <summary>
        /// This is a utility to convert a loglevel to its corresponding string.
        /// </summary>
        /// <remarks>
        /// Based on a big switch structure, if the level is illegal, the returning output string is
        /// blank. Additionally, the output string is aligned at center.
        /// </remarks>
        /// <param name="lev">The log level to convert to string.</param>
        /// <returns>
        /// A string representing the log level.
        /// </returns>
        public static string LogLevel2Str(LogLevel lev) => lev switch
        {
            LogLevel.Diagnostic =>  "   DEBUG   ",
            LogLevel.Information => "INFORMATION",
            LogLevel.Warning =>     "  WARNING  ",
            LogLevel.Error =>       "   ERROR   ",
            _ => ""
        };

        /// <summary>
        /// Writes the current line terminator to the standard output stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method just simply calls <c>System.Console.WriteLine()</c> method to terminate the 
        /// current line.
        /// </para>
        /// <para>
        /// I know people will ask that if just <c>: System.Console</c> it would be better but based on
        /// my personal experiments, it is not practical.
        /// </para>
        /// </remarks>
        public static void WriteLine()
        {
            System.Console.WriteLine();
        }

        /// <summary>
        /// Writes the specified string value, followed by the current line terminator, to the standard 
        /// output stream.
        /// </summary>
        /// <param name="str">The string to write.</param>
        public static void WriteLine(string str)
        {
            System.Console.WriteLine(str);
        }

        /// <summary>
        /// Writes the text representation of the specified object, followed by the current line terminator,
        /// to the standard output stream.
        /// </summary>
        /// <remarks>
        /// This method first performs a null check that if the object is null, the method's effect is the
        /// same as the effect of <c>LogConsole.WriteLine()</c>.
        /// </remarks>
        /// <param name="obj">The value to write.</param>
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

        /// <summary>
        /// Reads a line from the standard input stream and returns the value.
        /// </summary>
        /// <returns>
        /// The value just got from the user's input.
        /// </returns>
        public static string ReadLine()
        {
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

    /// <summary>
    /// Defines an enumeration type describing the log level of the current log console.
    /// </summary>
    /// <remarks>
    /// The log level can be compared with and converted with integers. Defined between the brackets, there
    /// exists the following relationships.
    /// <list type="bullet">
    /// <item><c>Diagnostic</c> = 0</item>
    /// <item><c>Information</c> = 1</item>
    /// <item><c>Warning</c> = 2</item>
    /// <item><c>Error</c> = 3</item>
    /// </list>
    /// The lower the value is, the more log will be shown to the user.
    /// </remarks>
    public enum LogLevel
    {
        Diagnostic = 0,
        Information = 1,
        Warning = 2,
        Error = 3
    };
}
