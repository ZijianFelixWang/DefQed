// For debugging the module.
#if DEBUG
//#define __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS__
#endif

using System.Threading.Tasks;
#if __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS
using System.Text.Json;
#endif
using System.IO;
using DefQed.Data;
using Console = Common.LogConsole;
using TimeSpan = System.TimeSpan;
#if __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS
using System.Collections.Generic;
#endif
using System.Diagnostics;

namespace DefQed.Core
{
    /// <summary>
    /// <c>Job</c> is the class that stores everything related to a DefQed utilization.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Significantly, the <c>Job</c> data structure enables the capacity to deal with
    /// multiple proof jobs at a same time. However, this awesome feature has not yet
    /// been implemented by me.
    /// </para>
    /// <para>
    /// This part also contains the main proof loop of the program.
    /// </para>
    /// </remarks>
    internal class Job
    {
        /// <summary>
        /// (field) This field stores the KnowledgeBase used by the proof.
        /// </summary>
        public KBase KnowledgeBase = new();
        
        /// <summary>
        /// (field) This field stores the value for the <c>TimeOut</c> property.
        /// </summary>
        private int timeOut = 365 * 24 * 3600;   // A year's proof hah

        /// <summary>
        /// (field) This field stores the proof text (as output) in a string.
        /// </summary>
        private string proofOutput = "";

        /// <summary>
        /// (field) This field stores the value for the switch controlling to tee the proof text or to not.
        /// </summary>
        private bool notToTee;

        /// <summary>
        /// (field) This is the <c>Task</c> of proving, to be called by execution, enabling multitasking again.
        /// </summary>
        private Task? ProofTask;

        /// <summary>
        /// Whether to tee the proof text to a file or not.
        /// </summary>
        /// <value>
        /// The <c>NotToTee</c> property, if set to <c>true</c> will show the proof steps to console instead of a file.
        /// </value>
        public bool NotToTee { get => notToTee; set => notToTee = value; }

        /// <summary>
        /// The detailed proof steps to present to the user. This should NOT be modified by the caller.
        /// </summary>
        /// <value>
        /// The <c>ProofOutput</c> property stores the detailed proof steps to present to the user.
        /// </value>
        public string ProofOutput { get => proofOutput; set => proofOutput = value; }

        /// <summary>
        /// The time limit of proving the problem. The default value is a whole year, nearly infinity for computers.
        /// </summary>
        /// <value>
        /// This <c>TimeOut</c> property configures the time limit of solving, as default value equals to a year.
        /// </value>
        public int TimeOut { get => timeOut; set => timeOut = value; }

        /// <summary>
        /// This method is a wrapper for method <c>PerformProof(int TimeOut), using the default timeout.</c>
        /// </summary>
        /// <remarks>
        /// <para>
        /// I admit that if the method is designed using an optional parameter, it will be more beautiful.
        /// </para>
        /// <para>
        /// The design will be fixed & improved later.
        /// </para>
        /// </remarks>
        public void PerformProof()
        {
            PerformProof(TimeOut);
        }

