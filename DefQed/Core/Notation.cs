using System;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Digests;

namespace DefQed.Core
{
    internal class Notation : IDisposable
    {
        public string Name = "";
        public int Id;
        public NotationOrigin Origin;

        public override string ToString() => $"Notation([{Id}]{Name.ToUpper()});";

        public void Dispose()
        {
            Name = "";
            Id = -1;
            Origin = 0;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            //return base.GetHashCode();
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

        public static bool operator != (Notation? n1, Notation? n2) => !(n1 == n2);

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

    internal enum NotationOrigin
    {
        Internal,   // eg, ==, >
        External    // eg, Point, Triangle
    };
}
