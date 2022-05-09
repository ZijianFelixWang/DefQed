using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            foreach (var t in pool)
            {
                Console.Log(LogLevel.Diagnostic, $"OLD Pool Content: {t.ToFriendlyString()}");
            }
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

            foreach (var t in pool)
            {
                Console.Log(LogLevel.Diagnostic, $"Pool Content: {t.ToFriendlyString()}");
            }

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
            List<(Bracket, Bracket)> transistors = new();

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

#if DEBUG
                foreach (var tst in transistors)
                {
                    Console.Log(LogLevel.Diagnostic, $"Pair: {tst.Item1.Symbol.Name} to {tst.Item2.Symbol.Name}");
                }
#endif

                // FTC algorithm called here~
                var ss = FreeTSTCombinator(transistors);
                foreach (var s in ss)
                {
                    foreach (var tst in s)
                    {
                        Console.Log(LogLevel.Diagnostic, $"(SubSet) Pair: {tst.Item1.Symbol.Name} to {tst.Item2.Symbol.Name}");
                    }

                    // Let's joint the conclusion.
                    for (int i = 0; i < reflection.Conclusion.Count; i++)
                    {
                        MicroStatement item = reflection.Conclusion[i];
                        // Apply transistors to item.
                        ApplyTransistors(ref item, s);

                        result.Add(item);
                        Console.Log(LogLevel.Diagnostic, $"DoReflect: Applied transistor {i + 1} of {reflection.Conclusion.Count}.");

                        history += $"[{i}]{item}";
                    }

                    history += "\n";
                }
            }
            else
            {
                Console.Log(LogLevel.Diagnostic, "DoReflect: Formula satisfaction failure.");
            }

            return result;
        }

        private static List<List<(Bracket, Bracket)>> FreeTSTCombinator(List<(Bracket, Bracket)> transistors)
        {
            // There're multiple entries. Let's cut it to subsets.
            /*
             *  [   DEBUG   ][5/4/2022 11:23:21] Pair: a to x
             *  [   DEBUG   ][5/4/2022 11:23:21] Pair: b to y
             *  [   DEBUG   ][5/4/2022 11:23:21] Pair: a to y
             *  [   DEBUG   ][5/4/2022 11:23:21] Pair: b to z
             *  [   DEBUG   ][5/4/2022 11:23:21] Pair: b to x
             *  [   DEBUG   ][5/4/2022 11:23:21] Pair: c to y
             *  [   DEBUG   ][5/4/2022 11:23:21] Pair: b to y
             *  [   DEBUG   ][5/4/2022 11:23:21] Pair: c to z
             *  
             *  Free Combination Algorithm...
             *  
             *  -> all 'a': select a->x // Free Combination Here
             *      -> remove all 'a', 'x'
             *      -> b->y, b->z, c->y, b->y, c->z
             *      -> all 'b': select b->y // FC Here
             *          -> remove all 'b', 'y'
             *          ->  c->z
             *          -> only one.
             *          -> combination done.
             *          -> l: a,b,c cl: x,y,z
             *          -> release (c,z)
             *          -> c->z AGAIN!
            */

            // First, we select one br as 'a'

            //List<Bracket> locked = new();

            List<List<(Bracket, Bracket)>> ret = new();
            FTCNextLevel(new(), new(), transistors, ref ret);
            foreach (var reco in ret)
            {
                Console.Log(LogLevel.Diagnostic, "SubTSTS begin.");
                foreach (var tst in reco)
                {
                    Console.Log(LogLevel.Diagnostic, $"Pair: {tst.Item1.Symbol.Name} to {tst.Item2.Symbol.Name}");
                }
                Console.Log(LogLevel.Diagnostic, "SubTSTS end.");
            }
            return ret;
        }

        private static void FTCNextLevel(List<Bracket> locked, List<Bracket> coLocked, List<(Bracket, Bracket)> transistors, ref List<List<(Bracket, Bracket)>> coms)
        {
            List<Bracket> allLefts = new();
            for (int i = 0; i < transistors.Count; i++)
            {
                if (!allLefts.Contains(transistors[i].Item1))
                {
                    allLefts.Add(transistors[i].Item1);
                }
            }

            //var tOld = transistors;

            for (int i1 = 0; i1 < transistors.Count; i1++)
            {
                (Bracket, Bracket) fir = transistors[i1];
                var a = fir.Item1;
                var x = fir.Item2;
                // a is selected as left of fir.
                if (!locked.Contains(a))
                {
                    // lock a
                    locked.Add(a);
                    coLocked.Add(x);

                    Console.Log(LogLevel.Diagnostic, $"Add lock({a.Symbol.Name}->{x.Symbol.Name})");

                    for (int i = 0; i < transistors.Count; i++)
                    {
                        (Bracket, Bracket) b = transistors[i];
                        Console.Log(LogLevel.Diagnostic, $"{b.Item1.Symbol.Name} {a.Symbol.Name} {b.Item2.Symbol.Name} {x.Symbol.Name}");
                        if ((b.Item1.Symbol.Name == a.Symbol.Name) || (b.Item2.Symbol.Name == x.Symbol.Name))
                        {
                            transistors.Remove(b);
                            Console.Log(LogLevel.Diagnostic, $"Remove b: {b.Item1.Symbol.Name}->{b.Item2.Symbol.Name}");
                        }
                    }
                    transistors.Add((a, x));
                    foreach (var tst in transistors)
                    {
                        //Console.Log(LogLevel.Diagnostic, $"Just Removed Abs: {tst.Item1.Symbol.Name} to {tst.Item2.Symbol.Name}");
                    }
                    // If not the end,
                    // should append to next level.
                    if (locked.Count < allLefts.Count)
                    {
                        FTCNextLevel(locked, coLocked, transistors, ref coms);
                    }
                    else
                    {
                        // all locked. This give a combination.
                        //coms.Add()
                        List<(Bracket, Bracket)> tmp = new();
                        for (int j = 0; j < locked.Count; j++)
                        {
                            tmp.Add((locked[j], coLocked[j]));
                        }
                        coms.Add(tmp);
                        // release locked, colocked (last and more).
                        // HEY, WE NEED TO RESTORE TSTs!
                    }
                }
            }
        }

        private static void ApplyTransistors(ref MicroStatement stmt, List<(Bracket, Bracket)> tst)
        {
            // stmt: a==c, tst: a->x, b->y, c->z

            // let's traversal it!
            ApplyTransistors(ref stmt.Brackets[0], tst);
            ApplyTransistors(ref stmt.Brackets[1], tst);
        }

        // Now the logic is simpler and better.
        private static void ApplyTransistors(ref Bracket br, List<(Bracket, Bracket)> tst)
        {
            // Try replacement...
            foreach (var pair in tst)
            {
                if (pair.Item1.GetHashCode() == br.GetHashCode())
                {
                    br = pair.Item2;
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

        #region cmt
        // eg, {a==b, b==c}=>{a==c}. (Actual: x==y,y==z) TRANSITOR:x->a, y->b, z->c
        // we need to transform "a==c" to "x==z"
        // LCMP
        #endregion

        private static void UpdatePool(List<List<MicroStatement>> rawPools, ref List<MicroStatement> pool)
        {
            // This simple method merges the rawPools into the original pool to keep it updated.
            foreach (var rp in rawPools)
            {
                foreach (var rpContent in rp)
                {
                   // LCMP

                    bool occur = false;
                    foreach (var i in pool)
                    {
                        if (i.ToFriendlyString() == rpContent.ToFriendlyString())
                        {
                            Console.Log(LogLevel.Diagnostic, "Same merge. Ignored.");
                            occur = true;
                        }
                    }
                    if (!occur)
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
