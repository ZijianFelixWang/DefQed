using System;
using System.Collections.Generic;

namespace DefQed.Core
{
    internal class MicroStatement : IDisposable
    {
        // a microstatement is firstly generated from E&S by other things...

        // These brackets allow type BH 1 , SYH 4
        public Bracket[] Brackets = new Bracket[2]; // L, R
        public Notation Connector = new(); // M

        public MicroStatement()
        {
            Brackets[0] = new();
            Brackets[1] = new();
        }

        public void Dispose()
        {
            Brackets[0].Dispose();
            Brackets[1].Dispose();
            Connector.Dispose();
        }

        // Then how to evaluate it?
        // L    M   R
        // S1   ==  S3
        // We refer to the definition table of notation...
        // Aha, a Pos-2 notation should be evaluatable.
        // But how? For example, we cannot use bruteforcing to define '=='
        // as it is unlimited. We then must have built in notations (Origin = 0)

        // Well... Evaluation should return a boolean.
        public bool Evaluate()
        {
            // Simplification & evaluation of brackets on both side
            // must be done before this (using Reflections).
            // This method is only responsible for an exact equ check.
            if (Connector.Origin != NotationOrigin.Internal)
            {
                // The connector cannot be resolved.
                throw new NotSupportedException("The given connector is not prebuilt. Evaluation of current micro statement is failed.");
            }

            switch (Connector.Name.ToUpper().Trim())
            {
                case "==":
                    // A simple equality check
                    if (Brackets[0].Equals(Brackets[1])) return true;
                    else return false;
                case ">":
                    // Only responsible for direct numbers' check, other is of reflectors' job.
                    if ((Brackets[0].BracketType != BracketType.SymbolHolder) || (Brackets[1].BracketType != BracketType.SymbolHolder))
                    {
                        return false;
                    }

                    if ((Brackets[0].Symbol == null) || (Brackets[1].Symbol == null))
                    {
                        return false;
                    }

////////#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if ((Brackets[0].Symbol.DirectValue == null) || (Brackets[1].Symbol.DirectValue == null))
//#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    {
                        return false;
                    }

////////#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if (Brackets[0].Symbol.DirectValue > Brackets[0].Symbol.DirectValue) return true;
//#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    return false;
                case "<":
                    // Only responsible for direct numbers' check, other is of reflectors' job.
                    if ((Brackets[0].BracketType != BracketType.SymbolHolder) || (Brackets[1].BracketType != BracketType.SymbolHolder))
                    {
                        return false;
                    }

                    if ((Brackets[0].Symbol == null) || (Brackets[1].Symbol == null))
                    {
                        return false;
                    }

////////#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if ((Brackets[0].Symbol.DirectValue == null) || (Brackets[1].Symbol.DirectValue == null))
//#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    {
                        return false;
                    }

////////#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if (Brackets[0].Symbol.DirectValue < Brackets[0].Symbol.DirectValue) return true;
//#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    return false;
                case ">=":
                    // Only responsible for direct numbers' check, other is of reflectors' job.
                    if ((Brackets[0].BracketType != BracketType.SymbolHolder) || (Brackets[1].BracketType != BracketType.SymbolHolder))
                    {
                        return false;
                    }

                    if ((Brackets[0].Symbol == null) || (Brackets[1].Symbol == null))
                    {
                        return false;
                    }

////////#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if ((Brackets[0].Symbol.DirectValue == null) || (Brackets[1].Symbol.DirectValue == null))
//#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    {
                        return false;
                    }

////////#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if (Brackets[0].Symbol.DirectValue >= Brackets[0].Symbol.DirectValue) return true;
//#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    return false;
                case "<=":
                    // Only responsible for direct numbers' check, other is of reflectors' job.
                    if ((Brackets[0].BracketType != BracketType.SymbolHolder) || (Brackets[1].BracketType != BracketType.SymbolHolder))
                    {
                        return false;
                    }

                    if ((Brackets[0].Symbol == null) || (Brackets[1].Symbol == null))
                    {
                        return false;
                    }

////////#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if ((Brackets[0].Symbol.DirectValue == null) || (Brackets[1].Symbol.DirectValue == null))
//#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    {
                        return false;
                    }

////////#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if (Brackets[0].Symbol.DirectValue <= Brackets[0].Symbol.DirectValue) return true;
//#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    return false;
                default:
                    return false;
            }
        }

        public override string ToString() => $"MicroStatement({Brackets[0]} {Connector} {Brackets[1]});";

        public string ToFriendlyString() => $"MicroStatement({Brackets[0].ToFriendlyString()} {Connector.Name} {Brackets[1].ToFriendlyString()}";

        public static string ToFriendlyStringList(List<MicroStatement> situ)
        {
            string res = "{";
            foreach (MicroStatement s in situ)
            {
                res += $"{s.ToFriendlyString()},";
            }
            return res[0..^1];
        }
    }
}
