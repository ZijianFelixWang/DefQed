using System;
using System.Collections.Generic;

namespace DefQed.Core
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    internal class Bracket
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        // Bracket -- the minimized structure that holds
        // To make it more general, logics are defined using Notations.
        // For this symbolization is not necessary.
        // 1) hold two brackets with a notation (origin = 0)
        // 2) hold a negated bracket
        // 3) hold a micro statement
        // 4) hold a symbol

        public MicroStatement? MicroStatement = null;
        public Symbol? Symbol = null;
        public Notation? Connector = null;
        public Bracket[] SubBrackets = new Bracket[2];

        public BracketType? BracketType = null;

        public bool Satisfied = false;  // This is for DefQed.Core.Formula.Validate().

        public static bool CheckIsomorphism(Bracket b0, Bracket b1, ref Dictionary<Symbol, Symbol> transitors, TransitorOrientation bias)
        {
            // Isomorphism means the two brackets are with the same structure.
            // This has no sense about the Symbol's actual Name, Id, DV but Notation (aka, Symbol type) must be equal.

            if (b0.BracketType != b1.BracketType)
            {
                return false;
            }

            if (b0.BracketType == Core.BracketType.BracketHolder)
            {
                if (b0.Connector != b1.Connector)
                {
                    return false;
                }
                return (CheckIsomorphism(b0.SubBrackets[0], b1.SubBrackets[0], ref transitors, bias) &&
                    CheckIsomorphism(b0.SubBrackets[1], b1.SubBrackets[1], ref transitors, bias)) ||
                    (CheckIsomorphism(b0.SubBrackets[0], b1.SubBrackets[1], ref transitors, bias) &&
                    CheckIsomorphism(b0.SubBrackets[1], b1.SubBrackets[0], ref transitors, bias));    // Symmetry safe ensured.
            }

            if (b0.BracketType == Core.BracketType.SymbolHolder)
            {
                if ((b0.Symbol == null) || (b1.Symbol == null) || (b0.Symbol.Notation != b1.Symbol.Notation))
                {
                    return false;
                }

                // okay, true. Now let's build transitor now.
                
                (Symbol, Symbol) tuple = (bias switch
                {
                    TransitorOrientation.LeftIndex => b0.Symbol,
                    TransitorOrientation.RightIndex => b1.Symbol,
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                    _ => throw new ArgumentOutOfRangeException("Invalid TransitorOrientation encountered.")
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
                }, bias switch
                {
                    TransitorOrientation.LeftIndex => b0.Symbol,
                    TransitorOrientation.RightIndex => b1.Symbol,
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                    _ => throw new ArgumentOutOfRangeException("Invalid TransitorOrientation encountered.")
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
                });

                if (!transitors.ContainsKey(tuple.Item1))
                {
                    transitors.Add(tuple.Item1, tuple.Item2);
                }
                // We add transitors just and the rest job is not ours.
            }

            return true;
        }

        public bool Evaluate()
        {
            if (BracketType == Core.BracketType.SymbolHolder) return false;

            if ((BracketType == Core.BracketType.StatementHolder) || (MicroStatement == null))
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                return MicroStatement.Evaluate();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

            if (BracketType == Core.BracketType.NegatedHolder)
                return !SubBrackets[0].Evaluate();

            if (Connector == null) return false;

            if (BracketType == Core.BracketType.BracketHolder)
            {
                return Connector.Name.ToUpper().Trim() switch
                {
                    "AND" => SubBrackets[0].Evaluate() && SubBrackets[1].Evaluate(),
                    "OR" => SubBrackets[0].Evaluate() || SubBrackets[1].Evaluate(),
                    _ => false,
                };
            }

            return false;
        }

        public override bool Equals(object? obj)
        {
            return obj is Bracket bracket &&
                   EqualityComparer<MicroStatement?>.Default.Equals(MicroStatement, bracket.MicroStatement) &&
                   EqualityComparer<Symbol?>.Default.Equals(Symbol, bracket.Symbol) &&
                   EqualityComparer<Notation?>.Default.Equals(Connector, bracket.Connector) &&
                   EqualityComparer<Bracket[]>.Default.Equals(SubBrackets, bracket.SubBrackets) &&
                   BracketType == bracket.BracketType;
        }

        public override string ToString()
        {
            string res = $"Bracket(Satisfied = {Satisfied}, Type = {BracketType},\n\t";
            res += BracketType switch
            {
                Core.BracketType.BracketHolder => $"{SubBrackets[0]} {Connector} {SubBrackets[1]});",
                Core.BracketType.NegatedHolder => $"Negated({SubBrackets[0]}););",
                Core.BracketType.StatementHolder => $"{MicroStatement});",
                Core.BracketType.SymbolHolder => $"{Symbol});",
                _ => ");",
            };
            return res;
        }
    }

    internal enum BracketType
    {
        BracketHolder,      // mode 1
        NegatedHolder,      // mode 2
        StatementHolder,    // mode 3
        SymbolHolder        // mode 4
    };

    internal enum TransitorOrientation
    {
        LeftIndex,  // b0 is index
        RightIndex  // b1 is index
    };
}
