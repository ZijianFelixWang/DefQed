using System;
using System.Collections.Generic;
using System.Text.Json;
using Console = Common.LogConsole;

namespace DefQed.Core
{
    public class Formula : IDisposable
    {
        // Formula -- logical set of micro statements
        public Bracket TopLevel = new();

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            TopLevel.Dispose();
        }

        public override string ToString()
        {
            return $"Formula({TopLevel});";
        }

        private static string Transistors2Str(Dictionary<Symbol, Symbol> tr)
        {
            string res = "[";

            JsonSerializerOptions op = new()
            {
                IncludeFields = true,
                MaxDepth = 1024
            };

            foreach (var t in tr)
            {
                res += $"({JsonSerializer.Serialize(t.Key, op)}, {JsonSerializer.Serialize(t.Value, op)})";
            }

            res += "]";

            return res;
        }

        // Then how to 'evaluate' the formula?
        // Appliable if we know how to evaluate MicroStatements.
        //public bool Evaluate() => TopLevel.Evaluate();

        // Validator: check if the TopLevel can be satisfied within a fixed given set of MSs.
        //public bool Validate(List<MicroStatement> situation, ref Dictionary<Symbol, Symbol> transistors) => VisitBracket(TopLevel, situation, ref transistors);

        // Well, the iso-checker and tst-builder algorithm here has serious problems!
        // We need to fix it. But... what if invlove in a binary tree?
        // TopLevel is a stmt holder.(type 3)

        public bool Validate(List<MicroStatement> situation, ref List<(Bracket, Bracket)> transistors)
        {
            var tBackup = transistors;

            bool rete = false;

            if ((TopLevel.BracketType == BracketType.StatementHolder) && (TopLevel.MicroStatement != null))
            {
                rete = ValidateMicroStatement(TopLevel.MicroStatement, situation, ref transistors);
            }
            if ((TopLevel.BracketType == BracketType.BracketHolder) && (TopLevel.SubBrackets[0] != null) && (TopLevel.SubBrackets[1] != null))
            {
                RecurseCheckMicroStatementFamily(TopLevel, situation, ref transistors);
                bool ret = TopLevel.Satisfied switch
                {
                    Satisfaction.Unknown => false,
                    Satisfaction.False => false,
                    Satisfaction.True => true,
                    _ => throw new AccessViolationException("Bad satisfaction type. This may be a bug.")
                };
                rete = ret;
            }
            if ((TopLevel.BracketType == BracketType.NegatedHolder) && (TopLevel.SubBrackets[0] != null))
            {
                RecurseCheckMicroStatementFamily(TopLevel, situation, ref transistors);
                bool ret = TopLevel.Satisfied switch
                {
                    Satisfaction.Unknown => false,
                    Satisfaction.False => false,
                    Satisfaction.True => true,
                    _ => throw new AccessViolationException("Bad satisfaction type. This may be a bug.")
                };
                rete = ret;
            }

            if (!rete)
            {
                transistors = tBackup;
            }
            return rete;
        }

