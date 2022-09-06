using DefQed.Core;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.IO;
using Console = Common.LogConsole;

namespace DefQed.Data
{

    /// <summary>
    /// <c>JSDriver</c> provides a way for DefQed to get the two pools from a JavaScript file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class uses MicroSoft.ClearScript API and is tested to be practical on Windows and GNU/Linux.
    /// </para>
    /// <para>
    /// A strange thing is that when configuring the exposure of JSDriver, aliasing will cause the function 
    /// call from the script to fail. I don't know the reason for now.
    /// </para>
    /// </remarks>
    public static class JSDriver
    {
        /// <summary>
        /// (field) Whether the JSDriver loading process has errored.
        /// </summary>
        private static bool errored = false;

        /// <summary>
        /// (field) The KBase constructed by the <c>JSDriver</c> class, to be copied to <c>kbase</c> to return.
        /// </summary>
        private static KBase kbase2 = new();

        /// <summary>
        /// (field) The symbols enrolled by the <c>JSDriver.Enroll()</c> function, to be legally used later.
        /// </summary>
        private static readonly Dictionary<string, Symbol> SymbolBank = new();

        /// <summary>
        /// (field) The left pool constructed by the <c>JSDriver</c> class.
        /// </summary>
        private static readonly List<MicroStatement> LeftPool = new();

        /// <summary>
        /// (field) The right pool constructed by the <c>JSDriver</c> class.
        /// </summary>
        private static readonly List<MicroStatement> RigtPool = new();

        /// <summary>
        /// Whether the JSDriver loading process has errored.
        /// </summary>
        /// <value>
        /// The <c>Errored</c> property represents whether the process has finished successfully. If so, should be <c>false</c>.
        /// </value>
        public static bool Errored { get => errored; set => errored = value; }

        /// <summary>
        /// Connects to the MySQL database specified by its parameters.
        /// </summary>
        /// <remarks>
        /// This functions should be called by the JavaScript file.
        /// </remarks>
        /// <param name="user">User of the database.</param>
        /// <param name="passwd">Password of the user.</param>
        /// <param name="db">Database to connect to.</param>
        public static void Connect(string user, string passwd, string db)
        {
            if ((user == null) || (passwd == null) || (db == null))
            {
                Console.Log(Common.LogLevel.Error, "Bad connection declaration in JavaScript.");
                Errored = true;
                return;
            }
            
            MySQLDriver.ConnStr = $"server=127.0.0.1;uid={user};pwd={passwd};database={db}";

            // connect now
            if (!MySQLDriver.Initialize())
            {
                Console.Log(Common.LogLevel.Error, "Connstr " + MySQLDriver.ConnStr + " failed.");
                Errored = true;
                return;
            }
        }

