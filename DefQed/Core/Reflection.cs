using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using System.Linq;
//using System.Text.Json;
using Console = DefQed.LogConsole;
using JsonSerializer2 = Newtonsoft.Json.JsonConvert;

namespace DefQed.Core
{
    internal record Reflection
    {
        // Reflection must specify a formula of MicroStatements (when this)
        // and a set of MicroStatements (then that). Note: the 'formula' must
        // has logics which supports 'A and (B or C)' or something like that.
        public Formula Condition = new();
        public List<MicroStatement> Conclusion = new();

        private string Conclusion2Str()
        {
            string res = "List(";
            for (int i = 0; i < Conclusion.Count; i++)
            {
                res += Conclusion[i].ToString();
                if (i != Conclusion.Count - 1)
                {
                    res += ",";
                }
            }
            res += ")";
            return res;
        }

        public static string Scan(List<Reflection> reflections, ref List<MicroStatement> pool)
        {
            Console.Log(LogLevel.Diagnostic, $"Scan: Preparing to apply scan: {reflections.Count} reflections, {pool.Count} microstatements.");

            // This scans a pool and apply appliable reflections to it.
            string history = "Scan(";

            // Multithreading capability to be implemented, this makes it easier to migrate to CUDA.
            List<Task<List<MicroStatement>>> tasks = new();
            List<MicroStatement> tempPool = pool;   // to avoid reference error.
            List<List<MicroStatement>> rawResults = new();  // to receive results.

            for (int i = 0; i < reflections.Count; i++)
            {
                if (i != reflections.Count)
                {
                    // fix the ghosty bug
                    tasks.Add(new Task<List<MicroStatement>>(() =>
                    {
                        return DoReflect(reflections, i, tempPool, ref history);
                    }));
                    Console.Log(LogLevel.Diagnostic, $"Scan: Created scan task {tasks[^1].Id}");
                }
            }

            // Start the task set asyncly.
            foreach (var t in tasks)
            {
                Console.Log(LogLevel.Diagnostic, $"Scan: Starting scan task {t.Id}.");
                t.Start();
            }

            Task.WaitAll(tasks.ToArray());
            Console.Log(LogLevel.Diagnostic, "Scan: All scans completed.");

            // Get tasks' result and deal with it.
            foreach (var t in tasks)
            {
                rawResults.Add(t.Result);
            }
            UpdatePool(rawResults, ref pool);
            Console.Log(LogLevel.Diagnostic, "Scan: Pool updated.");

            history += ");";
            return history;
        }

        // To prevent the C# bug
        private static List<MicroStatement> DoReflect(List<Reflection> reflections, int index, List<MicroStatement> pool, ref string history)
        {
            if (index >= reflections.Count)
            {
                // I know this will lower performance
                // But this makes the code design simple
                // To be fixed in next version...
                return DoReflect(reflections[0], pool, ref history);
            }
            else
            {
                // Reflect pool with r[i].
                // TODO: To be renamed to RecFunctor later...
                return DoReflect(reflections[index], pool, ref history);
            }
        }

        private static List<MicroStatement> DoReflect(Reflection reflection, List<MicroStatement> pool, ref string history)
        {
            Console.Log(LogLevel.Diagnostic, "DoReflect: Trying to do reflect...");

            List<MicroStatement> result = new(pool);    // thing to return

            // TST: left is req, right is situ
            Dictionary<Bracket, Bracket> transistors = new();

            if (reflection.Condition.Validate(pool, ref transistors))
            {
                // The pool satisfies the formula's condition.
                Console.Log(LogLevel.Diagnostic, "DoReflect: Formula satisfaction success.");

                Newtonsoft.Json.JsonSerializerSettings op = new()
                {
                    MaxDepth = 1024
                };

                string json = JsonSerializer2.SerializeObject(transistors, op);

                history += $"Reflection = {reflection}, Transistors = Json({json});,\n\t";

                // Let's joint the conclusion.
                for (int i = 0; i < reflection.Conclusion.Count; i++)
                {
                    MicroStatement item = reflection.Conclusion[i];
                    // Apply transistors to item.
                    ApplyTransistors(ref item, transistors);

                    // WE NEED TO REWRITE ALL!

                    result.Add(item);
                    Console.Log(LogLevel.Diagnostic, $"DoReflect: Applied transistor {i} of {reflection.Conclusion.Count}.");

                    history += $"[{i}]{item}";
                }

                history += "\n";
            }
            else
            {
                Console.Log(LogLevel.Diagnostic, "DoReflect: Formula satisfaction failure.");
            }

            return result;
        }

