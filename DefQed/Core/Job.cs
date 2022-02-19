// For debugging the module.
#define __USE_INLINE_XML_URI__

using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using DefQed.Data;
using Terminal.Gui;
using NStack;
using Console = DefQed.LogConsole;
using Environment = System.Environment;
using TimeSpan = System.TimeSpan;
using Exception = System.Exception;
using System.Collections.Generic;

namespace DefQed.Core
{
    internal class Job
    {
        // Description: Job is the class that stores everything related to a DefQed utilization.
        public string XMLFileName = "";
        public KBase KnowledgeBase = new();
        public int TimeOut = 365 * 24 * 3600;   // A year's proof hah

        private Task? ProofTask;
        private bool ProofPalsed = false;

        // Remark: if give a default value will lead into an excpetion...
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ProgressBar PulseBar;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private bool PulseNow = false;

        //public void Initialize()
        //{
        //    // So what does it do when the user executes the initialization command now?
        //    if (XMLFileName == "")
        //    {
        //        Console.WriteLine("Error: no xml specified.");
        //        Environment.Exit(-1);
        //    }

        //    // Parse & Connect
        //    XMLParser.ParseXML(XMLFileName, ref KnowledgeBase);
        //    // Initialization complete.
        //}

        public void PerformProof()
        {
            //Console.Log(LogLevel.Information, "To call PerformProof");
            PerformProof(TimeOut);
        }

