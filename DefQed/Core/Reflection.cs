using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

namespace DefQed.Core
{
    internal record Reflection
    {
        // Reflection must specify a formula of MicroStatements (when this)
        // and a set of MicroStatements (then that). Note: the 'formula' must
        // has logics which supports 'A and (B or C)' or something like that.
        public Formula Condition = new();
        public List<MicroStatement> Conclusion = new();

        public static string Scan(List<Reflection> reflections, ref List<MicroStatement> pool)
        {
            // This scans a pool and apply appliable reflections to it.
            string history = "Scan(";

            // Multithreading capability to be implemented, this makes it easier to migrate to CUDA.
            List<Task<List<MicroStatement>>> tasks = new();
            List<MicroStatement> tempPool = pool;   // to avoid reference error.
            List<List<MicroStatement>> rawResults = new();  // to receive results.

            for (int i = 0; i < reflections.Count; i++)
            {
                tasks.Add(new Task<List<MicroStatement>>(() =>
                {
                    return DoReflect(reflections[i], tempPool, ref history);
                }));
            }

            // Start the task set asyncly.
            foreach (var t in tasks)
            {
                t.Start();
            }

            Task.WaitAll(tasks.ToArray());

            // Get tasks' result and deal with it.
            foreach (var t in tasks)
            {
                rawResults.Add(t.Result);
            }
            UpdatePool(rawResults, ref pool);

            history += ");";
            return history;
        }

        private static List<MicroStatement> DoReflect(Reflection reflection, List<MicroStatement> pool, ref string history)
        {
            // UNDONE: Implement capability to use the OPACITY column.

            List<MicroStatement> result = pool;    // thing to return
            Dictionary<Symbol, Symbol> transitors = new();

            if (reflection.Condition.Validate(pool, ref transitors))
            {
                // The pool satisfies the formula's condition.
                history += $"Reflection = {reflection}, Transitors = Json({JsonSerializer.Serialize(transitors)});,\n\t";

                // Let's joint the conclusion.
                for (int i = 0; i < reflection.Conclusion.Count; i++)
                {
                    MicroStatement item = reflection.Conclusion[i];
                    // Apply transitors to item.
                    ApplyTransitors(ref item, transitors);
                    result.Add(item);

                    history += $"[{i}]{item}";
                }

                history += "\n";
            }
            return result;
        }

        // eg, {a==b, b==c}=>{a==c}. (Actual: x==y,y==z) TRANSITOR:x->a, y->b, z->c
        // we need to transform "a==c" to "x==z"
        private static void ApplyTransitors(ref MicroStatement piece, Dictionary<Symbol, Symbol> transitors)
        {
            // let's traversal everywhere of piece's two brackets and apply each transitor...
            switch (piece.Brackets[0].BracketType)
            {
                case BracketType.NegatedHolder:
                    ApplyTransitors(ref piece.Brackets[0], transitors);
                    break;
                case BracketType.BracketHolder:
                    ApplyTransitors(ref piece.Brackets[0], transitors);
                    ApplyTransitors(ref piece.Brackets[1], transitors);
                    break;
                case BracketType.SymbolHolder:
                    MicroStatement pieceCpy = piece;
                    // REM, a SymbolHolder can only hold one symbol.
                    piece.Brackets[0].Symbol = ((List<Symbol>)(from t in transitors where t.Value == pieceCpy.Brackets[0].Symbol select t.Key))[0];
                    // now the replacement is done. Wow.
                    break;
                default:
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                    throw new ArgumentOutOfRangeException("Cannot apply transitor set to an invalid micro statement.");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }
        }

        private static void ApplyTransitors(ref Bracket bracket, Dictionary<Symbol, Symbol> transitors)
        {
            switch (bracket.BracketType)
            {
                case BracketType.NegatedHolder:
                    ApplyTransitors(ref bracket.SubBrackets[0], transitors);
                    break;
                case BracketType.BracketHolder:
                    ApplyTransitors(ref bracket.SubBrackets[0], transitors);
                    ApplyTransitors(ref bracket.SubBrackets[1], transitors);
                    break;
                case BracketType.SymbolHolder:
                    Bracket br = bracket;
                    br.Symbol = ((List<Symbol>)(from t in transitors where t.Value == br.Symbol select t.Key))[0];
                    // now the replacement is done. Wow.
                    break;
                default:
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                    throw new ArgumentOutOfRangeException("Cannot apply transitor set to an invalid micro statement.");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

            }
        }

        private static void UpdatePool(List<List<MicroStatement>> rawPools, ref List<MicroStatement> pool)
        {
            // This simple method merges the rawPools into the original pool to keep it updated.
            foreach (var rp in rawPools)
            {
                pool = (List<MicroStatement>)pool.Union(rp);
            }
        }

        public override string ToString()
        {
            return $"Reflection(Condition = {Condition}, Conclusions = {Conclusion});";
        }
    }
}
