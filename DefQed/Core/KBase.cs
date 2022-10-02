﻿#if DEBUG
#define __HIDE_REFLECT_HISTORY__
#endif

using System;
using System.Collections.Generic;
using System.Text.Json;
using DefQed.Data;
using Console = Common.LogConsole;

namespace DefQed.Core
{
    /// <summary>
    /// The <c>KBase</c> class describes the definition for the knowledge base.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A knowledge base, if as a database, contains three types of things:
    /// <list type="bullet">
    /// <item>Notations: contains the registration of all notations available for manipulation.</item>
    /// <item>Reflections: contains the registration of all reflections, referencing to registries.</item>
    /// <item>Registries: storing the inner data structures of reflections.</item>
    /// </list>
    /// </para>
    /// <para>
    /// A knowledge base stores everything that the algorithm 'knows' in its mind. All of the working pools are
    /// content of this instance. This also contains methods concerning the proof's conduction, to be more specific,
    /// scanning pools, reflection loading and tst applier.
    /// </para>
    /// <para>
    /// The MySQL connection should be initialized before this class to be built.
    /// </para>
    /// <para>
    /// The <c>KBase</c> class implements <c>IDisposable</c> and should be disposed after use to save memory.</para>
    /// </remarks>
    public class KBase : IDisposable
    {
        // KBase -- The knowledge base (The real core part)
        // KBase contains: Notations, Reflections, and Registries
        // Before start, initialize MySQL conn first

        /// <summary>
        /// To dispose the class, implementing the <c>IDisposable</c> interface/
        /// </summary>
        /// <remarks>
        /// This disposal will also dispose the notation inside the symbol in a chain.
        /// </remarks>
        public void Dispose()
        {
            LeftPool.Clear();
            RightPool.Clear();
            Reflections.Clear();
            GC.SuppressFinalize(this);  // CA1816 quality rule
        }

        // Pools
        /// <summary>
        /// (field) The pool generated by conditions
        /// </summary>
        /// <remarks>
        /// This is a field, not a property so that there is no value tag in the document xml.
        /// </remarks>
        public List<MicroStatement> LeftPool = new();   // Pool generated by conditions

        /// <summary>
        /// (field) The pool generated by conclusions
        /// </summary>
        /// <remarks>
        /// This is a field, not a property so that there is no value tag in the document xml.
        /// </remarks>
        public List<MicroStatement> RightPool = new();  // Pool generated by conclusions

        // Reflections
        /// <summary>
        /// (field) The reflections loaded by the knowledge base, to be used to conduct proofs.
        /// </summary>
        /// <remarks>
        /// This is a field, not a property so that there is no value tag in the document xml.
        /// </remarks>
        public List<Reflection> Reflections = new();

        // Reflection history
        /// <summary>
        /// (field) The reflection history generated by the algorithm, can be understood as the steps generated.
        /// </summary>
        /// <remarks>
        /// This is a field, not a property so that there is no value tag in the document xml.
        /// </remarks>
        private string ReflectionHistory = "";

        /// <summary>
        /// To get the reflection history from the class
        /// </summary>
        /// <remarks>
        /// The reflection history string is a private field and cannot be read outside the class directly.
        /// Therefore, here is a <c>GenerateReport</c> function, returning the field's content directly.
        /// </remarks>
        /// <returns>
        /// The reflection history string, serving as the proof steps.
        /// </returns>
        public string GenerateReport()
        {
            return $"{ReflectionHistory}";
        }

        // Aux things
        /// <summary>
        /// (field) The symbol identifier record of the currently installed symbols.
        /// </summary>
        /// <remarks>
        /// This field is used and only used by this class's <c>GetNextSymbolId()</c> method and cannot be read or
        /// accessed outside this class.
        /// </remarks>
        private int SymbolIdRecord = 0;

        /// <summary>
        /// Returns the symbol id to be used by the symbol bank installer, from the parsers or loaders.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called by the <c>JSDriver</c> class and the <c>XMLParser</c> class.
        /// </para>
        /// <para>
        /// This method will also cause the <c>SymbolIdRecord</c> field to self-increase.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The next symbol id to be used by the installers.
        /// </returns>
        public int GetNextSymbolId()
        {
            SymbolIdRecord++;
            return SymbolIdRecord - 1;
        }

        /// <summary>
        /// Returns next notation id to be used by the calleds.
        /// </summary>
        /// <remarks>
        /// There is a ugly virtual exception handling system defined here... It will be changed later.
        /// </remarks>
        /// <returns>
        /// Next notation id to be used by the parser, loaders, installers.
        /// </returns>
        public static int GetNextNotationId()
        {
            int id = MySQLDriver.GetMaxId(TableType.Notations);
            if (id == -1)
            {
                Console.Log(Common.LogLevel.Warning, "Using next notation id zero. Do not perform proof.");
                return 0;   // Ignored.
            }
            if (id == -2)
            {
                // To abort protocol.
                Console.Log(Common.LogLevel.Error, "Error signal -2 received.");

                return -1;
            }

            return id + 1;
        }

