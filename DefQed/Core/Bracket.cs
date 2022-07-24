using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Console = Common.LogConsole;

namespace DefQed.Core
{
    public class Bracket : IDisposable
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

        public Satisfaction Satisfied = Satisfaction.Unknown;  // This is for DefQed.Core.Formula.Validate().

        // Will cause stack overflow
        //Lines of code removed here.

        public void Dispose()
        {
            MicroStatement = null;
            Symbol = null;
            Connector = null;
            if (SubBrackets[0] != null)
            {
                SubBrackets[0].Dispose();
            }
            if (SubBrackets[1] != null)
            {
                SubBrackets[1].Dispose();
            }
            BracketType = null;
        }

        public static bool CheckIsomorphism(Bracket b0, Bracket b1, ref Dictionary<Symbol, Symbol> transistors, TransistorOrientation bias)
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

                Console.Log(Common.LogLevel.Diagnostic, "Isomorphism checker: entering lower level.");

                return (CheckIsomorphism(b0.SubBrackets[0], b1.SubBrackets[0], ref transistors, bias) &&
                    CheckIsomorphism(b0.SubBrackets[1], b1.SubBrackets[1], ref transistors, bias)) ||
                    (CheckIsomorphism(b0.SubBrackets[0], b1.SubBrackets[1], ref transistors, bias) &&
                    CheckIsomorphism(b0.SubBrackets[1], b1.SubBrackets[0], ref transistors, bias));    // Symmetry safe ensured.
            }

            if (b0.BracketType == Core.BracketType.SymbolHolder)
            {
                if ((b0.Symbol == null) || (b1.Symbol == null) || (b0.Symbol.Notation != b1.Symbol.Notation))
                {
                    return false;
                }

                // okay, true. Now let's build transistor now.
                
                // real   fake
                // xyz    abc
                (Symbol, Symbol) tuple = (bias switch
                {
                    TransistorOrientation.LeftIndex => b0.Symbol,
                    TransistorOrientation.RightIndex => b1.Symbol,
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                    _ => throw new ArgumentOutOfRangeException("Invalid TransistorOrientation encountered.")
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
                }, bias switch
                {
                    TransistorOrientation.LeftIndex => b1.Symbol,
                    TransistorOrientation.RightIndex => b0.Symbol,
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                    _ => throw new ArgumentOutOfRangeException("Invalid TransistorOrientation encountered.")
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
                });

                if (!transistors.ContainsKey(tuple.Item1))
                {
                    transistors.Add(tuple.Item1, tuple.Item2);
                }
                // We add transistors just and the rest job is not ours.
                Console.Log(Common.LogLevel.Diagnostic, $"New transistor (r,f) = ({tuple.Item1.Name}, {tuple.Item2.Name})");
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            return GetHashCode() == obj.GetHashCode();
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

        public string ToFriendlyString()
        {
            string res = "Bracket(";
            res += BracketType switch
            {
                Core.BracketType.BracketHolder => $"{SubBrackets[0].ToFriendlyString()} {Connector.Name} {SubBrackets[1].ToFriendlyString()});",
                Core.BracketType.NegatedHolder => $"!({SubBrackets[0].ToFriendlyString()}););",
                Core.BracketType.StatementHolder => $"{MicroStatement.ToFriendlyString()});",
                Core.BracketType.SymbolHolder => $"{Symbol.Name});",
                _ => ");",
            };
            return res;
        }

        private Bracket GetHashableBracket()
        {
            return new Bracket
            {
                BracketType = BracketType,
                Connector = Connector,
                MicroStatement = MicroStatement,
                SubBrackets = SubBrackets,
                Symbol = Symbol
            };
        }

        public override int GetHashCode()
        {
            //return base.GetHashCode();
            JsonSerializerOptions op = new()
            {
                IncludeFields = true,
                MaxDepth = 1024
            };

            var sha3 = new Sha1Digest();

            byte[] input = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(GetHashableBracket(), op));
            sha3.BlockUpdate(input, 0, input.Length);
            byte[] output = new byte[64];
            sha3.DoFinal(output, 0);

            string hashString = BitConverter.ToString(output);
            hashString = hashString.Replace("-", "").ToLowerInvariant();
            hashString = hashString.Replace("0", "").ToLowerInvariant();

            try
            {
                return int.Parse(hashString.AsSpan(0, 8), System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }

    public enum BracketType
    {
        BracketHolder,      // mode 1
        NegatedHolder,      // mode 2
        StatementHolder,    // mode 3
        SymbolHolder        // mode 4
    };

    public enum TransistorOrientation
    {
        LeftIndex,  // b0 is index
        RightIndex  // b1 is index
    };

    public enum Satisfaction
    {
        Unknown,
        False,
        True
    };
}
