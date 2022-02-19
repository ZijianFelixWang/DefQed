using System;

namespace DefQed.Core
{
    internal class Symbol
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
    }
}
