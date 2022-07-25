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

        private MicroStatement? microStatement;
        private Symbol? symbol;
        private Notation? connector;
        private Bracket[] subBrackets = new Bracket[2];
        private BracketType? bracketType;
        private Satisfaction satisfied = Satisfaction.Unknown;  // This is for DefQed.Core.Formula.Validate().

        public BracketType? BracketType { get => bracketType; set => bracketType = value; }
        public Satisfaction Satisfied { get => satisfied; set => satisfied = value; }
        public Bracket[] SubBrackets { get => subBrackets; set => subBrackets = value; }
        public Notation? Connector { get => connector; set => connector = value; }
        public Symbol? Symbol { get => symbol; set => symbol = value; }
        public MicroStatement? MicroStatement { get => microStatement; set => microStatement = value; }

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
            GC.SuppressFinalize(this);  // CA1816 quality rule
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
            string res = "(";
            res += BracketType switch
            {
                Core.BracketType.BracketHolder => $"{SubBrackets[0].ToFriendlyString()} {Connector.Name} {SubBrackets[1].ToFriendlyString()})",
                Core.BracketType.NegatedHolder => $"!{SubBrackets[0].ToFriendlyString()})",
                Core.BracketType.StatementHolder => $"{MicroStatement.ToFriendlyString()})",
                Core.BracketType.SymbolHolder => $"{Symbol.Name})",
                _ => ")",
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