        private void PerformProof(int TimeOut)
        {
            PulseBar.Pulse();
            try
            {
                _ = MessageBox.Query("PerformProof called.", "Start proving...", "Ok");
                ProofTask = new(() =>
                {
                    if (!ProofPalsed)
                    {
                        while (!KnowledgeBase.TryBridging())
                        {
                            if (PulseNow = !PulseNow)
                            {
                                PulseBar.Pulse();
                            }
                            KnowledgeBase.ScanPools();
                        }
                    }
                });

                //_ = MessageBox.Query("PerformProof called.", "DMessage", "Ok");

                Console.Log(LogLevel.Information, "PerformProof called: Start proving");

                ProofTask.Start();

                if (ProofTask.Wait(new TimeSpan(0, 0, 0, 0, TimeOut)))
                {
                    //Console.WriteLine("Proof process has finished!");
                    Console.Log(LogLevel.Information, "Proof process has finished.");
                    _ = MessageBox.Query("Proof done.", "Proof execution done successfully.");
                    #region commented stuff decomment after debug
                    //Console.WriteLine("Below is report generated:");
                    //Console.WriteLine(KnowledgeBase.GenerateReport());
                    //Console.WriteLine("\nGenerate report file?\nN/ENTER = No; S = Serialized KBase; T = Proof text; B = Both");
                    //while (true)
                    //{
                    //    string? sel = Console.ReadLine();
                    //    if ((sel == null) || (sel.Trim().ToUpper() == "N"))
                    //    {
                    //        break;
                    //    }

                    //    sel = sel.Trim().ToUpper();
                    //    if (sel == "S")
                    //    {
                    //        TeeSerializedKBase();
                    //        break;
                    //    }
                    //    if (sel == "T")
                    //    {
                    //        TeeProofText();
                    //        break;
                    //    }
                    //    if (sel == "B")
                    //    {
                    //        TeeSerializedKBase();
                    //        TeeProofText();
                    //        break;
                    //    }
                    //    Console.WriteLine("\nGenerate report file?\nN/ENTER = No; S = Serialized KBase; T = Proof text; B = Both");
                    #endregion
                }
                else
                {
                    // timeout termination encountered.
                    Console.WriteLine("Proof terminated because of timeout. Dump current KBase? Y/N(Default)");
                    while (true)
                    {
                        string? sel = Console.ReadLine();
                        if ((sel == null) || (sel.Trim().ToUpper() == "N"))
                        {
                            break;
                        }

                        sel = sel.Trim().ToUpper();
                        if (sel == "Y")
                        {
                            TeeSerializedKBase();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // This error handling is very lazy here. No categoring of errors umm.
                // Something more may be added later.
                Console.WriteLine($"Error: Exception envountered. Exception Category: {ex.GetType()}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine("You can now save the dump file. Use CTRL+C to terminate the program.");
                TeeSerializedKBase();
            }
        }

#pragma warning disable CA1822 // Mark members as static
        public void SetLogLevelUI()
#pragma warning restore CA1822 // Mark members as static
        {
            Button confirm = new("Ok", is_default: true);
            Button cancel = new("Cancel", is_default: false);
            RadioGroup sel = new(2, 2, new ustring[] { "Diagnostic", "Information", "Warning", "Error" }, 1);
            confirm.Clicked += () =>
            {
                Console.LogLevel = sel.SelectedItem switch
                {
                    0 => LogLevel.Diagnostic,
                    1 => LogLevel.Information,
                    2 => LogLevel.Warning,
                    3 => LogLevel.Error,
                    _ => LogLevel.Information
                };

                Console.Log(LogLevel.Diagnostic, "Diagnostic example.");
                Console.Log(LogLevel.Information, "Information example.");
                Console.Log(LogLevel.Warning, "Warning example.");
                Console.Log(LogLevel.Error, "Error example.");


                Application.RequestStop();
            };
            cancel.Clicked += () =>
            {
                Application.RequestStop();
            };

            Dialog ui = new("Request input.", 50, 20, confirm, cancel);
            Label hint = new(1, 1, "Select the log level. The default option is information. Click Ok to confirm, Click cancel to go back.");

            ui.Add(hint);
            ui.Add(sel);
            Application.Run(ui);
            // Now the loop is finished.
        }

        public void KBaseInsertRowUI()
        {
            Button confirm = new("Ok", is_default: true);
            Button cancel = new("Cancel", is_default: false);
            RadioGroup sel = new(2, 2, new ustring[] { "Notations", "Reflections", "Registries" }, 0);

            cancel.Clicked += () =>
            {
                Application.RequestStop();
            };
            confirm.Clicked += () =>
            {
                switch(sel.SelectedItem)
                {
                    case 0:
                        // Notations
                        string title = "";
                        string origin = "";
                        string opacity = "";
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        title = TextInputBoxUI("Input for \"title\" column.").ToString();
                        origin = TextInputBoxUI("Input for \"origin\" column.").ToString();
                        opacity = TextInputBoxUI("Input for \"opacity\" column.").ToString(); ;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                        // Get next id
                        int id = MySQLDriver.GetMaxId(TableType.Notations) + 1;

#pragma warning disable CS8601 // Possible null reference assignment.
                        MySQLDriver.InsertRow(TableType.Notations, new List<string>(new string[] { "ID", "TITLE", "ORIGIN", "OPACITY" }), new List<string>(new string[] { id.ToString(), title, origin, opacity }));
#pragma warning restore CS8601 // Possible null reference assignment.
                        break;

                    case 1:
                        // Reflections
                        string cases = "";
                        string thuses = "";
                        string opacity1 = "";
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        cases = TextInputBoxUI("Input for \"cases\" column.").ToString();
                        thuses = TextInputBoxUI("Input for \"thuses\" column.").ToString();
                        opacity1 = TextInputBoxUI("Input for \"opacity\" column.").ToString(); ;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                        // Get next id
                        int id1 = MySQLDriver.GetMaxId(TableType.Reflections) + 1;

#pragma warning disable CS8601 // Possible null reference assignment.
                        MySQLDriver.InsertRow(TableType.Reflections, new List<string>(new string[] { "ID", "CASES", "THUSES", "OPACITY" }), new List<string>(new string[] { id1.ToString(), cases, thuses, opacity1 }));
#pragma warning restore CS8601 // Possible null reference assignment.
                        break;

                    case 2:
                        // Registries
                        string content = "";
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        content = TextInputBoxUI("Input for \"content\" column.").ToString();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                        // Get next id
                        int id2 = MySQLDriver.GetMaxId(TableType.Registries) + 1;

#pragma warning disable CS8601 // Possible null reference assignment.
                        MySQLDriver.InsertRow(TableType.Registries, new List<string>(new string[] { "ID", "CONTENT" }), new List<string>(new string[] { id2.ToString(), content }));
#pragma warning restore CS8601 // Possible null reference assignment.
                        break;

                    default:
                        Console.Log(LogLevel.Error, "Internal Error: Unexpected selection.");
                        break;
                }

                Console.Log(LogLevel.Warning, "You need to restart the instance to load changes.");

                Application.RequestStop();
            };


            Dialog ui = new("Quick KBase modifying: Diagnostic use only.", 50, 20, confirm, cancel);
            Label hint = new(1, 1, "Do not confirm if you do not know how the KBase works. For diagnostic purpose only.");

            ui.Add(hint);
            ui.Add(sel);

            Application.Run(ui);
            // Now the loop is finished.
        }

        public void SerializeDiagnosticBrackets()
        {

        }

        private void TextInputBoxInnerUI(ustring hintText)
        {

            Button confirm = new("Ok", is_default: true);
            Button cancel = new("Cancel", is_default: false);
            TextField field = new()
            {
                X = 2,
                Y = 2,
                Width = 40
            };

            confirm.Clicked += () =>
            {
                JustNowTextInput = field.Text;
                Application.RequestStop();
            };
            cancel.Clicked += () =>
            {
                Application.RequestStop();
            };
            Dialog ui = new("Request input.", 50, 20, confirm, cancel);
            Label hint = new(1, 1, hintText);

            ui.Add(hint);
            ui.Add(field);
            Application.Run(ui);
            // Now the loop is finished.
        }

        private ustring TextInputBoxUI(ustring hint)
        {
            TextInputBoxInnerUI(hint);
            return JustNowTextInput;
        }

        // What a ugly line of code here.
        private ustring JustNowTextInput = "";

        public void SetTimeoutUI()
        {
            Button confirm = new("Ok", is_default: true);
            Button cancel = new("Cancel", is_default: false);
            TextField field = new()
            {
                X = 2,
                Y = 2,
                Width = 40
            };
            confirm.Clicked += () =>
            {
                if (field.Text.ToString() != null)
                {
                    if (!int.TryParse(field.Text.ToString(), out TimeOut))
                    {
                        // parse error.
                        _ = MessageBox.ErrorQuery("Bad data format.", "The timeout value is not a valid int.", "Ok");
                    }
                }
                Application.RequestStop();
            };
            cancel.Clicked += () =>
            {
                Application.RequestStop();
            };
            Dialog ui = new("Request input.", 50, 20, confirm, cancel);
            Label hint = new(1, 1, "Input the DefQed XML file's full filename here. Click Ok to confirm, Click cancel to go back.");

            ui.Add(hint);
            ui.Add(field);
            Application.Run(ui);
            // Now the loop is finished.
        }

        public void LoadXMLUI()
        {
            Button confirm = new("Ok", is_default: true);
            Button cancel = new("Cancel", is_default: false);
            TextField field = new()
            {
                X = 2,
                Y = 2,
                Width = 40,
#if __USE_INLINE_XML_URI__
                Text = @"C:\Users\felix\Documents\projects\DefQed\DefQed\example.xml"
#endif
        };
            Dialog ui = new("Request input.", 50, 8, confirm, cancel);
            confirm.Clicked += () =>
            {
                if (field.Text.ToString() != null)
                {
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8604 // Possible null reference argument.
                    XMLFileName = field.Text.ToString();
                    Application.RequestStop();
                    bool err = false;
                    KBase temp = new();

                    XMLParser.ParseXML(XMLFileName, ref temp, ref err);

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8601 // Possible null reference assignment.

                    if (err)
                    {
                        _ = MessageBox.ErrorQuery("Error.", "XML parse failure.\nFor more details, see the log console.", new ustring[] { "Ok" });
                    }
                    else
                    {
                        KnowledgeBase = temp;
                        _ = MessageBox.Query("Congratulations.", "XMML parse and DB connect success.", "Ok");
                    }
                }
                else
                {
                    Application.RequestStop();
                }
            };
            cancel.Clicked += () =>
            {
                Application.RequestStop();
            };
            Label hint = new(1, 1, "Input the DefQed XML file's full filename here. Click Ok to confirm, Click cancel to go back.");

            ui.Add(hint);
            ui.Add(field);
            Application.Run(ui);
            // Now the loop is finished.
        }

        public void RenewInstanceUI()
        {
            int choice = MessageBox.Query("Are you sure?", "This operation will dispose the current job and cannot be restored.", defaultButton: 1, buttons: new ustring[] { "Yes", "No" });
            if (choice == 0)
            {
                if ((ProofTask != null) && (!ProofTask.IsCompleted))
                {
                    ProofTask.Dispose();
                }
                if ((MySQLDriver.connStr != "") && (MySQLDriver.connStr != null))
                {
                    MySQLDriver.Terminate();
                }
                XMLFileName = "";
                KnowledgeBase = new();
            }
        }

        public void DisposeProofUI()
        {
            int choice = MessageBox.Query("Are you sure?", "This operation will dispose the current proof and cannot be restored.", defaultButton: 1, buttons: new ustring[] { "Yes", "No" });
            if (choice == 0)
            {
                if ((ProofTask != null) && (!ProofTask.IsCompleted))
                {
                    ProofTask.Dispose();
                }
                else
                {
                    MessageBox.Query("Invalid operation.", "There is nothing to dispose.", "Ok");
                }
            }
        }

        public void QuitUI()
        {
            int choice = MessageBox.Query("Are you sure?", "This will terminate the application.", defaultButton: 1, buttons: new ustring[] { "Yes", "No" });
            if (choice == 0)
            {
                if ((ProofTask != null) && (!ProofTask.IsCompleted))
                {
                    ProofTask.Dispose();
                }
                if ((MySQLDriver.connStr != "") && (MySQLDriver.connStr != null))
                {
                    MySQLDriver.Terminate();
                }
                Application.Shutdown();
                Environment.Exit(0);
            }
        }

        public void PauseResumeUI()
        {
            if ((ProofTask == null) || (ProofTask.IsCompleted))
            {
                _ = MessageBox.Query("Invalid operation.", "There is nothing to pause or resume.", "Ok");
            }

            ProofPalsed = !ProofPalsed;
        }

        private void TeeSerializedKBase()
        {
            string serialization = JsonSerializer.Serialize(KnowledgeBase);
            string? filename;
            while (true)
            {
                Console.WriteLine("Asking: (Json Serialized KBase) File name.");
                filename = TextInputBoxUI("Where to save the serialized KBase?").ToString();
                if ((filename != null) && (filename.Length > 0))
                {
                    break;
                }

                int sel = MessageBox.Query("Cancel or not", "Do you want to cancel?", new ustring[] { "Yes", "No" });
                if (sel == 0)
                {
                    break;
                }
            }
#pragma warning disable CS8604 // Possible null reference argument.
            File.WriteAllText(filename, serialization);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        private void TeeProofText()
        {
            string? filename;
            while (true)
            {
                Console.WriteLine("Asking: (Proof Text) File name.");
                filename = TextInputBoxUI("Where to save the proof?").ToString();
                if ((filename != null) && (filename.Length > 0))
                {
                    break;
                }

                int sel = MessageBox.Query("Cancel or not", "Do you want to cancel?", new ustring[] { "Yes", "No" });
                if (sel == 0)
                {
                    break;
                }
            }
#pragma warning disable CS8604 // Possible null reference argument.
            File.WriteAllText(filename, KnowledgeBase.GenerateReport());
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