        /// <summary>
        /// Enrolls a <c>Symbol</c> to the <c>SymbolBank</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <c>Symbol</c> must be enrolled before being used by other functions.
        /// </para>
        /// This functions should be called by the JavaScript file.
        /// </remarks>
        /// <param name="type">The type of the symboll to enroll. Should be a valid notation name.</param>
        /// <param name="name">The name to identify the symbol.</param>
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
                notation.Id = KBase.GetNextNotationId();
                if (notation.Id != -1)
                {
                    KBase.InstallNotation(ref notation);
                }
                else
                {
                    Console.Log(Common.LogLevel.Error, "Unexpected notation id allocated.");
                    Errored = true;
                    return;
                }
            }
            SymbolBank.Add(name, new Symbol(kbase2.GetNextSymbolId(), notation, name));
        }

        /// <summary>
        /// Returns a <c>MicroStatement</c> data structure.
        /// </summary>
        /// <remarks>
        /// This functions should be called by the JavaScript file.
        /// </remarks>
        /// <param name="b0">The left bracket.</param>
        /// <param name="c">The connector of the two brackets. Should be a valid notation.</param>
        /// <param name="b1">The right bracket.</param>
        /// <returns>
        /// Returns the constructed MicroStatement.
        /// </returns>
        public static MicroStatement MicroStatement(Bracket b0, Notation c, Bracket b1)
        {
#pragma warning disable IDE0017 // Simplify object initialization
            MicroStatement m = new();
#pragma warning restore IDE0017 // Simplify object initialization

            m.Connector = c;
            m.Brackets[0] = b0;
            m.Brackets[1] = b1;

            return m;
        }

        /// <summary>
        /// Returns a <c>Notation</c> data structure.
        /// </summary>
        /// <remarks>
        /// This functions should be called by the JavaScript file.
        /// </remarks>
        /// <param name="name">
        /// The name of the notation. If not exists in the database, will be created.
        /// </param>
        /// <returns>
        /// Returns the constructed Notation.
        /// </returns>
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
                n.Id = KBase.GetNextNotationId();
                KBase.InstallNotation(ref n);
            }

            return n;
        }

        /// <summary>
        /// Returns a <c>Symbol</c> data structure.
        /// </summary>
        /// <remarks>
        /// This functions should be called by the JavaScript file.
        /// </remarks>
        /// <param name="name">
        /// The name of the symbol. Must be enrolled before using the <c>Enroll()</c> method.
        /// </param>
        /// <returns>
        /// The constructed symbol.
        /// </returns>
        public static Symbol Symbol(string name)
        {
            if (SymbolBank.ContainsKey(name))
            {
                return SymbolBank[name];
            }
            else
            {
                Errored = true;
                Console.Log(Common.LogLevel.Error, "Unenrolled symbol name.");
                return new();
            }
        }

        /// <summary>
        /// Returns a <c>Bracket</c> with type <c>SymbolHolder</c>.
        /// </summary>
        /// <remarks>
        /// This functions should be called by the JavaScript file.
        /// </remarks>
        /// <param name="sym">The symbol to hold.</param>
        /// <returns>
        /// The constructed bracket.
        /// </returns>
        public static Bracket SymbolHolder(Symbol sym) => new()
        {
            BracketType = BracketType.SymbolHolder,
            Satisfied = Satisfaction.Unknown,
            Symbol = sym
        };

        /// <summary>
        /// Returns a <c>Bracket</c> with type <c>NegatedHolder</c>.
        /// </summary>
        /// <remarks>
        /// This functions should be called by the JavaScript file.
        /// <para>
        /// Note: the negated holder 'negates' the bracket inside. (Just like adding a 'NOT').
        /// </para>
        /// </remarks>
        /// <param name="sub">The bracket to negate.</param>
        /// <returns>
        /// The constructed bracket.
        /// </returns>
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

        /// <summary>
        /// Returns a <c>Bracket</c> with type <c>StatementHolder</c>.
        /// </summary>
        /// <remarks>
        /// This functions should be called by the JavaScript file.
        /// </remarks>
        /// <param name="m">The MicroStatement to hold.</param>
        /// <returns>
        /// The constructed bracket.
        /// </returns>
        public static Bracket StatementHolder(MicroStatement m) => new()
        {
            BracketType = BracketType.SymbolHolder,
            Satisfied = Satisfaction.Unknown,
            MicroStatement = m
        };

        /// <summary>
        /// Returns a <c>Bracket</c> with type <c>BracketHolder</c>.
        /// </summary>
        /// <remarks>
        /// This functions should be called by the JavaScript file.
        /// </remarks>
        /// <param name="b0">The left bracket.</param>
        /// <param name="connector">The connector of the two brackets. Should be a valid notation.</param>
        /// <param name="b1">The right bracket.</param>
        /// <returns>
        /// The constructed bracket.
        /// </returns>
        public static Bracket BracketHolder(Bracket b0, Notation connector, Bracket b1)
        {
            Bracket b = new()
            {
                BracketType = BracketType.BracketHolder,
                Satisfied = Satisfaction.Unknown
            };

            b.SubBrackets[0] = b0;
            b.SubBrackets[1] = b1;
            b.Connector = connector;

            return b;
        }

        /// <summary>
        /// Adds a <c>MicroStatement</c> to the left pool.
        /// </summary>
        /// <remarks>
        /// This functions should be called by the JavaScript file.
        /// </remarks>
        /// <param name="m">
        /// The <c>MicroStatement</c> to be added to the left pool.
        /// </param>
        public static void Left(MicroStatement m) => LeftPool.Add(m);

        /// <summary>
        /// Adds a <c>MicroStatement</c> to the right pool.
        /// </summary>
        /// <remarks>
        /// This functions should be called by the JavaScript file.
        /// </remarks>
        /// <param name="m">
        /// The <c>MicroStatement</c> to be added to the right pool.
        /// </param>
        public static void Right(MicroStatement m) => RigtPool.Add(m);

        /// <summary>
        /// This function should be called by <c>DefQed.Program</c>, executes the targeted JavaScript and
        /// saves to the referenced <c>KBase</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The reference parameter <c>kbase</c> should be passed with an empty or freshly-newed KnowledgeBase
        /// because it will be erased by a new one, <c>kbase2</c>. The called should check the <c>Errored</c>
        /// property to check if the method has executed successfully.
        /// </para>
        /// <para>
        /// Note: if the program is built using 'DEBUG' profile, Console API is also available in the script.
        /// </para>
        /// The JavaScript should NOT call this method.
        /// </remarks>
        /// <param name="filename">Specifies file to be loaded.</param>
        /// <param name="kbase">(reference) The KBase constructed by the method.</param>
        public static void LoadJS(string filename, ref KBase kbase)
        {
            Console.Log(Common.LogLevel.Warning, "LoadJS feature is EXPERIMENTAL.");
            Errored = false;

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

            try
            {
                var engine = new V8ScriptEngine();
                engine.AddHostType(typeof(JSDriver));
#if DEBUG
                engine.AddHostType(typeof(System.Console));
#endif
                Console.Log(Common.LogLevel.Information, "Start to execute JavaScript.");
                engine.Execute(js);
                Console.Log(Common.LogLevel.Information, "JavaScript execution done.");
            }
            catch (Microsoft.ClearScript.ScriptEngineException ex)
            {
                Console.Log(Common.LogLevel.Error, "Script execution error. There may be an error in the js file.");
                Console.Log(Common.LogLevel.Error, $"Details: {ex.Message}");
                Environment.Exit(-1);
            }

            kbase = kbase2;
            kbase.LeftPool = LeftPool;
            kbase.RightPool = RigtPool;

            if (Errored)
            {
                Console.Log(Common.LogLevel.Error, "Error encountered when loading from javascript file.");
                Environment.Exit(-1);
            }
        }
    }
}