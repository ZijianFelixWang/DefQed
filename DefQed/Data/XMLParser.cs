using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using DefQed.Core;
using Console = Common.LogConsole;

namespace DefQed.Data
{
    internal static class XMLParser
    {
#pragma warning disable IDE0044 // Add readonly modifier
        private static Dictionary<string, Symbol> SymbolBank = new();
#pragma warning restore IDE0044 // Add readonly modifier

        public static KBase ParseXMLAsync(string filename)
        {
            Console.Log(Common.LogLevel.Diagnostic, $"ParseXMLAsync() Thread {Environment.CurrentManagedThreadId}");
            // LCMP
            KBase k = ParseXMLTask(filename).Result;
            return k;
        }

        private static Task<KBase> ParseXMLTask(string filename)
        {
            var task = Task.Run(() =>
            {
                Console.Log(Common.LogLevel.Diagnostic, "This is to create the xml parser task.");
                Console.Log(Common.LogLevel.Diagnostic, $"ParseXMLTask() Thread {Environment.CurrentManagedThreadId}");
                KBase k = new();
                bool e = false;
                ParseXML(filename, ref k, ref e);
                if (!e)
                {
                    return k;
                }
                else
                {
                    return new();
                }
            });
            return task;
        }

        public static void ParseXML(string filename, ref KBase kbase, ref bool error)
        {
            XmlDocument doc = new();
            XmlReaderSettings settings = new()
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            };

            try
            {
                doc.Load(XmlReader.Create(filename, settings));
            }
            catch (FileNotFoundException ex)
            {
                Console.Log(Common.LogLevel.Error, $"Given XML file doesn't exist.\n{ex.Message}");
                error = true;
                return;
            }
            catch (IOException ex)
            {
                Console.Log(Common.LogLevel.Error, $"Given XML file doesn't exist.\n{ex.Message}");
                error = true;
                return;
            }
            catch (ArgumentException ex)
            {
                Console.Log(Common.LogLevel.Error, $"Given XML file doesn't exist.\n{ex.Message}");
                error = true;
                return;
            }
            catch (XmlException ex)
            {
                Console.Log(Common.LogLevel.Error, $"XML Parse Error.\n{ex.Message}");
                error = true;
                return;
            }

            if (doc.DocumentElement == null)
            {
                Console.Log(Common.LogLevel.Error, "Input XML file is null.");
                error = true;
                return;
            }
            XmlElement root = doc.DocumentElement;
            