        private static void RecurseCheckMicroStatementFamily(Bracket br, List<MicroStatement> situation, ref List<(Bracket, Bracket)> transistors)
        {
            if ((br.BracketType == BracketType.BracketHolder) && (br.SubBrackets[0] != null) && (br.SubBrackets[1] != null))
            {
                // We need to view its children microstatements.
                RecurseCheckMicroStatementFamily(br.SubBrackets[0], situation, ref transistors);    // a==b
                RecurseCheckMicroStatementFamily(br.SubBrackets[1], situation, ref transistors);    // b==c
                bool br0 = br.SubBrackets[0].Satisfied switch
                {
                    Satisfaction.Unknown => false,
                    Satisfaction.False => false,
                    Satisfaction.True => true,
                    _ => throw new AccessViolationException("Bad satisfaction type. This may be a bug.")
                };
                bool br1 = br.SubBrackets[1].Satisfied switch
                {
                    Satisfaction.Unknown => false,
                    Satisfaction.False => false,
                    Satisfaction.True => true,
                    _ => throw new AccessViolationException("Bad satisfaction type. This may be a bug.")
                };

                switch(br.Connector.Name.ToUpper().Trim())
                {
                    case "AND":
                        br.Satisfied = (br0 && br1) switch
                        {
                            false => Satisfaction.False,
                            true => Satisfaction.True
                        };
                        break;
                    case "OR":
                        br.Satisfied = (br0 || br1) switch
                        {
                            false => Satisfaction.False,
                            true => Satisfaction.True
                        };
                        break;
                    default:
                        break;
                };
            }

            if ((br.BracketType == BracketType.StatementHolder) && (br.MicroStatement != null))
            {
                br.Satisfied = ValidateMicroStatement(br.MicroStatement, situation, ref transistors) switch
                {
                    false => Satisfaction.False,
                    true => Satisfaction.True
                };
            }

            if ((br.BracketType == BracketType.NegatedHolder) && (br.SubBrackets[0] != null))
            {
                RecurseCheckMicroStatementFamily(br.SubBrackets[0], situation, ref transistors);
                br.Satisfied = (!(br.SubBrackets[0].Satisfied switch
                {
                    Satisfaction.Unknown => false,
                    Satisfaction.False => false,
                    Satisfaction.True => true,
                    _ => throw new AccessViolationException("Bad satisfaction type. This may be a bug.")
                })) switch
                {
                    false => Satisfaction.False,
                    true => Satisfaction.True
                };
            }
        }

        private static bool ValidateMicroStatement(MicroStatement req, List<MicroStatement> situation, ref List<(Bracket, Bracket)> transistors)
        {
            // This function validate if a microstatement is a subtree of one of the situations.
            foreach (var t in situation)
            {
                Console.Log(Common.LogLevel.Diagnostic, $"REQ: {req.ToFriendlyString()} SITUATION: {t.ToFriendlyString()}");
            }
            List<bool> res = new();
            for (int i = 0; i < situation.Count; i++)
            {
                res.Add(ValidateMicroStatement(req, situation[i], ref transistors));
            }
            bool ret = false;
            foreach (var r in res)
            {
                if (r == true)
                {
                    ret = true;
                }
            }
            return ret;
        }

        private static bool ValidateMicroStatement(MicroStatement req, MicroStatement situ, ref List<(Bracket, Bracket)> transistors)
        {
            // This function checks whether some part of situ can be 'seen as' req.
            // Example: req: A==B,      situ x==y       (ok)
            //          req: A==B,      situ x+2==y+1   (ok)
            //          req: A==B+C,    situ x==2       (fail)
            //          req: A==B+C,    situ x-4==y+z+p (ok)

            // No need to transform any because some doesn't follow certain arithmatic laws they must be defined.

            return ValidateMicroStatement(req.Brackets[0], situ.Brackets[0], ref transistors)
                && ValidateMicroStatement(req.Brackets[1], situ.Brackets[1], ref transistors);
        }

        private static bool ValidateMicroStatement(Bracket req, Bracket situ, ref List<(Bracket, Bracket)> transistors)
        {
            // This function checks whether some part of situ can be 'seen as' req directly!
            if (req.BracketType != BracketType.SymbolHolder)
            {
                if (req.BracketType != situ.BracketType)
                {
                    // must !ex iso-m because totally different things!
                    return false;
                }
                // so same type. let's go on.
                if (req.BracketType == BracketType.BracketHolder)
                {
                    // DONE: TST Recalling logic.
                    // Hey there if false, tsts must be dropped. No need to worry as if validation failed, there'll be no replacement
                    // but we'd better recall them!
                    if (req.Connector.Name != situ.Connector.Name)
                    {
                        return false;
                    }
                    else
                    {
                        return ValidateMicroStatement(req.SubBrackets[0], situ.SubBrackets[0], ref transistors);
                    }
                }
            }

            // Now we encounter a symbol holder!
            // It seems that we need to rewrite all TST logics.

            transistors.Add((req, situ));
            Console.Log(Common.LogLevel.Diagnostic, $"New TST pair: from {req.ToFriendlyString()} to {situ.ToFriendlyString()};");

            return true;
        }

