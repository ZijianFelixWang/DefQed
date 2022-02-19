using System;
using System.Text;
using System.Runtime.InteropServices;
using Terminal.Gui;
using DefQed.Core;

namespace DefQed
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DefQed.");

            Console.OutputEncoding = Encoding.Unicode;
            Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Warning: OS is not Windows. Software execution not tested. Press enter to continue.");
                _ = Console.ReadLine();
            }

            while (Execution != null)
            {
                Execution.Invoke();
            }
            Application.Shutdown();
            Environment.Exit(0);
        }

        private static Job CurrentJob = new();

        // Remark: The text user interface of the program is Terminal.Gui licensed under the MIT license.

#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static Action Execution = MainApp;
#pragma warning restore CA2211 // Non-constant fields should not be visible

        private static void MainApp()
        {
            Application.UseSystemConsole = true;
            Application.Init();
            Application.HeightAsBuffer = true;

            var top = Application.Top;

            var window = new Window("DefQed version 0.01")
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 2
            };

            MenuBar Menu = new(new MenuBarItem[]
            {
                new("_File", new MenuItem[]
                {
                    new("_Load XML", "Load XML documented statement to prove.", CurrentJob.LoadXMLUI, shortcut: Key.CtrlMask | Key.M),
                    new("_Renew Instance", "Dispose the current job and start a new one.", CurrentJob.RenewInstanceUI, shortcut: Key.CtrlMask | Key.R),
                    new("_Quit", "Exit the program.", CurrentJob.QuitUI, shortcut: Key.CtrlMask | Key.Q)
                }),
                new("_Proof", new MenuItem[]
                {
                    new("_Start", "Begin proving.", CurrentJob.PerformProof, shortcut: Key.CtrlMask | Key.S),
                    new("_Pause", "Pause current proof or resume a paused proof.", CurrentJob.PauseResumeUI,shortcut: Key.CtrlMask | Key.P),
                    new("_Dispose", "Dispose current prood.", CurrentJob.DisposeProofUI, shortcut: Key.CtrlMask | Key.D),
                }),
                new("_Configuration", new MenuItem[]
                {
                    new("Set _Timeout", "Configure the timeout deadline of the proof.", CurrentJob.SetTimeoutUI, shortcut: Key.CtrlMask | Key.T),
                    new("Set Lo_g Level", "Configure the log level.", CurrentJob.SetLogLevelUI, shortcut: Key.CtrlMask | Key.G)
                }),
                new("_Help", new MenuItem[]
                {
                    new("D_ocumentation", "This will be available somewhen later...", () => { _ = MessageBox.Query("Not implemented...", "No documentation available in this version."); }, shortcut: Key.CtrlMask | Key.O),
                    new("_About", "Version information", () => { _ = MessageBox.Query("DefQed", "Version 0.01\nCopyright Zijian Felix Wang. All rights reserved.\n This should be licensed under MIT license."); }, shortcut: Key.CtrlMask | Key.V)
                })
            });

            Label hint = new(3, 3, "This job");
            CurrentJob.PulseBar = new(new Rect(3, 5, 40, 3));

            Label hint2 = new()
            {
                X = Pos.Right(CurrentJob.PulseBar) + 2,
                Y = Pos.Top(hint),
                Text = "Log console"
            };

            // Well, Terminal.Gui.FakeConsole is not implemented by its author.
            // So I need to implement my own LogConsole.
            //LogConsole.Initialize(Pos.Left(hint2), Pos.Bottom(hint2), 80, 80);
            LogConsole.Initialize(45, 4, 80, 60);

            window.Add(hint);
            window.Add(hint2);
            window.Add(CurrentJob.PulseBar);
            window.Add(LogConsole.Display);
            top.Add(window);
            top.Add(Menu);
            Application.Run(top);
        }
    }
}