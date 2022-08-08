using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Text;
using System.Text.Json;

namespace DefQed.Core
{
    /// <summary>
    /// The <c>Bracket</c> class stands for the minimized structure that holds something.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To make the algorithm's design more general, further logics are defined using <c>Notation</c>s. Moreoever,
    /// symbolization for some logical stuff is not necessary.
    /// </para>
    /// <para>
    /// Each <c>Bracket</c> can do one of these four things:
    /// <list type="number">
    /// <item>Hold two brackets with a notation (origin = 0)</item>
    /// <item>Hold a negated bracket</item>
    /// <item>Hold a <c>MicroStatement</c></item>
    /// <item>Hold a symbol</item>
    /// </list>
    /// </para>
    /// <para>
    /// The <c>Bracket</c> class implements <c>IDisposable</c> and should be disposed after use to save memory.
    /// </para>
    /// </remarks>
    public class Bracket : IDisposable
    {
        /// <summary>
        /// (field) This field stores the micro statement held.
        /// </summary>
        private MicroStatement? microStatement;

        /// <summary>
        /// (field) This field stores the symbol held.
        /// </summary>
        private Symbol? symbol;

        /// <summary>
        /// (field) This field stores the connector when being used as Bracketolder.
        /// </summary>
        private Notation? connector;

        /// <summary>
        /// (field) This field stores the subbracket(s) when being used as BracketHolder or NegatedHolder.
        /// </summary>
        private Bracket[] subBrackets = new Bracket[2];

        /// <summary>
        /// (field) This field stores the type for the bracket, which is also nullable.
        /// </summary>
        private BracketType? bracketType;

        /// <summary>
        /// (field) This field is used by the Validator, with default value Unknown.
        /// </summary>
        private Satisfaction satisfied = Satisfaction.Unknown;  // This is for DefQed.Core.Formula.Validate().

        /// <summary>
        /// The <c>BracketType</c> property defines the type (one in four) of the bracket.
        /// </summary>
        /// <value>
        /// The type for the bracket, to distinguish its category.
        /// </value>
        public BracketType? BracketType { get => bracketType; set => bracketType = value; }

        /// <summary>
        /// The <c>Satisfied</c> boolean property is used by the validator to investigate the brackets.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For more information, see also <c>DefQed.Core.Formula.Validate()</c> function, where this property is
        /// used for a few times.
        /// </para>
        /// <para>
        /// The default value for this property is <c>BracketType.Unknown</c>, which explains its naming origin itself.
        /// </para>
        /// </remarks>
        /// <value>
        /// Whether the bracket's micro statement is satisfied, to be used by <c>Validator</c>.
        /// </value>
        public Satisfaction Satisfied { get => satisfied; set => satisfied = value; }

        /// <summary>
        /// The <c>SubBrackets</c> property is a two-value array holding brackets when being used as BracketHolder or 
        /// NegatedHolder utilizations.
        /// </summary>
        /// <remarks>
        /// If being used as BracketHolder, the sub brackets are correspondlingly the left bracket and the right bracket.
        /// Otherwise, if being used as NegatedHolder, the first sub bracket is the bracket to negate, with the second empty.
        /// </remarks>
        /// <value>
        /// The bracket(s) held by this bracket, on the next (sub) level.
        /// </value>
        public Bracket[] SubBrackets { get => subBrackets; set => subBrackets = value; }

        /// <summary>
        /// The <c>Connector</c> property is a notation defining the connector of the micro statements held.
        /// </summary>
        /// <value>
        /// The connector of the two brackets. Should be a valid notation.
        /// </value>
        public Notation? Connector { get => connector; set => connector = value; }

        /// <summary>
        /// The <c>Symbol</c> property is the symbol held by this bracket when serving as SymbolHolder.
        /// </summary>
        /// <value>
        /// The <c>Symbol</c> object held. When such, the bracket is just a wrapper for the symbol.
        /// </value>
        public Symbol? Symbol { get => symbol; set => symbol = value; }

