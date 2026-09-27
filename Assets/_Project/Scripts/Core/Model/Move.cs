namespace NutSort.Core
{
    public struct Move
    {
        public int From;
        public int To;

        public Move(int from, int to)
        {
            From = from;
            To = to;
        }

        public override string ToString() => $"{From} -> {To}";
    }
}