            if (root.Name.Trim().ToLower() != "defqed")
            {
                // Base node error.
                Console.Log(Common.LogLevel.Error, "Not a DefQed XML file.");
                error = true;
                return;
            }

            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name.Trim().ToLower())
                {
                    case "connection":
                        ParseConnection(node, ref error);
                        break;
                    case "environment":
                        ParseEnvironment(node, ref kbase, ref error);
                        break;
                    case "statement":
                        ParseStatement(node, ref kbase, ref error);
                        break;
                }
            }
        }

        private static void ParseConnection(XmlNode node, ref bool error)
        {
            if (error)
            {
                return;
            }

            string? user = null;
            string? password = null;
            string? database = null;
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name.Trim().ToLower())
                {
                    case "user":
                        user = child.InnerText;
                        break;
                    case "passwd":
                        password = child.InnerText;
                        break;
                    case "database":
                        database = child.InnerText;
                        break;
                }
            }

            if ((user == null) || (password == null) || (database == null))
            {
                Console.Log(Common.LogLevel.Error, "XML Parse Error in connection tag.");
                error = true;
                return;
            }
            Common.Data.MySQLDriver.connStr = $"server=127.0.0.1;uid={user};pwd={password};database={database}";

            #region commented stuff
            #endregion

            if (!Common.Data.MySQLDriver.Initialize())
            {
                Console.Log(Common.LogLevel.Error, "Connstr " + Common.Data.MySQLDriver.connStr + " failed.");
                error = true;
            }
        }

        private static void ParseEnvironment(XmlNode node, ref KBase kbase, ref bool error)
        {
            if (error)
            {
                return;
            }

            if (kbase == null)
            {
                kbase = new();
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name.Trim().ToLower())
                {
                    case "enroll":
                        #region parse enroll
                        if ((child.Attributes.Count != 1) || (child.Attributes == null))
                        {
                            Console.Log(Common.LogLevel.Error, "XML Parse Error in enroll tag.");
                            error = true;
                            return;
                        }
                        string? category = child.Attributes["category"].Value;
                        string title = child.InnerText;

                        Notation notation = new()
                        {
                            Name = category
                        };

                        if (!KBase.VerifyNotation(ref notation))
                        {
                            // this is a new notation.
                            notation.Origin = NotationOrigin.External;
                            notation.Id = kbase.GetNextNotationId();
                            if (notation.Id != -1)
                            {
                                KBase.InstallNotation(ref notation);
                            }
                            else
                            {
                                error = true;
                                return;
                            }
                        }

                        SymbolBank.Add(title, new Symbol(kbase.GetNextSymbolId(), notation, title));
                        #endregion

                        break;
                    case "force":
                        // I think this should support multiple forcing.
                        // Maybe force should be replaced by that in next version.
                        #region parse force
                        MicroStatement situ = new();
                        string letItem = "";
                        string beItem = "";
                        string letCategory = "";
                        string beCategory = "";
                        foreach (XmlNode forceChild in child.ChildNodes)
                        {
                            switch (forceChild.Name.Trim().ToLower())
                            {
                                case "let":
                                    if ((forceChild.Attributes.Count != 1) || (forceChild.Attributes == null))
                                    {
                                        Console.Log(Common.LogLevel.Error, "XML Parse Error in let tag.");
                                        error = true;
                                        return;
                                    }
                                    letCategory = forceChild.Attributes["category"].Value;
                                    letItem = forceChild.InnerText;
                                    break;
                                case "be":
                                    if ((forceChild.Attributes.Count != 1) || (forceChild.Attributes == null))
                                    {
                                        Console.Log(Common.LogLevel.Error, "XML Parse Error in let tag.");
                                        error = true;
                                        return;
                                    }
                                    beCategory = forceChild.Attributes["category"].Value;
                                    beItem = forceChild.InnerText;
                                    break;
                            }
                        }

                        // Create the MicroStatement now.
                        // This will initialize the pool.
                        situ.Connector = new()
                        {
                            Name = "=="
                        };
                        if (!KBase.VerifyNotation(ref situ.Connector))
                        {
                            // Gosh! The "==" doesn't exist in database.
                            situ.Connector.Origin = NotationOrigin.Internal;
                            situ.Connector.Id = kbase.GetNextNotationId();
                            KBase.InstallNotation(ref situ.Connector);
                        }
                        // connector ready now.

                        situ.Brackets[0].BracketType = BracketType.SymbolHolder;
                        if (!SymbolBank.ContainsKey(letItem))
                        {
                            Console.Log(Common.LogLevel.Error, "Unenrolled let item encountered.");
                            error = true;
                            return;
                        }
                        situ.Brackets[0].Symbol = SymbolBank[letItem];

                        situ.Brackets[0].BracketType = BracketType.SymbolHolder;

                        // We need to generate a notation with type 'category'.
                        if (beCategory == "")
                        {
                            Console.Log(Common.LogLevel.Error, "Failed to parse XML: Error getting beCategory.");
                            error = true;
                            return;
                        }

                        Notation dNotation = new()
                        {
                            Name = beCategory
                        };
                        if (!KBase.VerifyNotation(ref dNotation))
                        {
                            // create new notation
                            dNotation.Id = kbase.GetNextNotationId();
                            dNotation.Origin = NotationOrigin.External;
                            KBase.InstallNotation(ref dNotation);
                        }

                        Symbol sDirect;

                        if(!SymbolBank.ContainsKey(beItem))
                        {
                            // it is a direct thing
                            sDirect = new(dNotation, double.Parse(beItem));
                        }
                        else
                        {
                            sDirect = SymbolBank[beItem];
                        }
                        situ.Brackets[1].Symbol = sDirect;
                        situ.Brackets[1].BracketType = BracketType.SymbolHolder;
                        #endregion

                        // initialize left pool
                        if (kbase.LeftPool == null)
                        {
                            kbase.LeftPool = new();
                        }

                        kbase.LeftPool.Add(situ);

                        break;
                }
            }
        }

        private static void ParseStatement(XmlNode node, ref KBase kbase, ref bool error)
        {
            if (error)
            {
                return;
            }

            // Debug feb.26.2022
            // failure to parse "that": everything null.

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name.Trim().ToLower() != "prove")
                {
                    Console.Log(Common.LogLevel.Error, "Unexpected node encountered in statement tag.");
                    error = true;
                    return;
                }

                // each <prove/> tag refers to one mirostatement.
                MicroStatement toProve = new();

                if ((child.ChildNodes.Count != 1) || (child.ChildNodes == null))
                {
                    Console.Log(Common.LogLevel.Error, "Unexpected error in prove tag.");
                    error = true;
                    return;
                }