        // Pool operations
        /// <summary>
        /// This method overviews the two pools and then check if they can satisfy some reflections.
        /// </summary>
        /// <remarks>
        /// This method utilizes the <c>Reflection.Scan</c> method to overview the two pools and then check 
        /// </remarks>
        public void ScanPools()
        {
            // ScanPool -- This method overviews the two pools and then check if they can satisfy some reflections.
            // Then it adds the reflected things to the pools.
            // Call hrc: ScanPools -> Reflection.Scan()
            Console.Log(Common.LogLevel.Diagnostic, $"ScanPools: Scanning left pool.");
            ReflectionHistory += Reflection.Scan(Reflections, ref LeftPool);
            Console.Log(Common.LogLevel.Diagnostic, $"ScanPools: Scanning right pool.");
            ReflectionHistory += Reflection.Scan(Reflections, ref RightPool);
            Console.Log(Common.LogLevel.Diagnostic, $"ScanPools: ReflectionHistory is hiden. RH size is {ReflectionHistory.Length}");
        }

        // TODO: fill up all xml comments.

        // HowTo: Update reflections from just done. This is the principle for the self learning procedure.
        // This includes two parts: Compose [multiple available] all routes -> Insert into reflection records.
        // UNDONE: Self-learning

        // Hey! These things should be done after [bridging] procedure.
        public bool TryBridging()
        {
            bool occur = true;
            foreach (var item in RightPool)
            {
                var hash = item.ToFriendlyString();
                bool subOccur = false;
                foreach (var l in LeftPool)
                {
                    if (l.ToFriendlyString() == hash)
                    {
                        subOccur = true;
                    }
                }
                if (!subOccur)
                {
                    occur = false;
                }
            }
            return occur;
        }

        // Utilities
        public void LoadReflections()
        {
            Console.Log(Common.LogLevel.Information, "Loading reflections.");

            List<List<string>> query = DefQed.Data.MySQLDriver.AcquireWholeTable(TableType.Reflections);
            foreach (List<string> row in query)
            {
                if (row.Count < 4)
                {
                    throw new FormatException("Table reflections in DefQed database is invalid.");
                }

                if ((row == null) || (row[1] == null) || (row[2] == null))
                {
                    throw new ArgumentNullException("Null row queryed in reflections' loading process.");
                }

                // row[1]: Serialized Condition formula registry
                // row[2]: Serialized Conclusions list registry

                string condId = row[1];
                string concId = row[2];
                string condJson = "";
                string concJson = "";

                List<List<string>> query2 = DefQed.Data.MySQLDriver.QueryTable(TableType.Registries, "ID", condId, new List<string>(new string[] { "CONTENT" }));
                foreach (List<string> cRow in query2)
                {
                    if (cRow.Count < 1)
                    {
                        throw new FormatException("Table registries in DefQed database is invalid.");
                    }

                    if ((row == null) || (row[0] == null))
                    {
                        throw new NullReferenceException("Null row queryed in reflections' loading process.");
                    }

                    condJson = cRow[0];
                }

                query2 = DefQed.Data.MySQLDriver.QueryTable(TableType.Registries, "ID", concId, new List<string>(new string[] { "CONTENT" }));
                foreach (List<string> cRow in query2)
                {
                    if (cRow.Count < 1)
                    {
                        throw new FormatException("Table registries in DefQed database is invalid.");
                    }

                    if ((row == null) || (row[0] == null))
                    {
                        throw new NullReferenceException("Null row queryed in reflections' loading process.");
                    }

                    concJson = cRow[0];
                }

                Console.Log(Common.LogLevel.Diagnostic, $"Deserializing reflection {Reflections.Count}.");
                
                JsonSerializerOptions op = new()
                {
                    IncludeFields = true,
                    MaxDepth = 1024
                };

                Reflections.Add(new Reflection()
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    Condition = JsonSerializer.Deserialize<Formula>(condJson, op),
                    Conclusion = JsonSerializer.Deserialize<List<MicroStatement>>(concJson, op)
#pragma warning restore CS8601 // Possible null reference assignment.
                });

                Console.Log(Common.LogLevel.Information, $"Reflection {Reflections.Count} loaded.");
            }
        }

        // Check notation via name, and give notation id back.
        public static bool VerifyNotation(ref Notation notation)
        {
            if (notation == null)
            {
                return false;
            }

            List<string> tmp = new()
            {
                "ID"
            };
            List<List<string>> query = DefQed.Data.MySQLDriver.QueryTable(TableType.Notations, "TITLE", notation.Name.ToUpper().Trim(), tmp);
            if (query.Count > 0)
            {
                notation.Id = int.Parse(query[0][0]);

                tmp.Clear();
                tmp.Add("==");
                tmp.Add(">");
                tmp.Add("<");
                tmp.Add("<=");
                tmp.Add(">=");
                if (tmp.Contains(notation.Name))
                {
                    notation.Origin = NotationOrigin.Internal;
                }
                else
                {
                    notation.Origin = NotationOrigin.External;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void InstallNotation(ref Notation notation)
        {
            List<string> columns = new()
            {
                "ID",
                "TITLE",
                "ORIGIN",
                "OPACITY"
            };

            List<string> values = new();

            // DO = TODO - TO
            // DO: error handling here.
            int max = MySQLDriver.GetMaxId(TableType.Notations);
            if (max == -2)
            {
                // Chose to abort
                Console.Log(Common.LogLevel.Warning, "InstallNotation: Abort.");
                notation = new();
                return;
            }
            else if (max == -1)
            {
                values.Add(Convert.ToString(0));
            }
            else
            {
                values.Add(Convert.ToString(max + 1));
            }

            values.Add(notation.Name);
            string origin = notation.Origin switch
            {
                NotationOrigin.Internal => "0",
                NotationOrigin.External => "1",
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                _ => throw new ArgumentOutOfRangeException("Unexpected notation origin.")
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            };
            values.Add(origin);
            values.Add("1.00");

            MySQLDriver.InsertRow(TableType.Notations, columns, values);
        }
    }
}