        private bool VisitBracket(Bracket bracket, List<MicroStatement> situation, ref Dictionary<Symbol, Symbol> transistors)
        {
            JsonSerializerOptions op = new()
            {
                IncludeFields = true,
                MaxDepth = 1024
            };

#if __NO_VERBOSE_SERIALIZATION__
            Console.Log(Common.LogLevel.Diagnostic, $"VisitBracket: visiting... bracket= ..., situation= ..., transistors= ...");
#else
            Console.Log(Common.LogLevel.Diagnostic, $"VisitBracket: visiting... bracket= {bracket.ToFriendlyString()}, situation= {MicroStatement.ToFriendlyStringList(situation)}, transistors= {Transistors2Str(transistors)}");
#endif

            // Initially bracket = TopLevel

#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            if (bracket == null)
            {
                throw new ArgumentNullException("Bracket to visit is null.");
            }

            switch (bracket.BracketType)
            {
                case BracketType.SymbolHolder:
                    throw new ArgumentOutOfRangeException("Cannot have a symbol holder bracket in formulas.");
                case BracketType.NegatedHolder:
                    return !VisitBracket(bracket.SubBrackets[0], situation, ref transistors);
                    //break;
                case BracketType.StatementHolder:

                    // Hey there, question come that the symbols may have different names!
                    // We need a renamer mapping which must be constant.
                    // Transition is now the hardest core part of the proofing process.

                    // Transistor implementation
                    // Dictionary: Symbol in pool -> Symbol in formula

                    if (bracket.MicroStatement == null)
                    {
                        throw new ArgumentNullException("Cannot compare a null micro statement.");
                    }

                    // Hey this linq will cause InvalidCastException
                   
                    List<MicroStatement> candidates = new();
                    foreach(var item in situation)
                    {
                        if (item.Connector == bracket.MicroStatement.Connector)
                        {
                            candidates.Add(item);
                        }
                    }

                    if (candidates.Count == 0)
                    {
                        return false;
                    }

                    // Transistor logic:
                    // Aha! first compare bracket structure. This requires a way to
                    // check if two brackets' structures are with isomorphism.

                    for (int i = 0;; i++)
                    {
                        // To avoid ArgumentOutOfRangeException:
                        if (i >= candidates.Count)
                        {
                            break;
                        }

                        // In the following perspective, the b0 bracket should be... the top br of a MicroStatement's brs.
                        if (!Bracket.CheckIsomorphism(
                            bracket.MicroStatement.Brackets[0],
                            candidates[i].Brackets[0],
                            ref transistors,
                            TransistorOrientation.LeftIndex))
                        {
                            candidates.RemoveAt(i);
                        }
                        if (!Bracket.CheckIsomorphism(
                            bracket.MicroStatement.Brackets[1],
                            candidates[i].Brackets[1],
                            ref transistors,
                            TransistorOrientation.LeftIndex))
                        {
                            candidates.RemoveAt(i);
                        }
                    }
                    // Here, if there still ex candidates, it's valid and may be multiple!

                    if (candidates.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case BracketType.BracketHolder:
                    if (bracket.Connector == null)
                    {
                        throw new ArgumentNullException("Bracket to visit has a null connector.");
                    }

                    bool resLeft = VisitBracket(bracket.SubBrackets[0], situation, ref transistors);
                    bool resRight = VisitBracket(bracket.SubBrackets[1], situation, ref transistors);

                    return bracket.Connector.Name.ToUpper().Trim() switch
                    {
                        "AND" => resLeft && resRight,
                        "OR" => resLeft || resRight,
                        _ => false,
                    };
                case null:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException("Invalid bracket type encountered.");
            }
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        }
    }
}
