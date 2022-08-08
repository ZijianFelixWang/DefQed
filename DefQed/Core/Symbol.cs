using System;

namespace DefQed.Core
{
    /// <summary>
    /// The <c>Symbol</c> class describes a <c>Symbol</c> object, as an instance of a <c>Notation</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each <c>Symbol</c> is an instance of a <c>Notation</c>. Eg, symbol "x" belongs to notation "item".
    /// </para>
    /// <para>
    /// Every symbol has its <c>id</c>, <c>name</c> and <c>notation</c>. Normally, a symbol is distinguished
    /// by its name while symbols with the same name do can coexist.
    /// </para>
    /// <para>
    /// The <c>Symbol</c> class implements <c>IDisposable</c> and should be disposed after use to save memory.
    /// </para>
    /// </remarks>
    public class Symbol : IDisposable
    {
        /// <summary>
        /// (field) This field stores the name of the symbol, intending to distinguish it.
        /// </summary>
        private string name = "";

        /// <summary>
        /// (field) This field stores the formal identifier, with default value -2.
        /// </summary>
        private int id = -2;

        /// <summary>
        /// (field) This field stores the notation of the symbol, with default value blank.
        /// </summary>
        private Notation notation = new();

        /// <summary>
        /// The <c>Name</c> property is used to identify the symbol in most cases.
        /// </summary>
        /// <value>
        /// To be used to identify a symbol easily, like finding a citizen with his or her name.
        /// </value>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// The <c>Id</c> property is used by the program to identify the symbol.
        /// </summary>
        /// <value>
        /// Each symbol has a unique id, to distinguish it more precisely.
        /// </value>
        public int Id { get => id; set => id = value; }

        /// <summary>
        /// The <c>Notation</c> property controls the "type" of the symbol.
        /// </summary>
        /// <value>
        /// The type of the symbol. Must be a valid <c>Notation</c>. Eg, the type of "123" is "Number".
        /// </value>
        public Notation Notation { get => notation; set => notation = value; }

        /// <summary>
        /// Generates a string to display the symbol, used in generating proof text.
        /// </summary>
        /// <remarks>
        /// The string has form: <c>(Notation/[Id]Name)</c>
        /// </remarks>
        /// <returns>
        /// A string illustrating the details of the symbol.
        /// </returns>
        public override string ToString()
        {
            return $"({Notation}/[{Id}]{Name.ToUpper()})";
        }

        /// <summary>
        /// Default constructor of the <c>Symbol</c> class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This constructor will name the symbol as <c>UntitledSymbol</c>. This may be an indicator for
        /// unutilized. However, although not much possible, user may still just name the symbol as
        /// <c>UntitledSymbol</c> or other strange stuff.
        /// </para>
        /// <para>
        /// This constructor also makes deserialization for it easier.
        /// </para>
        /// </remarks>
        public Symbol()
        {
            // for Deserialization
            Id = default;
            Name = "UntitledSymbol";
            Notation = new();
        }

        /// <summary>
        /// This is the standard constructor of the symbol.
        /// </summary>
        /// <remarks>
        /// Normally, a symbol is constructed using this constructor, although it is not essential.
        /// </remarks>
        /// <param name="id">Identifier for this symbol.</param>
        /// <param name="notation">The notation for this symbol to instance.</param>
        /// <param name="name">(optional) Name of this symbol, with default value blank.</param>
        public Symbol(int id, Notation notation, string name = "")
        {
            Id = id;
            Notation = notation;
            Name = name;
        }

        /// <summary>
        /// This is the constructor for symbols like numbers and decimals.
        /// </summary>
        /// <remarks>
        /// For example, the symbol name for 12.74 is "12.74". It should be noted that irrational constants
        /// such as Pi or E are not supported by this, because this feature is purely numerical. To be more
        /// specific, Pi should be named as "Pi" exactly.
        /// </remarks>
        /// <param name="notation">The notation for this symbol to instance.</param>
        /// <param name="dVal">The numerical value for this symbol.</param>
        public Symbol(Notation notation, double dVal)
        {
            Id = -1;
            Name = Convert.ToString(dVal);
            Notation = notation;
        }

        /// <summary>
        /// To dispose the class, implementing the <c>IDisposable</c> interface/
        /// </summary>
        /// <remarks>
        /// This disposal will also dispose the notation inside the symbol in a chain.
        /// </remarks>
        public void Dispose()
        {
            Notation.Dispose();
            GC.SuppressFinalize(this);  // CA1816 quality rule
        }
    }
}
