#if DEBUG
#define __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS__
#define __NO_WELCOME_SCREEN__
#define __TEST_LOG__
//#define __SERIALIZE_DEBUG_JSON__
#endif

using System;
using System.Text;
using System.Runtime.InteropServices;
//using Terminal.Gui
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
            //Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");
            LogConsole.Log(LogLevel.Information, $"OS: {RuntimeInformation.OSDescription}");

            //if (args.Length > 0)
            //{
            //    Console.WriteLine("Warning: Command line arguments not supported in this version. Press enter to continue.");
            //    _ = Console.ReadLine();
            //}

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
            //{
            //    X = 1,
            //    Y = 1,
            //    Width = Dim.Fill() - 2,
            //    Height = Dim.Fill() - 2
            //};

            //            MenuBar Menu = new(new MenuBarItem[]
            //            {
            //                new("_File", new MenuItem[]
            //                {
            //                    new("_Load XML", "Load XML documented statement to prove.", CurrentJob.LoadXMLUI, shortcut: Key.CtrlMask | Key.M),
            //                    new("_Renew Instance", "Dispose the current job and start a new one.", CurrentJob.RenewInstanceUI, shortcut: Key.CtrlMask | Key.R),
            //                    new("_Quit", "Exit the program.", CurrentJob.QuitUI, shortcut: Key.CtrlMask | Key.Q)
            //                }),
            //                new("_Proof", new MenuItem[]
            //                {
            //                    new("_Start", "Begin proving.", CurrentJob.PerformProof, shortcut: Key.CtrlMask | Key.S),
            //                    new("_Pause", "Pause current proof or resume a paused proof.", CurrentJob.PauseResumeUI,shortcut: Key.CtrlMask | Key.P),
            //                    new("_Dispose", "Dispose current prood.", CurrentJob.DisposeProofUI, shortcut: Key.CtrlMask | Key.D),
            //                }),
            //                new("_Configuration", new MenuItem[]
            //                {
            //                    new("Set _Timeout", "Configure the timeout deadline of the proof.", CurrentJob.SetTimeoutUI, shortcut: Key.CtrlMask | Key.T),
            //                    new("Set Lo_g Level", "Configure the log level.", CurrentJob.SetLogLevelUI, shortcut: Key.CtrlMask | Key.G),
            //                    new("_Insert Into KBase", "For diagnostic use only.", CurrentJob.KBaseInsertRowUI, shortcut: Key.CtrlMask | Key.K),
            //#if __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS__
            //                    new("Seriali_ze Diagnostic Brackets", "Should be removed after debug", CurrentJob.SerializeDiagnosticBrackets, shortcut: Key.CtrlMask | Key.Z)
            //#endif
            //                }),
            //                new("_Help", new MenuItem[]
            //                {
            //                    new("D_ocumentation", "This will be available somewhen later...", () => { _ = MessageBox.Query("Not implemented...", "No documentation available in this version."); }, shortcut: Key.CtrlMask | Key.O),
            //                    new("_About", "Version information", () => { _ = MessageBox.Query("DefQed", "Version 0.01\nCopyright Zijian Felix Wang. All rights reserved.\nEmail: felix_wzj@yahoo.com\n This should be licensed under MIT license."); }, shortcut: Key.CtrlMask | Key.V)
            //                })
            //            });

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