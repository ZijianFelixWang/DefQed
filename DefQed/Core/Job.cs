// For debugging the module.
#if DEBUG
#define __USE_INLINE_XML_URI__
#define __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS__
#define __CTRL_G_TO_RUN__
#define __AUTO_XML_SUBMIT__
//#define __NO_TEE_AFTER_PROOF__
//#define __INSERT_LINE_UI__ //Deprec!
#endif

using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using DefQed.Data;
////using Terminal.Gui
////using NStack
using Console = DefQed.LogConsole;
using Environment = System.Environment;
using TimeSpan = System.TimeSpan;
using Exception = System.Exception;
using System.Collections.Generic;
using System.Diagnostics;

namespace DefQed.Core
{
    internal class Job
    {
        // Description: Job is the class that stores everything related to a DefQed utilization.

        // Maybe some time later the ui codes and job codes should be separated.

        public string XMLFileName = "";
        public KBase KnowledgeBase = new();
        public int TimeOut = 365 * 24 * 3600;   // A year's proof hah

        private Task? ProofTask;
        private bool ProofPalsed = false;

        // Remark: if give a default value will lead into an excpetion...
        //#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        //        public ProgressBar PulseBar;
        //#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        //private bool PulseNow = false;

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
            //PulseBar.Pulse();
            try
            {
                //_ = MessageBox.Query("PerformProof called.", "Start proving...", "Ok");
                Console.Log(LogLevel.Information, "Start proving...");

                // Load reflections...
                KnowledgeBase.LoadReflections();
                //PulseBar.Pulse();
                Console.Log(LogLevel.Diagnostic, "Pulse.");

                ProofTask = new(() =>
                {
                    if (!ProofPalsed)
                    {
                        while (!KnowledgeBase.TryBridging())
                        {
                            Console.Log(LogLevel.Diagnostic, "Bridging failed, try to scan pool.");
                            KnowledgeBase.ScanPools();
                            //if (PulseNow = !PulseNow)
                            //{
                            //Console.Log(LogLevel.Diagnostic, "UI pulse");
                            //Console.Log(LogLevel.Diagnostic, "Pulse.");
                            //}
                            System.Console.ReadLine();
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
                    //_ = MessageBox.Query("Proof done.", "Proof execution done successfully.");
                    
#if !__NO_TEE_AFTER_PROOF__
                    #region commented stuff decomment after debug
                    Console.WriteLine("Below is report generated:");
                    Console.WriteLine(KnowledgeBase.GenerateReport());
                    Console.WriteLine("\nGenerate report file?\nN/ENTER = No; S = Serialized KBase; T = Proof text; B = Both");
                    while (true)
                    {
                        string? sel = Console.ReadLine();
                        if ((sel == null) || (sel.Trim().ToUpper() == "N"))
                        {
                            break;
                        }

                        sel = sel.Trim().ToUpper();
                        if (sel == "S")
                        {
                            TeeSerializedKBase();
                            break;
                        }
                        if (sel == "T")
                        {
                            TeeProofText();
                            break;
                        }
                        if (sel == "B")
                        {
                            TeeSerializedKBase();
                            TeeProofText();
                            break;
                        }
                        Console.WriteLine("\nGenerate report file?\nN/ENTER = No; S = Serialized KBase; T = Proof text; B = Both");
                    }
                    #endregion
#endif
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
                Console.Log(LogLevel.Error, $"Exception envountered. Exception Category: {ex.GetType()}\nException Message: {ex.Message}");
                Console.WriteLine("You can now save the dump file or terminate the program.");
                TeeSerializedKBase();

                Environment.Exit(-2);
            }
        }

#pragma warning disable CA1822 // Mark members as static
//        public void SetLogLevelUI()
//#pragma warning restore CA1822 // Mark members as static
//        {
//            // The ctrl-G...
//#if __CTRL_G_TO_RUN__
//            LoadXMLUI();
//            PerformProof();
//#else
//            Button confirm = new("Ok", is_default: true);
//            Button cancel = new("Cancel", is_default: false);
//            RadioGroup sel = new(2, 2, new ustring[] { "Diagnostic", "Information", "Warning", "Error" }, 1);
//            confirm.Clicked += () =>
//            {
//                Console.LogLevel = sel.SelectedItem switch
//                {
//                    0 => LogLevel.Diagnostic,
//                    1 => LogLevel.Information,
//                    2 => LogLevel.Warning,
//                    3 => LogLevel.Error,
//                    _ => LogLevel.Information
//                };

//                Console.Log(LogLevel.Information, "Log level set to " + Console.LogLevel);

//                Console.Log(LogLevel.Diagnostic, "Diagnostic example.");
//                Console.Log(LogLevel.Information, "Information example.");
//                Console.Log(LogLevel.Warning, "Warning example.");
//                Console.Log(LogLevel.Error, "Error example.");


//                Application.RequestStop();
//            };
//            cancel.Clicked += () =>
//            {
//                Application.RequestStop();
//            };

//            Dialog ui = new("Request input.", 50, 20, confirm, cancel);
//            Label hint = new(1, 1, "Select the log level. The default option is information. Click Ok to confirm, Click cancel to go back.");

//            ui.Add(hint);
//            ui.Add(sel);
//            Application.Run(ui);
//            // Now the loop is finished.
//#endif
//        }

#if __INSERT_LINE_UI__
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
                switch (sel.SelectedItem)
                {
                    case 0:
                        // Notations
                        string title = "";
                        string origin = "";
                        string opacity = "";
//#endif

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        title = TextInputBoxUI("Input for \"title\" column.").ToString();
                        origin = TextInputBoxUI("Input for \"origin\" column.").ToString();
                        opacity = TextInputBoxUI("Input for \"opacity\" column.").ToString(); ;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                        // Get next id
                        int id = MySQLDriver.GetMaxId(TableType.Notations) + 1;

#pragma warning disable CS8601 // Possible null reference assignment.
                        try
                        {
                            MySQLDriver.InsertRow(TableType.Notations, new List<string>(new string[] { "ID", "TITLE", "ORIGIN", "OPACITY" }), new List<string>(new string[] { id.ToString(), title, origin, opacity }));
                        }
                        catch (Exception ex)
                        {
                            Console.Log(LogLevel.Error, "Database failure." + ex.Message);
                            return;
                        }
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
                        int id1;
                        try
                        {
                            id1 = MySQLDriver.GetMaxId(TableType.Reflections) + 1;
                        }
                        catch (Exception ex)
                        {
                            Console.Log(LogLevel.Error, "Database failure." + ex.Message);
                            return;
                        }

#pragma warning disable CS8601 // Possible null reference assignment.
                        try
                        {
                            MySQLDriver.InsertRow(TableType.Reflections, new List<string>(new string[] { "ID", "CASES", "THUSES", "OPACITY" }), new List<string>(new string[] { id1.ToString(), cases, thuses, opacity1 }));
                        }
                        catch (Exception ex)
                        {
                            Console.Log(LogLevel.Error, "Database failure." + ex.Message);
                            return;
                        }
#pragma warning restore CS8601 // Possible null reference assignment.
                        break;

                    case 2:
                        // Registries
                        string content = "";
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        content = TextInputBoxUI("Input for \"content\" column.").ToString();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                        // Get next id
                        int id2;
                        try
                        {
                            id2 = MySQLDriver.GetMaxId(TableType.Registries) + 1;
                        }
                        catch (Exception ex)
                        {
                            Console.Log(LogLevel.Error, "Database failure." + ex.Message);
                            return;
                        }
#pragma warning disable CS8601 // Possible null reference assignment.
                        try
                        {
                            MySQLDriver.InsertRow(TableType.Registries, new List<string>(new string[] { "ID", "CONTENT" }), new List<string>(new string[] { id2.ToString(), content }));
                        }
                        catch (Exception ex)
                        {
                            Console.Log(LogLevel.Error, "Database failure." + ex.Message);
                            return;
                        }
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
#endif

#if __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS__
//#pragma warning disable CA1822 // Mark members as static
        public void SerializeDiagnosticBrackets()
//#pragma warning restore CA1822 // Mark members as static
        {
            //_ = MessageBox.Query("DIAG", "This should be commented after this debug.");
#region serialize cond
            //_ = MessageBox.Query("DIAG", "Serializing debug condition.");
            Formula cond = new();

            cond.TopLevel.BracketType = BracketType.BracketHolder;
            cond.TopLevel.Connector = new Notation
            {
                Name = "AND",
                Id = 2,
                Origin = NotationOrigin.Internal
            };
            cond.TopLevel.SubBrackets = new Bracket[2];
            cond.TopLevel.SubBrackets[0] = new();
            cond.TopLevel.SubBrackets[1] = new();

            cond.TopLevel.SubBrackets[0] = new Bracket
            {
                BracketType = BracketType.StatementHolder,
                MicroStatement = new MicroStatement
                {
                    Brackets = new Bracket[2],
                    Connector = new Notation
                    {
                        Name = "==",
                        Id = 1,
                        Origin = NotationOrigin.Internal
                    }
                }
            };
            cond.TopLevel.SubBrackets[1] = new Bracket
            {
                BracketType = BracketType.StatementHolder,
                MicroStatement = new MicroStatement
                {
                    Brackets = new Bracket[2],
                    Connector = new Notation
                    {
                        Name = "==",
                        Id = 1,
                        Origin = NotationOrigin.Internal
                    }
                }
            };

            // Fix unexpected error...
#pragma warning disable IDE0079 // Remove unnecessary suppression
            if (cond.TopLevel.SubBrackets[0].MicroStatement.Brackets == null)
            {
                cond.TopLevel.SubBrackets[0].MicroStatement.Brackets = new Bracket[2];
            };
#pragma warning restore IDE0079 // Remove unnecessary suppression
            if (cond.TopLevel.SubBrackets[1].MicroStatement.Brackets == null)
            {
                cond.TopLevel.SubBrackets[1].MicroStatement.Brackets = new Bracket[2];
            };
            cond.TopLevel.SubBrackets[0].MicroStatement.Brackets[0] = new();
            cond.TopLevel.SubBrackets[0].MicroStatement.Brackets[1] = new();

            cond.TopLevel.SubBrackets[0].MicroStatement.Brackets[0].BracketType = BracketType.SymbolHolder;
            cond.TopLevel.SubBrackets[0].MicroStatement.Brackets[0].Symbol = new(0, new Notation
            {
                Name = "item",
                Id = 0,
                Origin = NotationOrigin.External
            }, "a");
            cond.TopLevel.SubBrackets[0].MicroStatement.Brackets[1].BracketType = BracketType.SymbolHolder;
            cond.TopLevel.SubBrackets[0].MicroStatement.Brackets[1].Symbol = new(1, new Notation
            {
                Name = "item",
                Id = 0,
                Origin = NotationOrigin.External
            }, "b");

            cond.TopLevel.SubBrackets[1] = new Bracket
            {
                BracketType = BracketType.StatementHolder,
                MicroStatement = new MicroStatement
                {
                    Brackets = new Bracket[2],
                    Connector = new Notation
                    {
                        Name = "==",
                        Id = 1,
                        Origin = NotationOrigin.Internal
                    }
                }
            };
            cond.TopLevel.SubBrackets[1].MicroStatement.Brackets[0] = new();
            cond.TopLevel.SubBrackets[1].MicroStatement.Brackets[1] = new();

            cond.TopLevel.SubBrackets[1].MicroStatement.Brackets[0].BracketType = BracketType.SymbolHolder;
            cond.TopLevel.SubBrackets[1].MicroStatement.Brackets[0].Symbol = new(1, new Notation
            {
                Name = "item",
                Id = 1,
                Origin = NotationOrigin.External
            }, "b");
            cond.TopLevel.SubBrackets[1].MicroStatement.Brackets[1].BracketType = BracketType.SymbolHolder;
            cond.TopLevel.SubBrackets[1].MicroStatement.Brackets[1].Symbol = new(2, new Notation
            {
                Name = "item",
                Id = 2,
                Origin = NotationOrigin.External
            }, "c");
            JsonSerializerOptions op = new()
            {
                IncludeFields = true,
                MaxDepth = 1024
            };

            string json1 = JsonSerializer.Serialize(cond, op);


            //_ = MessageBox.Query("DIAG", "OK, see it in log console.");
            Console.Log(LogLevel.Information, json1);

#endregion
#region serialize conc
            //_ = MessageBox.Query("DIAG", "Serializing debug conclusion.");
            List<MicroStatement> conc = new();
            conc.Add(new MicroStatement
            {
                Connector = new Notation
                {
                    Name = "==",
                    Id = 1,
                    Origin = NotationOrigin.Internal
                },
                Brackets = new Bracket[2]
            });
            conc[0].Brackets[0] = new();
            conc[0].Brackets[1] = new();
            conc[0].Brackets[0].BracketType = BracketType.SymbolHolder;
            conc[0].Brackets[0].Symbol = new(0, new Notation
            {
                Name = "item",
                Id = 0,
                Origin = NotationOrigin.External
            }, "a");
            conc[0].Brackets[1].BracketType = BracketType.SymbolHolder;
            conc[0].Brackets[1].Symbol = new(2, new Notation
            {
                Name = "item",
                Id = 2,
                Origin = NotationOrigin.External
            }, "c");

            json1 = JsonSerializer.Serialize(conc, op);

            //_ = MessageBox.Query("DIAG", "OK, see it in log console.");
            Console.Log(LogLevel.Information, json1);
#endregion
        }
#endif

        //private void TextInputBoxInnerUI(ustring hintText)
        //{

        //    Button confirm = new("Ok", is_default: true);
        //    Button cancel = new("Cancel", is_default: false);
        //    TextField field = new()
        //    {
        //        X = 2,
        //        Y = 2,
        //        Width = 40
        //    };

        //    confirm.Clicked += () =>
        //    {
        //        JustNowTextInput = field.Text;
        //        Application.RequestStop();
        //    };
        //    cancel.Clicked += () =>
        //    {
        //        Application.RequestStop();
        //    };
        //    Dialog ui = new("Request input.", 50, 20, confirm, cancel);
        //    Label hint = new(1, 1, hintText);

        //    ui.Add(hint);
        //    ui.Add(field);
        //    Application.Run(ui);
        //    // Now the loop is finished.
        //}

        //private ustring TextInputBoxUI(ustring hint)
        //{
        //    TextInputBoxInnerUI(hint);
        //    return JustNowTextInput;
        //}

        //// What a ugly line of code here.
        //private ustring JustNowTextInput = "";

        public void SetTimeoutUI()
        {
            //Button confirm = new("Ok", is_default: true);
            //Button cancel = new("Cancel", is_default: false);
            //TextField field = new()
            //{
            //    X = 2,
            //    Y = 2,
            //    Width = 40
            //};
            string? field = System.Console.ReadLine();
            if (!int.TryParse(field, out TimeOut))
            {
                // parse error.
                //_ = MessageBox.ErrorQuery("Bad data format.", "The timeout value is not a valid int.", "Ok");
                Console.Log(LogLevel.Error, "Bad data format. The timeout value is not a valid int.");
            }
        }

        public void LoadXMLUI(string filename)
        {
#if __AUTO_XML_SUBMIT__
            XMLFileName = filename.Trim();
            bool err = false;
            KBase temp = new();

            Stopwatch watch = new();
            watch.Start();
            XMLParser.ParseXML(XMLFileName, ref temp, ref err);
            watch.Stop();
            Console.Log(LogLevel.Information, $"XML parsing done in {watch.ElapsedMilliseconds} ms.");

            if (err)
            {
                Console.Log(LogLevel.Error, "XML parse failure.");
            }
            else
            {
                KnowledgeBase = temp;
                Console.Log(LogLevel.Information, "Congratulations: XML parse and DB connect ok.");
            }

#else
            Button confirm = new("Ok", is_default: true);
            Button cancel = new("Cancel", is_default: false);
            TextField field = new()
            {
                X = 2,
                Y = 2,
                Width = 40,
#if __USE_INLINE_XML_URI__
                Text = @"C:\Users\felix\Documents\projects\DefQed\DefQed\Examples\Diagnostic.xml"
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

                    Stopwatch watch = new();
                    watch.Start();
                    XMLParser.ParseXML(XMLFileName, ref temp, ref err);
                    watch.Stop();
                    Console.Log(LogLevel.Information, $"XML parsing done in {watch.ElapsedMilliseconds} ms.");

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8601 // Possible null reference assignment.

                    if (err)
                    {
                        _ = MessageBox.ErrorQuery("Error.", "XML parse failure.\nFor more details, see the log console.", new ustring[] { "Ok" });
                    }
                    else
                    {
                        KnowledgeBase = temp;
                        _ = MessageBox.Query("Congratulations.", "XML parse and DB connect success.", "Ok");
                        Console.Log(LogLevel.Information, "Congratulations: XML parse and DB connect ok.");
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
#endif
        }

        public void RenewInstanceUI()
        {
            //int choice = MessageBox.Query("Are you sure?", "This operation will dispose the current job and cannot be restored.", defaultButton: 1, buttons: new ustring[] { "Yes", "No" });
            int choice;
            Console.WriteLine("This operation will dispose the current job and cannot be restored. (y,N)");
            string? field = System.Console.ReadLine();
            choice = field switch
            {
                null => 1,
                "y" => 0,
                "Y" => 0,
                "n" => 1,
                "N" => 1,
                _ => 1
            };
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
                KnowledgeBase.Dispose();
                Console.Log(LogLevel.Information, "Instance refreshed.");
            }
        }

        public void DisposeProofUI()
        {
            //int choice = MessageBox.Query("Are you sure?", "This operation will dispose the current proof and cannot be restored.", defaultButton: 1, buttons: new ustring[] { "Yes", "No" });
            Console.WriteLine("This operation will dispose the current proof and cannot be restored. (y,N)");
            string? field = System.Console.ReadLine();
            int choice = field switch
            {
                null => 1,
                "y" => 0,
                "Y" => 0,
                "n" => 1,
                "N" => 1,
                _ => 1
            };
            if (choice == 0)
            {
                if ((ProofTask != null) && (!ProofTask.IsCompleted))
                {
                    ProofTask.Dispose();
                    Console.Log(LogLevel.Information, "Proof disposed successfully.");
                }
                else
                {
                    //MessageBox.Query("Invalid operation.", "There is nothing to dispose.", "Ok");
                    Console.Log(LogLevel.Error, "There is nothing to dispose.");
                }
            }
        }

        public void QuitUI()
        {
            //int choice = MessageBox.Query("Are you sure?", "This will terminate the application.", defaultButton: 1, buttons: new ustring[] { "Yes", "No" });
            Console.WriteLine("This operation will terminate the operation. (y,N)");
            string? field = System.Console.ReadLine();
            int choice = field switch
            {
                null => 1,
                "y" => 0,
                "Y" => 0,
                "n" => 1,
                "N" => 1,
                _ => 1
            };
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
                //Application.Shutdown();
                Environment.Exit(0);
            }
        }

        public void PauseResumeUI()
        {
            if ((ProofTask == null) || (ProofTask.IsCompleted))
            {
                Console.Log(LogLevel.Error, "There is nothing to pause or resume.");
            }

            ProofPalsed = !ProofPalsed;
            Console.Log(LogLevel.Information, "Negated task successfully.");
        }

        private void TeeSerializedKBase()
        {
            string serialization = JsonSerializer.Serialize(KnowledgeBase);
            string? filename;
            while (true)
            {
                System.Console.Write("Asking: (Json Serialized KBase) File name: ");
                filename = System.Console.ReadLine();
                if ((filename != null) && (filename.Length > 0))
                {
                    break;
                }
            }
//#pragma warning disable CS8604 // Possible null reference argument.
            File.WriteAllText(filename, serialization);
//#pragma warning restore CS8604 // Possible null reference argument.
        }

//#pragma warning disable IDE0051 // Remove unused private members
        private void TeeProofText()
//#pragma warning restore IDE0051 // Remove unused private members
        {
            string? filename;
            while (true)
            {
                Console.WriteLine("Asking: (Proof Text) File name.");
                System.Console.Write("Where to save the proof?  ");
                filename = Console.ReadLine();
                if ((filename != null) && (filename.Length > 0))
                {
                    break;
                }
            }
//#pragma warning disable CS8604 // Possible null reference argument.
            File.WriteAllText(filename, KnowledgeBase.GenerateReport());
//#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
