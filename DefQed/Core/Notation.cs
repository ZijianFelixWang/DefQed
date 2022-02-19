namespace DefQed.Core
{
    internal class Notation
    {
        public string Name = "";
        public int Id;
        public NotationOrigin Origin;

        public override string ToString() => $"Notation([{Id}]{Name.ToUpper()});";
    }

    internal enum NotationOrigin
    {
        Internal,   // eg, ==, >
        External    // eg, Point, Triangle
    };
}