//#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (child.ChildNodes[0].Name.Trim().ToLower() != "that")
                {
                    Console.Log(Common.LogLevel.Error, "Unexpected tag encountered in prove tag.");
                    error = true;
                    return;
                }
                toProve.Connector = new()
                {
                    Name = child.ChildNodes[0].Attributes["category"].Value
                };

                if (!KBase.VerifyNotation(ref toProve.Connector))
                {
                    // Connector doesn't exist in KBase.
                    // Register connector now.
                    toProve.Connector.Origin = NotationOrigin.External;
                    toProve.Connector.Id = kbase.GetNextNotationId();
                    KBase.InstallNotation(ref toProve.Connector);
                }

                // Next step: resolve the subs of the child
                if ((child.ChildNodes[0].ChildNodes.Count != 2) || (child.ChildNodes[0].ChildNodes == null))
                {
                    Console.Log(Common.LogLevel.Error, "Error resolving root that tag in prove tag.");
                    error = true;
                    return;
                }
#pragma warning disable CS8604 // Possible null reference argument.
                ParseProveBracket(child.ChildNodes[0].ChildNodes[0], ref kbase, ref toProve.Brackets[0], ref error);
                ParseProveBracket(child.ChildNodes[0].ChildNodes[1], ref kbase, ref toProve.Brackets[1], ref error);
#pragma warning restore CS8604 // Possible null reference argument.
//#pragma warning restore CS8602 // Dereference of a possibly null reference.

                if (kbase.RightPool == null)
                {
                    kbase.RightPool = new();
                }
                kbase.RightPool.Add(toProve);
            }
            return;
        }

        private static void ParseProveBracket(XmlNode node, ref KBase kbase, ref Bracket bracket, ref bool error)
        {
            if (error)
            {
                return;
            }

            if (node.Name.Trim().ToLower() != "that")
            {
                Console.Log(Common.LogLevel.Error, "Unable to resolve that tag.");
                error = true;
                return;
            }

            if (bracket == null)
            {
                // This prohibits null reference from happen.
                bracket = new();
            }

            // Bug insider: XmlNode != XmlElement
            // A blabla text is also a XmlNode...

            if ((node.ChildNodes.Count == 1) && (node.FirstChild.GetType() == typeof(XmlText)))
            {
                // Symbol holder
                bracket.BracketType = BracketType.SymbolHolder;
                if (SymbolBank.ContainsKey(node.InnerText))
                {
                    bracket.Symbol = SymbolBank[node.InnerText];
                    return;
                }
                else
                {
                    Console.Log(Common.LogLevel.Error, "Unenrolled symbol encountered in conclusion.");
                    error = true;
                    return;
                }
            }
            if ((node.Attributes != null) && (node.Attributes.GetNamedItem("category").Value.Trim().ToLower() == "negated"))
            {
                // Negated holder
                bracket.BracketType = BracketType.NegatedHolder;
                if (node.ChildNodes.Count != 1)
                {
                    Console.Log(Common.LogLevel.Error, "Invalid prove bracket encountered: Multiple things to negate.");
                    error = true;
                    return;
                }

                // Here comes some funny recur.
#pragma warning disable CS8604 // Possible null reference argument.
                ParseProveBracket(node.ChildNodes[0], ref kbase, ref bracket.SubBrackets[0], ref error);
#pragma warning restore CS8604 // Possible null reference argument.
                return;
            }

//#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if ((node.ChildNodes.Count == 2) && (node.Attributes != null) && (new List<string>(new string[] {"==", ">", "<", ">=", "<="})).Contains(node.Attributes["category"].Value))
//#pragma warning restore CS8602 // Dereference of a possibly null reference.
            {
                // Bracket holder
                bracket.BracketType = BracketType.BracketHolder;
                Notation connector = new()
                {
//#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    Name = node.Attributes["category"].Value,
//#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    Origin = NotationOrigin.Internal
                };
                if (!KBase.VerifyNotation(ref connector))
                {
                    // bad notation
                    Console.Log(Common.LogLevel.Error, "Invalid connector in bracket to enroll.");
                    error = true;
                    return;
                }

                bracket.Connector = connector;

                Bracket left = new();
                Bracket right = new();
#pragma warning disable CS8604 // Possible null reference argument.
                ParseProveBracket(node.ChildNodes[0], ref kbase, ref left, ref error);
                ParseProveBracket(node.ChildNodes[1], ref kbase, ref right, ref error);
#pragma warning restore CS8604 // Possible null reference argument.

                bracket.SubBrackets[0] = left;
                bracket.SubBrackets[1] = right;

                return;
            }
        }
    }
}
