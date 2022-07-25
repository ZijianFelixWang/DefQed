using System;
using System.Collections.Generic;
using Console = Common.LogConsole;

namespace DefQed.Core
{
    public class Formula : IDisposable
    {
        // Formula -- logical set of micro statements
        public Bracket TopLevel = new();

        public void Dispose()
        {
            TopLevel.Dispose();
            GC.SuppressFinalize(this);  // CA1816 quality rule
        }

        public override string ToString()
        {
            return $"{TopLevel.ToFriendlyString()}";
        }

        // Validator: check if the TopLevel can be satisfied within a fixed given set of MSs.

        // Well, the iso-checker and tst-builder algorithm here has serious problems!
        // We need to fix it. But... what if invlove in a binary tree?
        // TopLevel is a stmt holder.(type 3)

        public bool Validate(List<MicroStatement> situation, ref List<(Bracket, Bracket)> transistors)
        {
            var tBackup = transistors;

            bool rete = false;

            if ((TopLevel.BracketType == BracketType.StatementHolder) && (TopLevel.MicroStatement != null))
            {
                rete = ValidateMicroStatement(TopLevel.MicroStatement, situation, ref transistors);
            }
            if ((TopLevel.BracketType == BracketType.BracketHolder) && (TopLevel.SubBrackets[0] != null) && (TopLevel.SubBrackets[1] != null))
            {
                RecurseCheckMicroStatementFamily(TopLevel, situation, ref transistors);
                bool ret = TopLevel.Satisfied switch
                {
                    Satisfaction.Unknown => false,
                    Satisfaction.False => false,
                    Satisfaction.True => true,
                    _ => throw new AccessViolationException("Bad satisfaction type. This may be a bug.")
                };
                rete = ret;
            }
            if ((TopLevel.BracketType == BracketType.NegatedHolder) && (TopLevel.SubBrackets[0] != null))
            {
                RecurseCheckMicroStatementFamily(TopLevel, situation, ref transistors);
                bool ret = TopLevel.Satisfied switch
                {
                    Satisfaction.Unknown => false,
                    Satisfaction.False => false,
                    Satisfaction.True => true,
                    _ => throw new AccessViolationException("Bad satisfaction type. This may be a bug.")
                };
                rete = ret;
            }

            if (!rete)
            {
                transistors = tBackup;
            }
            return rete;
        }

        private static void RecurseCheckMicroStatementFamily(Bracket br, List<MicroStatement> situation, ref List<(Bracket, Bracket)> transistors)
        {
            if ((br.BracketType == BracketType.BracketHolder) && (br.SubBrackets[0] != null) && (br.SubBrackets[1] != null))
            {
                // We need to view its children microstatements.
                RecurseCheckMicroStatementFamily(br.SubBrackets[0], situation, ref transistors);    // a==b
                RecurseCheckMicroStatementFamily(br.SubBrackets[1], situation, ref transistors);    // b==c
                bool br0 = br.SubBrackets[0].Satisfied switch
                {
                    Satisfaction.Unknown => false,
                    Satisfaction.False => false,
                    Satisfaction.True => true,
                    _ => throw new AccessViolationException("Bad satisfaction type. This may be a bug.")
                };
                bool br1 = br.SubBrackets[1].Satisfied switch
                {
                    Satisfaction.Unknown => false,
                    Satisfaction.False => false,
                    Satisfaction.True => true,
                    _ => throw new AccessViolationException("Bad satisfaction type. This may be a bug.")
                };

                switch(br.Connector.Name.ToUpper().Trim())
                {
                    case "AND":
                        br.Satisfied = (br0 && br1) switch
                        {
                            false => Satisfaction.False,
                            true => Satisfaction.True
                        };
                        break;
                    case "OR":
                        br.Satisfied = (br0 || br1) switch
                        {
                            false => Satisfaction.False,
                            true => Satisfaction.True
                        };
                        break;
                    default:
                        break;
                };
            }

            if ((br.BracketType == BracketType.StatementHolder) && (br.MicroStatement != null))
            {
                br.Satisfied = ValidateMicroStatement(br.MicroStatement, situation, ref transistors) switch
                {
                    false => Satisfaction.False,
                    true => Satisfaction.True
                };
            }

            if ((br.BracketType == BracketType.NegatedHolder) && (br.SubBrackets[0] != null))
            {
                RecurseCheckMicroStatementFamily(br.SubBrackets[0], situation, ref transistors);
                br.Satisfied = (!(br.SubBrackets[0].Satisfied switch
                {
                    Satisfaction.Unknown => false,
                    Satisfaction.False => false,
                    Satisfaction.True => true,
                    _ => throw new AccessViolationException("Bad satisfaction type. This may be a bug.")
                })) switch
                {
                    false => Satisfaction.False,
                    true => Satisfaction.True
                };
            }
        }

        private static bool ValidateMicroStatement(MicroStatement req, List<MicroStatement> situation, ref List<(Bracket, Bracket)> transistors)
        {
            // This function validate if a microstatement is a subtree of one of the situations.
            foreach (var t in situation)
            {
                Console.Log(Common.LogLevel.Diagnostic, $"REQ: {req.ToFriendlyString()} SITUATION: {t.ToFriendlyString()}");
            }
            List<bool> res = new();
            for (int i = 0; i < situation.Count; i++)
            {
                res.Add(ValidateMicroStatement(req, situation[i], ref transistors));
            }
            bool ret = false;
            foreach (var r in res)
            {
                if (r == true)
                {
                    ret = true;
                }
            }
            return ret;
        }

        private static bool ValidateMicroStatement(MicroStatement req, MicroStatement situ, ref List<(Bracket, Bracket)> transistors)
        {
            // This function checks whether some part of situ can be 'seen as' req.
            // Example: req: A==B,      situ x==y       (ok)
            //          req: A==B,      situ x+2==y+1   (ok)
            //          req: A==B+C,    situ x==2       (fail)
            //          req: A==B+C,    situ x-4==y+z+p (ok)

            // No need to transform any because some doesn't follow certain arithmatic laws they must be defined.

            return ValidateMicroStatement(req.Brackets[0], situ.Brackets[0], ref transistors)
                && ValidateMicroStatement(req.Brackets[1], situ.Brackets[1], ref transistors);
        }

        private static bool ValidateMicroStatement(Bracket req, Bracket situ, ref List<(Bracket, Bracket)> transistors)
        {
            // This function checks whether some part of situ can be 'seen as' req directly!
            if (req.BracketType != BracketType.SymbolHolder)
            {
                if (req.BracketType != situ.BracketType)
                {
                    // must !ex iso-m because totally different things!
                    return false;
                }
                // so same type. let's go on.
                if (req.BracketType == BracketType.BracketHolder)
                {
                    // DONE: TST Recalling logic.
                    // Hey there if false, tsts must be dropped. No need to worry as if validation failed, there'll be no replacement
                    // but we'd better recall them!
                    if (req.Connector.Name != situ.Connector.Name)
                    {
                        return false;
                    }
                    else
                    {
                        return ValidateMicroStatement(req.SubBrackets[0], situ.SubBrackets[0], ref transistors);
                    }
                }
            }

            // Now we encounter a symbol holder!
            // It seems that we need to rewrite all TST logics.

            transistors.Add((req, situ));
            Console.Log(Common.LogLevel.Diagnostic, $"New TST pair: from {req.ToFriendlyString()} to {situ.ToFriendlyString()};");

            return true;
        }
    }
}
