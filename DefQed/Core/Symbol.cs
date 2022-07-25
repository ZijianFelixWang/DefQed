using System;

namespace DefQed.Core
{
    public class Symbol : IDisposable
    {
        // Symbol -- the 'newed' notation
        // if 'ABC' as Name, okay but what if evaluation pops out '12'
        private string name;
        private int id;
        private Notation notation;

        public string Name { get => name; set => name = value; }
        public int Id { get => id; set => id = value; }
        public Notation Notation { get => notation; set => notation = value; }

        public override string ToString()
        {
            return $"({Notation}/[{Id}]{Name.ToUpper()})";
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
        }

        public void Dispose()
        {
            Notation.Dispose();
            GC.SuppressFinalize(this);  // CA1816 quality rule
        }
    }
}
