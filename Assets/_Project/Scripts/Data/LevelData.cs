using NutSort.Core;

namespace NutSort.Data
{
    public class LevelData
    {
        public int Id { get; }
        // for stars
        public int MinMoves { get; }
        public BoardState Board { get; }

        public LevelData(int id, int minMoves, BoardState board)
        {
            Id = id;
            MinMoves = minMoves;
            Board = board;
        }
    }
}