        /// <summary>
        /// The <c>MicroStatement</c> property configures the MicroStatement held by the bracket.
        /// </summary>
        /// <value>
        /// The <c>MicroStatement</c> object held by the bracket when existing as a <c>StatementHolder</c>.
        /// </value>
        public MicroStatement? MicroStatement { get => microStatement; set => microStatement = value; }

        // Will cause stack overflow
        //Lines of code removed here.

        /// <summary>
        /// To dispose the class, implementing the <c>IDisposable</c> interface/
        /// </summary>
        /// <remarks>
        /// This disposal will also dispose the notation inside the symbol in a chain.
        /// </remarks>
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

        /// <summary>
        /// This override function checks if <c>obj</c> is same as <c>this</c> as brackets.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To be noted, this function does not perform a null check and the parameter object is nullable.
        /// </para>
        /// <para>
        /// This function calls <c>GetHashCode()</c> function to implement the capability.
        /// </para>
        /// </remarks>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>
        /// A boolean representing whether the two are equal.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        /// <summary>
        /// Generates a string to display the bracket, used in generating proof text.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is deprecated because its generation is too long and too complicated for humans to
        /// read, ignoring the algorithm's beauty and simplicity.
        /// </para>
        /// <para>
        /// Please use <c>Bracket.ToFriendlyString</c> for convertion, despite that this override method is
        /// the default case if you do not explicitly call its replacement.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A string illustrating the details of the bracket.
        /// </returns>
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

        /// <summary>
        /// Generates a friendlier string to display the bracket, used in generating proof text.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The string has various forms. To investigate deeper into the exact forms of the generation, you have
        /// to look into the code.
        /// </para>
        /// <para>
        /// As it is named as <c>ToFriendlyString()</c>, this method's output is much friendlier than the default 
        /// converter. It utilizes a big switch expression to convert and tries to avoid useless junk data to be
        /// written to the log console.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A string illustrating the details of the notation.
        /// </returns>
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

        /// <summary>
        /// Returns a hashable bracket in order to get its hash code for comparing.
        /// </summary>
        /// <remarks>
        /// Normally a bracket should not be hashed because it has a property <c>Satisfied</c> which varies during
        /// the process of seeking for proof. This method simply copies the current bracket's data, but keeping
        /// the value of <c>Satisfied</c> its default.
        /// </remarks>
        /// <returns>
        /// A bracket whose <c>Satisfied</c> property is kept to default.
        /// </returns>
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

        /// <summary>
        /// Returns a int hash code which is unique for each <c>Bracket</c> created.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The hash code is the HEX interpretation of a modification of the SHA3 (Secure Hash Algorithm 3)
        /// of the JSON serialization of the <c>this</c> Bracket.
        /// </para>
        /// <para>
        /// If any error occurs during the process described above, the hash code will be <c>-1</c>.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The hash code for the bracket.
        /// </returns>
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

    /// <summary>
    /// Defines an enumeration categoring the bracket into four holder modes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Explanations above each of the four modes are in the section for the Bracket class. Although sometimes, for
    /// example the BracketHolder mode is called mode one, actually it is not associated closely with the integer just
    /// like what did in the enumeration definition for Common.LogLevel.
    /// </para>
    /// <para>
    /// Although there is do NO protection, during its lifetime a bracket should NOT change its type.
    /// </para>
    /// </remarks>
    public enum BracketType
    {
        BracketHolder,      // mode 1
        NegatedHolder,      // mode 2
        StatementHolder,    // mode 3
        SymbolHolder        // mode 4
    };

    /// <summary>
    /// Defines whether a bracket is satisfied, with three possible values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This enumeration has three possibilities: <c>Unknown</c>, <c>False</c>, <c>True</c>, with <c>Unknown</c> default.
    /// </para>
    /// <para>
    /// I do not use <c>bool</c> type because it does not support the capability of <c>Unknown</c>.
    /// </para>
    /// </remarks>
    public enum Satisfaction
    {
        Unknown,
        False,
        True
    };
}
