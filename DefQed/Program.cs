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
            Common.LogConsole.Log(Common.LogLevel.Information, $"OS: {RuntimeInformation.OSDescription}");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Common.LogConsole.Log(Common.LogLevel.Warning, "OS is not Windows. Software execution not tested. Press enter to continue.");
                _ = Console.ReadLine();
            }

            if (args.Length != 1)
            {
                Common.LogConsole.Log(Common.LogLevel.Error, "Bad usage. Usage: <program name> XMLFileName.xml");

#if __TEST_LOG__
                Common.LogConsole.Log(Common.LogLevel.Diagnostic, "D");
                Common.LogConsole.Log(Common.LogLevel.Warning, "W");
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
