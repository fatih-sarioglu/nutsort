namespace NutSort.Core
{
    // used by both the game and the solver
    public static class MoveRules
    {
        public static bool CanMove(BoardState state, Move move)
        {
            if (move.From < 0 || move.From >= state.BoltCount || move.To < 0 || move.To >= state.BoltCount)
                return false;
            if (move.From == move.To)
                return false;
            if (state.IsEmpty(move.From))
                return false;
            // completed bolts are locked
            if (state.IsCompleted(move.From))
                return false;
            if (state.IsFull(move.To))
                return false;

            // target has to be empty or same color
            return state.IsEmpty(move.To) || state.GetTopColor(move.From) == state.GetTopColor(move.To);
        }
    }
}
