// For debugging the module.
#if DEBUG
#define __USE_INLINE_XML_URI__
#define __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS__
#define __CTRL_G_TO_RUN__
#define __AUTO_XML_SUBMIT__
#define __DONT_HANDLE_EXCEPTION__
//#define __NO_TEE_AFTER_PROOF__
//#define __INSERT_LINE_UI__ //Deprec!
#endif

using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using DefQed.Data;
using Console = Common.LogConsole;
using TimeSpan = System.TimeSpan;
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

        // Remark: if give a default value will lead into an excpetion...
        
        public void PerformProof()
        {
            PerformProof(TimeOut);
        }

        private void PerformProof(int TimeOut)
        {
#if __DONT_HANDLE_EXCEPTION__
            Console.Log(Common.LogLevel.Information, "Start proving...");

            // Load reflections...
            KnowledgeBase.LoadReflections();

            ProofTask = new(() =>
            {
                while (!KnowledgeBase.TryBridging())
                {
                    Console.Log(Common.LogLevel.Diagnostic, "Bridging failed, try to scan pool.");
                    KnowledgeBase.ScanPools();
                }
                
            });

            Console.Log(Common.LogLevel.Information, "PerformProof called: Start proving");

            ProofTask.Start();

            if (ProofTask.Wait(new TimeSpan(0, 0, 0, 0, TimeOut)))
            {
                Console.Log(Common.LogLevel.Information, "Proof process has finished.");
#if !__NO_TEE_AFTER_PROOF__
                #region commented stuff decomment after debug

                var forDbg = KnowledgeBase.GenerateReport();

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
                        Console.Log(Common.LogLevel.Warning, "This version will not serialize KBase.");
                        break;
                    }
                    if (sel == "T")
                    {
                        TeeProofText();
                        break;
                    }
                    if (sel == "B")
                    {
                        Console.Log(Common.LogLevel.Warning, "This version will not serialize KBase.");
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
                Console.WriteLine("Proof terminated because of timeout.");
                Console.Log(Common.LogLevel.Warning, "This version will not serialize KBase.");
            }
#else
            try
            {
                Console.Log(Common.LogLevel.Information, "Start proving...");

                // Load reflections...
                KnowledgeBase.LoadReflections();
                Console.Log(Common.LogLevel.Diagnostic, "Pulse.");

                ProofTask = new(() =>
                {
                    if (!ProofPalsed)
                    {
                        while (!KnowledgeBase.TryBridging())
                        {
                            Console.Log(Common.LogLevel.Diagnostic, "Bridging failed, try to scan pool.");
                            KnowledgeBase.ScanPools();
                        }
                    }
                });

                Console.Log(Common.LogLevel.Information, "PerformProof called: Start proving");

                ProofTask.Start();

                if (ProofTask.Wait(new TimeSpan(0, 0, 0, 0, TimeOut)))
                {
                    Console.Log(Common.LogLevel.Information, "Proof process has finished.");
#if !__NO_TEE_AFTER_PROOF__
            #region commented stuff decomment after debug
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
                Console.Log(Common.LogLevel.Error, $"Exception envountered. Exception Category: {ex.GetType()}\nException Message: {ex.Message}");
                Console.WriteLine("You can now save the dump file or terminate the program.");
                TeeSerializedKBase();

                Environment.Exit(-2);
            }
#endif
        }

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
                        int id = DefQed.Data.MySQLDriver.GetMaxId(TableType.Notations) + 1;

#pragma warning disable CS8601 // Possible null reference assignment.
                        try
                        {
                            DefQed.Data.MySQLDriver.InsertRow(TableType.Notations, new List<string>(new string[] { "ID", "TITLE", "ORIGIN", "OPACITY" }), new List<string>(new string[] { id.ToString(), title, origin, opacity }));
                        }
                        catch (Exception ex)
                        {
                            Console.Log(Common.LogLevel.Error, "Database failure." + ex.Message);
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
                            id1 = DefQed.Data.MySQLDriver.GetMaxId(TableType.Reflections) + 1;
                        }
                        catch (Exception ex)
                        {
                            Console.Log(Common.LogLevel.Error, "Database failure." + ex.Message);
                            return;
                        }

#pragma warning disable CS8601 // Possible null reference assignment.
                        try
                        {
                            DefQed.Data.MySQLDriver.InsertRow(TableType.Reflections, new List<string>(new string[] { "ID", "CASES", "THUSES", "OPACITY" }), new List<string>(new string[] { id1.ToString(), cases, thuses, opacity1 }));
                        }
                        catch (Exception ex)
                        {
                            Console.Log(Common.LogLevel.Error, "Database failure." + ex.Message);
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
                            id2 = DefQed.Data.MySQLDriver.GetMaxId(TableType.Registries) + 1;
                        }
                        catch (Exception ex)
                        {
                            Console.Log(Common.LogLevel.Error, "Database failure." + ex.Message);
                            return;
                        }
#pragma warning disable CS8601 // Possible null reference assignment.
                        try
                        {
                            DefQed.Data.MySQLDriver.InsertRow(TableType.Registries, new List<string>(new string[] { "ID", "CONTENT" }), new List<string>(new string[] { id2.ToString(), content }));
                        }
                        catch (Exception ex)
                        {
                            Console.Log(Common.LogLevel.Error, "Database failure." + ex.Message);
                            return;
                        }
#pragma warning restore CS8601 // Possible null reference assignment.
                        break;

                    default:
                        Console.Log(Common.LogLevel.Error, "Internal Error: Unexpected selection.");
                        break;
                }

                Console.Log(Common.LogLevel.Warning, "You need to restart the instance to load changes.");

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
        public static void SerializeDiagnosticBrackets()
//#pragma warning restore CA1822 // Mark members as static
        {
#region serialize cond
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

            Console.Log(Common.LogLevel.Information, json1);

#endregion
#region serialize conc
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

            Console.Log(Common.LogLevel.Information, json1);
#endregion
        }
#endif

        public void LoadXML(string filename)
        {
//#if __AUTO_XML_SUBMIT__
            XMLFileName = filename.Trim();
            bool err = false;
            KBase temp = new();

            Stopwatch watch = new();
            watch.Start();
            XMLParser.ParseXML(XMLFileName, ref temp, ref err);
            watch.Stop();
            Console.Log(Common.LogLevel.Information, $"XML parsing done in {watch.ElapsedMilliseconds} ms.");

            if (err)
            {
                Console.Log(Common.LogLevel.Error, "XML parse failure.");
            }
            else
            {
                KnowledgeBase = temp;
                Console.Log(Common.LogLevel.Information, "Congratulations: XML parse and DB connect ok.");
            }

        }

//#pragma warning disable IDE0051 // Remove unused private members
        private void TeeProofText()
//#pragma warning restore IDE0051 // Remove unused private members
        {
            string? filename;
            while (true)
            {
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
