#if DEBUG
#define __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS__
#define __NO_WELCOME_SCREEN__
//#define __TEST_LOG__
//#define __SERIALIZE_DEBUG_JSON__
#endif

using System;
using System.Text;
using System.Runtime.InteropServices;
using McMaster.Extensions.CommandLineUtils;
using DefQed.Core;

namespace DefQed
{
    public class Program
    {
        [Option(ShortName = "l")]
        public Common.LogLevel? LogLevel { get; set; }

        [Option(Description = "File format: xml (default), js, qed", ShortName = "f")]
        public string? Format { get; set; }

        [Option(Description = "Time out, in seconds, default: 31536000 =365*24*3600", ShortName = "t")]
        public int? TimeOut { get; set; }

        [Option(Description = "Proof output distination. Program will be quiet then.", ShortName = "p")]
        public string? ProofFileName { get; set; }

        [Option(Description = "Not to tee proof text. This will override -p option. (Quiet mode)", ShortName = "q")]
        public bool Quiet { get; set; }

        [Argument(0, Description = "What to do with DefQed")]
        [System.ComponentModel.DataAnnotations.Required]
        public CommandType? Command { get; set; }

        [Argument(1, Description = "Optional. The parameter for command.")]
        public string? Parameter { get; set; }

        public static void Main(string[] args)
        {
            Console.WriteLine("DefQed. For detailed help, refer to github.");

            Console.Title = "DefQed Version 0.02\tStill in development\tby felix_wzj@yahoo.com";
            Console.ResetColor();

            Console.OutputEncoding = Encoding.Unicode;
            Common.LogConsole.Log(Common.LogLevel.Information, $"OS: {RuntimeInformation.OSDescription}");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Common.LogConsole.Log(Common.LogLevel.Warning, "OS is not Windows. Software execution not tested. Press enter to continue.");
            }

            CommandLineApplication.Execute<Program>(args);
        }

        private static readonly Job CurrentJob = new();

#pragma warning disable IDE0051 // Remove unused private members
        private void OnExecute()
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (LogLevel != null)
            {
                Common.LogConsole.LogLevel = (Common.LogLevel)LogLevel;
            }
            Common.LogConsole.Log(Common.LogLevel.Information, $"Set LogLevel as {Common.LogConsole.LogLevel2Str(Common.LogConsole.LogLevel)}.");
            
            if (TimeOut != null)
            {
                CurrentJob.TimeOut = (int)TimeOut;
            }
            Common.LogConsole.Log(Common.LogLevel.Information, $"Set TimeOut as {CurrentJob.TimeOut} seconds.");

            if (ProofFileName != null)
            {
                CurrentJob.ProofOutput = ProofFileName;
            }

            if (Quiet == true)
            {
                CurrentJob.NotToTee = true;
            }

            switch (Command)
            {
                case CommandType.Version:
                    Common.LogConsole.Log(Common.LogLevel.Information, "DefQed version 0.02 IN DEVELOPMENT.");
                    Environment.Exit(0);
                    break;
                case CommandType.Prove:
                    if (Parameter == null)
                    {
                        Common.LogConsole.Log(Common.LogLevel.Error, "Parameter is null.");
                        Environment.Exit(-1);
                    }


                    if (Format == null)
                    {
                        Format = "xml";
                    }

                    switch (Format.ToLower())
                    {
                        case "xml":
                        default:
                            ProveXML(Parameter);
                            break;
                        case "js":
                            ProveJavaScript(Parameter);
                            break;
                        case "qed":
                            ProveQEDPackage(Parameter);
                            break;
                    }
                    break;
                case CommandType.Reconfigure:
                    Common.LogConsole.Log(Common.LogLevel.Error, "Not implemented yet.");
                    Environment.Exit(-1);
                    break;
                default:
                    Common.LogConsole.Log(Common.LogLevel.Error, "Failed to parse command.");
                    Environment.Exit(-1);
                    break;
            }
        }

        private static void ProveJavaScript(string arg)
        {
            CurrentJob.LoadJS(arg);
            CurrentJob.PerformProof();
        }

        private static void ProveQEDPackage(string arg)
        {
            Common.LogConsole.Log(Common.LogLevel.Error, "Not impleented yet..");
            Environment.Exit(-1);
        }

        private static void ProveXML(string arg)
        {
#if __SERIALIZE_DEBUG_JSON__
            CurrentJob.SerializeDiagnosticBrackets();
            Console.ReadLine();
#endif

            CurrentJob.LoadXML(arg);
            CurrentJob.PerformProof();
        }
        public enum CommandType
        {
            Prove,
            Reconfigure,
            Version
        }
    }
}
