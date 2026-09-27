using System;
using System.Collections.Generic;

namespace NutSort.Core
{
    // game logic of a level, board + undo history
    // move: MoveApplied -> BoltCompleted -> LevelWon
    // undo: BoltUncompleted -> MoveUndone
    public class GameSession
    {
        private readonly BoardState _initial;
        private readonly Stack<(BoardState board, Move move)> _history = new Stack<(BoardState, Move)>();

        public BoardState Current { get; private set; }

        public int MoveCount => _history.Count;
        public bool IsWon => Current.IsSolved();
        public bool CanUndo => _history.Count > 0 && !IsWon;

        // move, nut count
        public event Action<Move, int> MoveApplied;
        public event Action<Move, int> MoveUndone;
        public event Action<int> BoltCompleted;
        public event Action<int> BoltUncompleted;
        public event Action LevelWon;
        public event Action<BoardState> Restarted;

        public GameSession(BoardState initial)
        {
            _initial = initial;
            Current = initial;
        }

        public bool TryMove(Move move)
        {
            if (!MoveRules.CanMove(Current, move))
                return false;

            var previous = Current;
            int count = previous.GetTransferCount(move);
            Current = previous.Apply(move);
            _history.Push((previous, move));

            MoveApplied?.Invoke(move, count);

            if (!previous.IsCompleted(move.To) && Current.IsCompleted(move.To))
                BoltCompleted?.Invoke(move.To);

            if (Current.IsSolved())
                LevelWon?.Invoke();

            return true;
        }

        public bool Undo()
        {
            if (!CanUndo)
                return false;

            var (previous, move) = _history.Pop();
            var undone = Current;
            Current = previous;

            if (undone.IsCompleted(move.From) && !Current.IsCompleted(move.From))
                BoltUncompleted?.Invoke(move.From);
            if (undone.IsCompleted(move.To) && !Current.IsCompleted(move.To))
                BoltUncompleted?.Invoke(move.To);

            MoveUndone?.Invoke(move, previous.GetTransferCount(move));
            return true;
        }

        public void Restart()
        {
            _history.Clear();
            Current = _initial;
            Restarted?.Invoke(Current);
        }
    }
}