        private static void ApplyTransistors(ref MicroStatement stmt, Dictionary<Bracket, Bracket> tst)
        {
            // let's traversal it!
            ApplyTransistors(ref stmt.Brackets[0], tst);
            ApplyTransistors(ref stmt.Brackets[1], tst);
        }

        // Now the logic is simpler and better.
        private static void ApplyTransistors(ref Bracket br, Dictionary<Bracket, Bracket> tst)
        {
            foreach (var key in tst.Keys)
            {
                if (br.Equals(key))
                {
                    br = tst[key];
                    return;
                }
            }
            // if not?
            switch (br.BracketType)
            {
                case BracketType.NegatedHolder:
                    ApplyTransistors(ref br.SubBrackets[0], tst);
                    break;
                case BracketType.BracketHolder:
                    ApplyTransistors(ref br.SubBrackets[0], tst);
                    ApplyTransistors(ref br.SubBrackets[1], tst);
                    break;
                default:
                    break;
            }
        }

        // eg, {a==b, b==c}=>{a==c}. (Actual: x==y,y==z) TRANSITOR:x->a, y->b, z->c
        // we need to transform "a==c" to "x==z"
//        private static void ApplyTransistors(ref MicroStatement piece, Dictionary<Symbol, Symbol> transistors)
//        {
//            // let's traversal everywhere of piece's two brackets and apply each transistor...
//            switch (piece.Brackets[0].BracketType)
//            {
//                case BracketType.NegatedHolder:
//                    ApplyTransistors(ref piece.Brackets[0], transistors);
//                    break;
//                case BracketType.BracketHolder:
//                    ApplyTransistors(ref piece.Brackets[0], transistors);
//                    ApplyTransistors(ref piece.Brackets[1], transistors);
//                    break;
//                case BracketType.SymbolHolder:
//                    MicroStatement pieceCpy = piece;
//                    //Using Linq will cause another InvalidCastException... Damn it.
//                    //REM, a SymbolHolder can only hold one symbol.
//                    //piece.Brackets[0].Symbol = ((List<Symbol>)(from t in transistors where t.Value == pieceCpy.Brackets[0].Symbol select t.Key))[0];
//                    //Symbol s = new();
//                    foreach (var t in transistors)
//                    {
//                        if (t.Value == pieceCpy.Brackets[0].Symbol)
//                        {
//                            //.s = t.Key;
//                            piece.Brackets[0].Symbol = t.Key;
//                        }
//                    }
//                    // now the replacement is done. Wow.
//                    break;
//                case null:
//                    break;
//                default:
//#pragma warning disable CA2208 // Instantiate argument exceptions correctly
//                    throw new ArgumentOutOfRangeException("Cannot apply transistor set to an invalid micro statement.");
//#pragma warning restore CA2208 // Instantiate argument exceptions correctly
//            }
        //}

//        private static void ApplyTransistors(ref Bracket bracket, Dictionary<Symbol, Symbol> transistors)
//        {
//            switch (bracket.BracketType)
//            {
//                case BracketType.NegatedHolder:
//                    ApplyTransistors(ref bracket.SubBrackets[0], transistors);
//                    break;
//                case BracketType.BracketHolder:
//                    ApplyTransistors(ref bracket.SubBrackets[0], transistors);
//                    ApplyTransistors(ref bracket.SubBrackets[1], transistors);
//                    break;
//                case BracketType.SymbolHolder:
//                    Bracket br = bracket;

//                    // For a pair of titem, value is abc (formula things, to be replaced), key is xyz (real things)

//                    //br.Symbol = ((List<Symbol>)(from t in transistors where t.Value == br.Symbol select t.Key))[0];
//                    foreach (var t in transistors)
//                    {
//                        if (t.Value == br.Symbol)
//                        {
//                            //.s = t.Key;
//                            br.Symbol = t.Key;
//                        }
//                    }
//                    // now the replacement is done. Wow.
//                    break;
//                default:
//#pragma warning disable CA2208 // Instantiate argument exceptions correctly
//                    throw new ArgumentOutOfRangeException("Cannot apply transistor set to an invalid micro statement.");
//#pragma warning restore CA2208 // Instantiate argument exceptions correctly

//            }
//        }

        private static void UpdatePool(List<List<MicroStatement>> rawPools, ref List<MicroStatement> pool)
        {
            // This simple method merges the rawPools into the original pool to keep it updated.
            foreach (var rp in rawPools)
            {
                //pool = (List<MicroStatement>)pool.Union(rp);
                foreach (var rpContent in rp)
                {
                    if (!pool.Contains(rpContent))
                    {
                        pool.Add(rpContent);
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"Reflection(Condition = {Condition}, Conclusions = {Conclusion2Str()});";
        }
    }
}