        /// <summary>
        /// The entry method of performing the proof, including the main loop.
        /// </summary>
        /// <remarks>
        /// This method does these things:
        /// <list type="bullet">
        /// <item>
        /// Load reflections with method <c>KBase.LoadReflections()</c>
        /// </item>
        /// <item>
        /// Define a <c>ProofTask</c> and execute it, including the main <c>while</c> loop.
        /// </item>
        /// <item>
        /// Measure the time used by the <c>ProofTask</c> execution and terminate it with <c>timeout</c>.
        /// </item>
        /// <item>
        /// Tee the proof steps into a file based on user's configuration.
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="TimeOut">The custom value for <c>TimeOut</c>.</param>
        private void PerformProof(int TimeOut)
        {
            Stopwatch w2 = new();
            w2.Start();

            // Load reflections...
            KnowledgeBase.LoadReflections();

            ProofTask = new(() =>
            {
                while (!KnowledgeBase.TryBridging())
                {
                    Console.Log(Common.LogLevel.Information, "Bridging failed, try to scan pool.");
                    KnowledgeBase.ScanPools();
                }
                
            });

            Console.Log(Common.LogLevel.Information, "PerformProof called: Start proving");

            ProofTask.Start();

            if (ProofTask.Wait(new TimeSpan(0, 0, 0, 0, TimeOut)))
            {
                w2.Stop();
                _ =     MySQLDriver.Terminate();
                Console.Log(Common.LogLevel.Information, $"Proof process has finished in {w2.ElapsedMilliseconds} ms.");

#if DEBUG
                Console.Log(Common.LogLevel.Diagnostic, "Below is proof:");
                Console.WriteLine(KnowledgeBase.GenerateReport());
                Console.Log(Common.LogLevel.Diagnostic, "----------------------");
#else
                if (NotToTee)
                {
                    Console.Log(Common.LogLevel.Diagnostic, "Below is proof:");
                    Console.WriteLine(KnowledgeBase.GenerateReport());
                    Console.Log(Common.LogLevel.Diagnostic, "----------------------");
                }
#endif

                if (!NotToTee)
                {
                    TeeProofText();
                }
            }
            else
            {
                // timeout termination encountered.
                Console.Log(Common.LogLevel.Error, "Proof terminated because of timeout.");
                Console.Log(Common.LogLevel.Information, $"Current Steps:\n{KnowledgeBase.GenerateReport()}");
            }
        }

#if __ALLOW_SERIALIZE_DIAGNOSTIC_BRACKETS__
        /// <summary>
        /// (debug) If enabled, could serialize the diagnostic brackets and show on console.
        /// </summary>
        /// <remarks>
        /// Note: This method is neither a generator for brackets nor a toy or demonstration of 
        /// the program. Instead, it is only valuable for certain initial debugging purposes are 
        /// will be removed some time later.
        /// </remarks>
        public static void SerializeDiagnosticBrackets()
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
            List<MicroStatement> conc = new()
            {
                new MicroStatement
                {
                    Connector = new Notation
                    {
                        Name = "==",
                        Id = 1,
                        Origin = NotationOrigin.Internal
                    },
                    Brackets = new Bracket[2]
                }
            };
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

        /// <summary>
        /// Utilizes the <c>JSDriver.LoadJS()</c> method (or internal API) to load & execute JavaScript.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will also measure the time used by the loading process and show it to the user.
        /// </para>
        /// <para>
        /// Note: the database will be connected if loading is successful.
        /// </para>
        /// </remarks>
        /// <param name="filename">The file containing JavaScript related, to be executed.</param>
        public void LoadJS(string filename)
        {
            Stopwatch watch = new();
            watch.Start();
            JSDriver.LoadJS(filename, ref KnowledgeBase);
            watch.Stop();
            Console.Log(Common.LogLevel.Information, $"JS load done in {watch.ElapsedMilliseconds} ms");
            Console.Log(Common.LogLevel.Information, "Congratulations: All set up, ready to work.");
        }

        /// <summary>
        /// Utilizes the <c>XMLParser.ParseXML</c> method (or internal API) to load information from XML.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will also measure the time used by the parser.
        /// </para>
        /// <para>
        /// Note: the database will be connected if the parser does its job successfully.
        /// </para>
        /// </remarks>
        /// <param name="filename">The XML file to be parsed.</param>
        public void LoadXML(string filename)
        {
//#if __AUTO_XML_SUBMIT__
            bool err = false;
            KBase temp = new();

            Stopwatch watch = new();
            watch.Start();
            XMLParser.ParseXML(filename.Trim(), ref temp, ref err);
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

        /// <summary>
        /// This method outputs the proof text (steps) to a file by asking the user for its location.
        /// </summary>
        /// <remarks>
        /// The asking is done by prompting a question inside the console. Therefore, no GUI file
        /// selector will be shown or displayed in this version. However, it may be added as another
        /// option later in version next.
        /// </remarks>
        private void TeeProofText()
        {
            string? filename;

            if (ProofOutput == "")
            {
                while (true)
                {
                    System.Console.Write("Where to save the proof?  ");
                    filename = Console.ReadLine();
                    if ((filename != null) && (filename.Length > 0))
                    {
                        break;
                    }
                }
            }
            else
            {
                filename = ProofOutput;
            }
//#pragma warning disable CS8604 // Possible null reference argument.
            File.WriteAllText(filename, KnowledgeBase.GenerateReport());
//#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}