#if DEBUG
#define __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS__
#define __NO_WELCOME_SCREEN__
#define __TEST_LOG__
//#define __SERIALIZE_DEBUG_JSON__
#endif

using System;
using System.Text;
using System.Runtime.InteropServices;
using DefQed.Core;

namespace DefQed
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DefQed.");

            Console.Title = "DefQed Version 0.01\tStill in development\tby felix_wzj@yahoo.com";
            Console.ResetColor();

            Console.OutputEncoding = Encoding.Unicode;
            LogConsole.Log(LogLevel.Information, $"OS: {RuntimeInformation.OSDescription}");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                LogConsole.Log(LogLevel.Warning, "OS is not Windows. Software execution not tested. Press enter to continue.");
                _ = Console.ReadLine();
            }

            if (args.Length != 1)
            {
                LogConsole.Log(LogLevel.Error, "Bad usage. Usage: <program name> XMLFileName.xml");

#if __TEST_LOG__
                LogConsole.Log(LogLevel.Diagnostic, "D");
                LogConsole.Log(LogLevel.Warning, "W");
#endif

                Environment.Exit(-1);
            }

            MainApp(args[0]);
            Environment.Exit(0);
        }

        private static readonly Job CurrentJob = new();

        private static void MainApp(string arg)
        {
            Console.Title = "DefQed version 0.01";

#if __SERIALIZE_DEBUG_JSON__
            CurrentJob.SerializeDiagnosticBrackets();
            Console.ReadLine();
#endif

            // A better ui will be added later....
            CurrentJob.LoadXMLUI(arg);
            CurrentJob.PerformProof();
        }
    }
}
