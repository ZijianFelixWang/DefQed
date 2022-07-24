using System;
using System.Collections.Generic;

namespace DefQed.Core
{
    public class MicroStatement : IDisposable
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
            GC.SuppressFinalize(this);  // CA1816 quality rule
        }

        public override string ToString() => $"MicroStatement({Brackets[0]} {Connector} {Brackets[1]});";

        public string ToFriendlyString() => $"[{Brackets[0].ToFriendlyString()} {Connector.Name} {Brackets[1].ToFriendlyString()}]";

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
