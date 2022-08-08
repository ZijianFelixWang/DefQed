using System;
using System.Collections.Generic;
using Console = Common.LogConsole;

namespace DefQed.Core
{
    /// <summary>
    /// The <c>Formula</c> class describes the definition for a logical set of micro statements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <c>formula</c> is basically a bracket which acts as a logical structure of micro-statements.
    /// </para>
    /// <para>
    /// The objects declared by the Formula class is yet simple. However, most of its functionalities 
    /// are put into pratice via its methods, which walk through the whole structure.
    /// </para>
    /// <para>
    /// The <c>Formula</c> class implements <c>IDisposable</c> and should be disposed after use to save memory.
    /// </para>
    /// </remarks>
    public class Formula : IDisposable
    {
        /// <summary>
        /// (field) This field stores the <c>topLevel</c> bracket.
        /// </summary>
        private Bracket topLevel = new();

        /// <summary>
        /// The <c>TopLevel</c> property defines the inner structure of a formula.
        /// </summary>
        /// <value>
        /// The bracket structure of the formula as a bracket.
        /// </value>
        public Bracket TopLevel { get => topLevel; set => topLevel = value; }

        /// <summary>
        /// To dispose the class, implementing the <c>IDisposable</c> interface/
        /// </summary>
        /// <remarks>
        /// This disposal will also dispose the bracket <c>topLevel</c> inside the formula class.
        /// </remarks>
        public void Dispose()
        {
            TopLevel.Dispose();
            GC.SuppressFinalize(this);  // CA1816 quality rule
        }

        /// <summary>
        /// Generates a string to display the bracket, used in generating proof text.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method just explicitly calls the <c>ToFriendlyString</c> method whose output is more readable.
        /// </para>
        /// <para>
        /// The existence of this method is necessary because the <c>ToString</c> method is the default converter
        /// from the formula object to the string object.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A string illustrating the details of the formula.
        /// </returns>
        public override string ToString()
        {
            return $"{TopLevel.ToFriendlyString()}";
        }

        /// <summary>
        /// Checks if the TopLevel can be satisfied within a fixed given set of micro statements.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method has a reference parameter: <c>transitors</c>. If the validation is successful, a dictionary of transistors is built
        /// which willbe utilized by the transistors' applier method to execute the transistor pairs.
        /// </para>
        /// <para>
        /// If the check is not successful, the output transistors will certainly be incorrect, which means it should be ignored by the caller.
        /// </para>
        /// <para>
        /// Actually, this method is just a wrapper and driver for the private method <c>RecurseCheckMicroStatementFamily</c> and the calling hierachy is
        /// actually rather complicated. Nevertheless, the count of methods designed should not decrease, making the design more readable and easier to 
        /// understand.
        /// </para>
        /// <para>
        /// It is necessary to note that the validator only checks if the formula satisfies something. It can be understood like a diff command,
        /// outputing the difference of comparasion (tst) and fails if the difference is too large, although this illustration is rather not exact.
        /// </para>
        /// </remarks>
        /// <param name="situation">A set of mico statements to be checked for satisfaction.</param>
        /// <param name="transistors">The set/dictionary of transistor pairs to be built if the check is successful.</param>
        /// <returns>A boolean representing whether the validation process is successful.</returns>
        /// <exception cref="AccessViolationException">
        /// This exception is thrown if an unexpected satisfaction type is encountered. User will never meet this exception because actually it is 
        /// totally impossible for the satisfaction type to be wrong. However, if the program's memory is modified or editted or infected or damaged,
        /// this exception might raise.
        /// </exception>
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

        /// <summary>
        /// Checks if a bracket can be satisfied by certain list of micro statements, called a situation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is private and is called by <c>Validator</c>. The validator gives this method the stuff to check and this methods performs 
        /// the check.
        /// </para>
        /// <para>
        /// This methods recurese during the checking workflow if a bracket holder or negated holder is encounter being the parameter.
        /// </para>
        /// <para>
        /// For some type of more technical checking, the method explicitly turns to <c>ValidateMicroStatement</c> to complete the task assigned.
        /// </para>
        /// </remarks>
        /// <param name="br">The bracket to check.</param>
        /// <param name="situation">A set of mico statements to be checked for satisfaction.</param>
        /// <param name="transistors">The set/dictionary of transistor pairs to be built if the check is successful.</param>
        /// <exception cref="AccessViolationException">
        /// This exception is thrown if an unexpected satisfaction type is encountered. User will never meet this exception because actually it is 
        /// totally impossible for the satisfaction type to be wrong. However, if the program's memory is modified or editted or infected or damaged,
        /// this exception might raise.
        /// </exception>
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

        /// <summary>
        /// Validates if a micro statement is a subtree of one of the situation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A subtree of a bracket means a 'part' of the bracket. This methods tries to find a place inside the bracket's tree data structure 
        /// where the income micro statement <c>req</c> is exactly the structure.
        /// </para>
        /// <para>
        /// This method will call itself as well as its variations in order to perform the inspection.
        /// </para>
        /// </remarks>
        /// <param name="req">The micro statement to check</param>
        /// <param name="situation">A list of micro statements serving as the resource to inspect.</param>
        /// <param name="transistors">The set/dictionary of transistor pairs to be built if the check is successful.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks whether some part of the <c>situ</c> can be seen as the requirement micro statement.
        /// </summary>
        /// <remarks>
        /// Here comes an example, which used to be a test code.
        /// <para>
        /// Example: req: A==B,      situ x==y       (ok)
        ///          req: A==B,      situ x+2==y+1   (ok)
        ///          req: A==B+C,    situ x==2       (fail)
        ///          req: A==B+C,    situ x-4==y+z+p (ok)
        /// </para>
        /// <para>
        /// No need to transform any because some doesn't follow certain arithmatic laws they must be defined.
        /// </para>
        /// </remarks>
        /// <param name="req">The micro statement to check</param>
        /// <param name="situ">The micro statement serving as the resource of inspection.</param>
        /// <param name="transistors">The set/dictionary of transistor pairs to be built if the check is successful.</param>
        /// <returns></returns>
        private static bool ValidateMicroStatement(MicroStatement req, MicroStatement situ, ref List<(Bracket, Bracket)> transistors)
        {
            return ValidateMicroStatement(req.Brackets[0], situ.Brackets[0], ref transistors)
                && ValidateMicroStatement(req.Brackets[1], situ.Brackets[1], ref transistors);
        }

        /// <summary>
        /// This function checks whether some part of situ can be 'seen as' req directly!
        /// </summary>
        /// <remarks>
        /// <para>
        /// I strongly recommend you to view the source code of the function for furthur understanding because the comments are clear.
        /// </para>
        /// <para>
        /// This is called by the validate micro statement function to actually performs the checking procedure.
        /// </para>
        /// </remarks>
        /// <param name="req">The bracket to check.</param>
        /// <param name="situ">The bracket serving as the resource of inspection.</param>
        /// <param name="transistors">The set/dictionary of transistor pairs to be built if the check is successful.</param>
        /// <returns></returns>
        private static bool ValidateMicroStatement(Bracket req, Bracket situ, ref List<(Bracket, Bracket)> transistors)
        {
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
