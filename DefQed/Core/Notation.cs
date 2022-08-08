using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Text;
using System.Text.Json;

namespace DefQed.Core
{
    /// <summary>
    /// The <c>Notaion</c> class describes a <c>Notation</c> object, for symbols to instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each notation has three properties: <c>Name</c>, <c>Id</c> and <c>Origin</c> and will be further
    /// described detailly below.
    /// </para>
    /// <para>
    /// The <c>Notation</c> class implements <c>IDisposable</c> and should be disposed after use to save memory.
    /// </para>
    /// </remarks>
    public class Notation : IDisposable
    {
        /// <summary>
        /// (field) This field stores the name for the notation, with default value blank.
        /// </summary>
        private string name = "";
        
        /// <summary>
        /// (field) This field stores the formal identifier.
        /// </summary>
        private int id;

        /// <summary>
        /// (field) This field stores the origin for the notation, indicating where it comes from.
        /// </summary>
        private NotationOrigin origin;

        /// <summary>
        /// The <c>Name</c> property is used to identify the notation in most cases.
        /// </summary>
        /// <value>
        /// To be used to identify a notation easily, like finding a citizen with his or her name.
        /// </value>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// The <c>Id</c> property is used by the program to identify the notation.
        /// </summary>
        /// <value>
        /// Each notation has a unique id, to distinguish it more precisely.
        /// </value>
        public int Id { get => id; set => id = value; }

        /// <summary>
        /// The <c>Origin</c> property shows where does the <c>Notation</c> comes from.
        /// </summary>
        /// <remarks>
        /// Most of the notations are <c>External</c>, eg, Triangle and Function. However, there are
        /// a few notations <c>Internal</c>, such as the foundamental == and Item ones.
        /// </remarks>
        /// <value>
        /// Describes whether the notation is <c>Internal</c> or <c>External</c>.
        /// </value>
        public NotationOrigin Origin { get => origin; set => origin = value; }

        /// <summary>
        /// Generates a string to display the notation, used in generating proof text.
        /// </summary>
        /// <remarks>
        /// The string has form: <c>([Id]Name)</c>
        /// </remarks>
        /// <returns>
        /// A string illustrating the details of the notation.
        /// </returns>
        public override string ToString() => $"([{Id}]{Name})";

        /// <summary>
        /// To dispose the class, implementing the <c>IDisposable</c> interface/
        /// </summary>
        /// <remarks>
        /// This disposal will also dispose the notation inside the symbol in a chain.
        /// </remarks>
        public void Dispose()
        {
            Name = "";
            Id = -1;
            Origin = 0;
            GC.SuppressFinalize(this);  // CA1816 quality rule
        }

        /// <summary>
        /// This override function checks if <c>obj</c> is same as <c>this</c> as notations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To begin with, the function performs a null check towards the <c>object</c>. That is, if
        /// the object to compare with is null itself, the output is always <c>false</c>, although the
        /// <c>this</c> may be newly created itself.
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
            if (obj == null)
            {
                return false;
            }
            return GetHashCode() == obj.GetHashCode();
        }

        /// <summary>
        /// Returns a int hash code which is unique for each <c>Notation</c> created.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The hash code is the HEX interpretation of a modification of the SHA3 (Secure Hash Algorithm 3)
        /// of the JSON serialization of the <c>this</c> Notation.
        /// </para>
        /// <para>
        /// If any error occurs during the process described above, the hash code will be <c>-1</c>.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The hash code for the notation.
        /// </returns>
        public override int GetHashCode()
        {
            JsonSerializerOptions op = new()
            {
                IncludeFields = true,
                MaxDepth = 1024
            };

            var sha3 = new Sha1Digest();

            byte[] input = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(this, op));
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

        /// <summary>
        /// This is an overload of operator <c>!=</c> used to compare two notations.
        /// </summary>
        /// <param name="n1">Left side of the compare operator.</param>
        /// <param name="n2">RIght side of the compare operator.</param>
        /// <returns>
        /// A boolean representing whether the two notations are the same.
        /// </returns>
        public static bool operator !=(Notation? n1, Notation? n2) => !(n1 == n2);

        /// <summary>
        /// This is an overload of operator <c>==</c> used to compare two notations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// First, the operator applies a null check to the two notations. That is, if one or two of the 
        /// notations compared is or are null, the result will be <c>false</c> even if both null.
        /// </para>
        /// <para>
        /// Thd Id and Name of the notations are takin into consideration. If at least of the two properties
        /// is same, the two notations are thought to be same.
        /// </para>
        /// </remarks>
        /// <param name="n1">Left side of the compare operator.</param>
        /// <param name="n2">RIght side of the compare operator.</param>
        /// <returns>
        /// A boolean representing whether the two notations are the same.
        /// </returns>
        public static bool operator ==(Notation? n1, Notation? n2)
        {
            // If the following code uses "==" then there'll be a stack overflow exception.
            if (n1 is null || n2 is null)
            {
                return false;
            }

            if ((n1.Id == n2.Id) || (n1.Name == n2.Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Defines and enumeration type describing where the notation comes from.
    /// </summary>
    /// <remarks>
    /// Most of the notations are <c>External</c>, eg, Triangle and Function. However, there are
    /// a few notations <c>Internal</c>, such as the foundamental == and Item ones.
    /// </remarks>
    public enum NotationOrigin
    {
        Internal,   // eg, ==, >
        External    // eg, Point, Triangle
    };
}