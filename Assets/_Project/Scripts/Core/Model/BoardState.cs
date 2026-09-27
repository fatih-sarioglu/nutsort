using System;
using System.Text;

namespace NutSort.Core
{
    // each bolt is an array of color ids from bottom to top
    // never changes after creation, Apply returns a new state
    public class BoardState
    {
        private readonly byte[][] _bolts;
        private readonly int _hash;

        public int Capacity { get; }
        public int BoltCount => _bolts.Length;

        public BoardState(int capacity, byte[][] bolts)
        {
            Capacity = capacity;
            _bolts = bolts;
            _hash = ComputeHash();
        }

        public int GetCount(int bolt) => _bolts[bolt].Length;

        public int GetFreeSpace(int bolt) => Capacity - _bolts[bolt].Length;

        public bool IsEmpty(int bolt) => _bolts[bolt].Length == 0;

        public bool IsFull(int bolt) => _bolts[bolt].Length == Capacity;

        // index 0 is the bottom nut
        public byte GetColor(int bolt, int index) => _bolts[bolt][index];

        public byte GetTopColor(int bolt)
        {
            var nuts = _bolts[bolt];
            return nuts[nuts.Length - 1];
        }

        // how many same color nuts are on top
        public int GetTopGroupSize(int bolt)
        {
            var nuts = _bolts[bolt];
            if (nuts.Length == 0)
                return 0;

            byte top = nuts[nuts.Length - 1];
            int size = 1;
            for (int i = nuts.Length - 2; i >= 0 && nuts[i] == top; i--)
                size++;
            return size;
        }

        public bool IsCompleted(int bolt) => IsFull(bolt) && GetTopGroupSize(bolt) == Capacity;

        public bool IsSolved()
        {
            for (int i = 0; i < _bolts.Length; i++)
            {
                if (!IsEmpty(i) && !IsCompleted(i))
                    return false;
            }
            return true;
        }

        // top group or less if there is not enough space
        public int GetTransferCount(Move move) =>
            Math.Min(GetTopGroupSize(move.From), GetFreeSpace(move.To));

        // doesnt check rules, call MoveRules.CanMove first
        public BoardState Apply(Move move)
        {
            int count = GetTransferCount(move);
            var source = _bolts[move.From];
            var target = _bolts[move.To];

            var newSource = new byte[source.Length - count];
            Array.Copy(source, newSource, newSource.Length);

            var newTarget = new byte[target.Length + count];
            Array.Copy(target, newTarget, target.Length);
            Array.Copy(source, source.Length - count, newTarget, target.Length, count);

            // other bolts are the same so reuse their arrays
            var bolts = (byte[][])_bolts.Clone();
            bolts[move.From] = newSource;
            bolts[move.To] = newTarget;

            return new BoardState(Capacity, bolts);
        }

        // sorted copy, solver uses it to skip same boards with different bolt order
        public BoardState ToCanonical()
        {
            var sorted = (byte[][])_bolts.Clone();
            Array.Sort(sorted, CompareBolts);
            return new BoardState(Capacity, sorted);
        }

        private static int CompareBolts(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return a.Length.CompareTo(b.Length);
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return a[i].CompareTo(b[i]);
            }
            return 0;
        }

        public override bool Equals(object obj)
        {
            var other = obj as BoardState;
            if (other == null || other._hash != _hash || other.Capacity != Capacity || other.BoltCount != BoltCount)
                return false;

            for (int i = 0; i < _bolts.Length; i++)
            {
                var a = _bolts[i];
                var b = other._bolts[i];
                if (a.Length != b.Length)
                    return false;
                for (int j = 0; j < a.Length; j++)
                {
                    if (a[j] != b[j])
                        return false;
                }
            }
            return true;
        }

        // cached because solver calls this a lot
        public override int GetHashCode() => _hash;

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _bolts.Length; i++)
            {
                if (i > 0)
                    sb.Append(" | ");
                sb.Append('[').Append(string.Join(",", _bolts[i])).Append(']');
            }
            return sb.ToString();
        }

        private int ComputeHash()
        {
            unchecked
            {
                int hash = 17 * 31 + Capacity;
                foreach (var bolt in _bolts)
                {
                    hash = hash * 31 + bolt.Length;
                    foreach (var nut in bolt)
                        hash = hash * 31 + nut;
                }
                return hash;
            }
        }
    }
}
