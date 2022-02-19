using System;
using System.Collections.Generic;
using System.Linq;

namespace DefQed.Core
{
    internal class Formula
    {
        // Formula -- logical set of micro statements
        public Bracket TopLevel = new();

        public override string ToString()
        {
            return $"Formula({TopLevel});";
        }

        // Then how to 'evaluate' the formula?
        // Appliable if we know how to evaluate MicroStatements.
        //public bool Evaluate() => TopLevel.Evaluate();

        // Validator: check if the TopLevel can be satisfied within a fixed given set of MSs.
        public bool Validate(List<MicroStatement> situation, ref Dictionary<Symbol, Symbol> transitors) => VisitBracket(TopLevel, situation, ref transitors);

        private bool VisitBracket(Bracket bracket, List<MicroStatement> situation, ref Dictionary<Symbol, Symbol> transitors)
        {
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
                    return !VisitBracket(bracket.SubBrackets[0], situation, ref transitors);
                    //break;
                case BracketType.StatementHolder:

                    // Hey there, question come that the symbols may have different names!
                    // We need a renamer mapping which must be constant.
                    // Transition is now the hardest core part of the proofing process.

                    // Transitor implementation
                    // Dictionary: Symbol in pool -> Symbol in formula

                    if (bracket.MicroStatement == null)
                    {
                        throw new ArgumentNullException("Cannot compare a null micro statement.");
                    }

                    List<MicroStatement> candidates = (List<MicroStatement>)(from item in situation where item.Connector == bracket.MicroStatement.Connector select item);
                    if (candidates.Count == 0)
                    {
                        return false;
                    }

                    // Transitor logic:
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
                        if (!Bracket.CheckIsomorphism(bracket.MicroStatement.Brackets[0], candidates[i].Brackets[0], ref transitors, TransitorOrientation.LeftIndex))
                        {
                            candidates.RemoveAt(i);
                        }
                        if (!Bracket.CheckIsomorphism(bracket.MicroStatement.Brackets[1], candidates[i].Brackets[1], ref transitors, TransitorOrientation.LeftIndex))
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

                    bool resLeft = VisitBracket(bracket.SubBrackets[0], situation, ref transitors);
                    bool resRight = VisitBracket(bracket.SubBrackets[1], situation, ref transitors);

                    return bracket.Connector.Name.ToUpper().Trim() switch
                    {
                        "AND" => resLeft && resRight,
                        "OR" => resLeft || resRight,
                        _ => false,
                    };
                default:
                    throw new ArgumentOutOfRangeException("Invalid bracket type encountered.");
            }
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        }
    }
}
