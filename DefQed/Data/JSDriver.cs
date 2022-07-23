using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.ClearScript.V8;
using DefQed.Core;
using Console = Common.LogConsole;

namespace DefQed.Data
{
    internal static class JSDriver
    {
        public static bool errored = false;

        private static KBase kbase2 = new();
#pragma warning disable IDE0044 // Add readonly modifier
        private static Dictionary<string, Symbol> SymbolBank = new();
#pragma warning restore IDE0044 // Add readonly modifier


        public static void Connect(string user, string passwd, string db)
        {
            if ((user == null) || (passwd == null) || (db == null))
            {
                Console.Log(Common.LogLevel.Error, "Bad connection declaration in JavaScript.");
                errored = true;
                return;
            }
            MySQLDriver.connStr = $"server=127.0.0.1;uid={user};pwd={passwd};database={db}";

            // connect now
            if (!MySQLDriver.Initialize())
            {
                Console.Log(Common.LogLevel.Error, "Connstr " + MySQLDriver.connStr + " failed.");
                errored = true;
                return;
            }
        }

        public static void Enroll(string type, string name)
        {
            Notation notation = new()
            {
                Name = type
            };

            if (!KBase.VerifyNotation(ref notation))
            {
                // this is a new notation.
                notation.Origin = NotationOrigin.External;
                notation.Id = kbase2.GetNextNotationId();
                if (notation.Id != -1)
                {
                    KBase.InstallNotation(ref notation);
                }
                else
                {
                    Console.Log(Common.LogLevel.Error, "Unexpected notation id allocated.");
                    errored = true;
                    return;
                }
            }
            SymbolBank.Add(name, new Symbol(kbase2.GetNextSymbolId(), notation, name));
        }

        public static void LeftPool(List<MicroStatement> s) => kbase2.LeftPool = s;

        public static void RightPool(List<MicroStatement> s) => kbase2.RightPool = s;

        public static MicroStatement MicroStatement(Bracket b0, Notation c, Bracket b1)
        {
            MicroStatement m = new();

            m.Connector = c;
            m.Brackets[0] = b0;
            m.Brackets[1] = b1;

            return m;
        }

        public static Notation Notation(string name)
        {
            Notation n = new()
            {
                Name = name,
                Origin = NotationOrigin.Internal
            };

            if (!KBase.VerifyNotation(ref n))
            {
                n.Origin = NotationOrigin.External;
                n.Id = kbase2.GetNextNotationId();
                KBase.InstallNotation(ref n);
            }

            return n;
        }

        public static Symbol Symbol(string name)
        {
            if (SymbolBank.ContainsKey(name))
            {
                return SymbolBank[name];
            }
            else
            {
                errored = true;
                Console.Log(Common.LogLevel.Error, "Unenrolled symbol name.");
                return new();
            }
        }

        public static Bracket SymbolHolder(Symbol sym) => new()
        {
            BracketType = BracketType.SymbolHolder,
            Satisfied = Satisfaction.Unknown,
            Symbol = sym
        };

        public static Bracket NegatedHolder(Bracket sub)
        {
            Bracket b = new()
            {
                BracketType = BracketType.NegatedHolder,
                Satisfied = Satisfaction.Unknown
            };

            b.SubBrackets[0] = sub;
            b.SubBrackets[1] = new();

            return b;
        }

        public static Bracket StatementHolder(MicroStatement m) => new()
        {
            BracketType = BracketType.SymbolHolder,
            Satisfied = Satisfaction.Unknown,
            MicroStatement = m
        };

        public static Bracket BracketHolder(Bracket b0, Bracket b1)
        {
            Bracket b = new()
            {
                BracketType = BracketType.NegatedHolder,
                Satisfied = Satisfaction.Unknown
            };

            b.SubBrackets[0] = b0;
            b.SubBrackets[1] = b1;

            return b;
        }

        public static void LoadJS(string filename, ref KBase kbase)
        {
            errored = false;
            
            if (kbase == null)
            {
                kbase = new();
            }

            kbase2 = kbase;

            string js = "";
            try
            {
                js = File.ReadAllText(filename);
            }
            catch (Exception exp)
            {
                Console.Log(Common.LogLevel.Error, "Failed to load content from specified file.");
                Console.Log(Common.LogLevel.Error, exp.Message);
                Environment.Exit(-1);
            }

            var engine = new V8ScriptEngine();
            engine.AddHostType("$", typeof(JSDriver));
            engine.Execute(js);

            kbase = kbase2;

            if (errored)
            {
                Console.Log(Common.LogLevel.Error, "Error encountered when loading from javascript file.");
                Environment.Exit(-1);
            }
        }
    }
}