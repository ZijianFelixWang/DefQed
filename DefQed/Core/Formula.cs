using System;
using System.Collections.Generic;
using System.Text.Json;
//using System.Linq;
using Console = DefQed.LogConsole;

namespace DefQed.Core
{
    internal class Formula : IDisposable
    {
        // Formula -- logical set of micro statements
        public Bracket TopLevel = new();

        public void Dispose()
        {
            TopLevel.Dispose();
        }

        public override string ToString()
        {
            return $"Formula({TopLevel});";
        }

        private string Transistors2Str(Dictionary<Symbol, Symbol> tr)
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
        public bool Validate(List<MicroStatement> situation, ref Dictionary<Symbol, Symbol> transistors) => VisitBracket(TopLevel, situation, ref transistors);

        private bool VisitBracket(Bracket bracket, List<MicroStatement> situation, ref Dictionary<Symbol, Symbol> transistors)
        {
            JsonSerializerOptions op = new()
            {
                IncludeFields = true,
                MaxDepth = 1024
            };

            Console.Log(LogLevel.Diagnostic, $"VisitBracket: visiting... bracket= {JsonSerializer.Serialize(bracket, op)}, situation= {JsonSerializer.Serialize(situation, op)}, transistors= {Transistors2Str(transistors)}");

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
                    //List<MicroStatement> candidates = (List<MicroStatement>)(from item in situation where item.Connector == bracket.MicroStatement.Connector select item);
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
                        if (!Bracket.CheckIsomorphism(bracket.MicroStatement.Brackets[0], candidates[i].Brackets[0], ref transistors, TransistorOrientation.LeftIndex))
                        {
                            candidates.RemoveAt(i);
                        }
                        if (!Bracket.CheckIsomorphism(bracket.MicroStatement.Brackets[1], candidates[i].Brackets[1], ref transistors, TransistorOrientation.LeftIndex))
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
