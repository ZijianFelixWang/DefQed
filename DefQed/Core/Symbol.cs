using System;

namespace DefQed.Core
{
    public class Symbol : IDisposable
    {
        // Symbol -- the 'newed' notation
        // if 'ABC' as Name, okay but what if evaluation pops out '12'
        public string Name;
        public int Id;
        public Notation Notation;
        public double? DirectValue = null;

        public override string ToString()
        {
            return $"Symbol(Notation = {Notation}, [{Id}]({Name.ToUpper()}), Value = {DirectValue});";
        }

        public Symbol()
        {
            // for Deserialization
            Id = default;
            Name = "UntitledSymbol";
            Notation = new();
        }

        public Symbol(int id, Notation notation, string name = "")
        {
            Id = id;
            Notation = notation;
            Name = name;
        }

        public Symbol(Notation notation, double dVal)
        {
            Id = -1;
            Name = Convert.ToString(dVal);
            Notation = notation;
            DirectValue = dVal;
        }

        public void Dispose()
        {
            Notation.Dispose();
            GC.SuppressFinalize(this);  // CA1816 quality rule
        }
    }
}
