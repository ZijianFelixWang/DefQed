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
        }

        // Then how to evaluate it?
        // L    M   R
        // S1   ==  S3
        // We refer to the definition table of notation...
        // Aha, a Pos-2 notation should be evaluatable.
        // But how? For example, we cannot use bruteforcing to define '=='
        // as it is unlimited. We then must have built in notations (Origin = 0)

        // Well... Evaluation should return a boolean.
        // A lot of useless stuff deleted here!
        // Less code, more performance.

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
