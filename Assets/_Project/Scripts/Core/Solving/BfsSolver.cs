using System.Collections.Generic;

namespace NutSort.Core
{
    public enum SolverStatus
    {
        Solved,
        Unsolvable,
        NodeLimitReached
    }

    public class SolverResult
    {
        public SolverStatus Status;
        public List<Move> Path = new List<Move>();

        public bool IsSolved => Status == SolverStatus.Solved;
        public int MinMoves => Path.Count;
    }

    // bfs, so the first solution it finds is the shortest one
    // used for the hint button
    public static class BfsSolver
    {
        public const int DefaultNodeLimit = 500_000;

        public static SolverResult Solve(BoardState start, int nodeLimit = DefaultNodeLimit)
        {
            var result = new SolverResult();
            if (start.IsSolved())
            {
                result.Status = SolverStatus.Solved;
                return result;
            }

            // key is canonical so we dont visit the same board twice
            // value is parent + move to build the path at the end
            var cameFrom = new Dictionary<BoardState, (BoardState parent, Move move)>();
            cameFrom[start.ToCanonical()] = (null, default);

            var queue = new Queue<BoardState>();
            queue.Enqueue(start);
            int expanded = 0;

            while (queue.Count > 0)
            {
                if (expanded >= nodeLimit)
                {
                    result.Status = SolverStatus.NodeLimitReached;
                    return result;
                }

                var state = queue.Dequeue();
                expanded++;

                for (int from = 0; from < state.BoltCount; from++)
                {
                    for (int to = 0; to < state.BoltCount; to++)
                    {
                        var move = new Move(from, to);
                        if (!MoveRules.CanMove(state, move) || IsPointless(state, move))
                            continue;

                        var next = state.Apply(move);
                        var key = next.ToCanonical();
                        if (cameFrom.ContainsKey(key))
                            continue;

                        cameFrom.Add(key, (state, move));
                        if (next.IsSolved())
                        {
                            result.Status = SolverStatus.Solved;
                            result.Path = BuildPath(cameFrom, start, next);
                            return result;
                        }

                        queue.Enqueue(next);
                    }
                }
            }

            result.Status = SolverStatus.Unsolvable;
            return result;
        }

        // moving a single color bolt to an empty one is useless
        private static bool IsPointless(BoardState state, Move move) =>
            state.IsEmpty(move.To) && state.GetTopGroupSize(move.From) == state.GetCount(move.From);

        private static List<Move> BuildPath(Dictionary<BoardState, (BoardState parent, Move move)> cameFrom,
            BoardState start, BoardState goal)
        {
            var path = new List<Move>();
            var current = goal;
            while (!current.Equals(start))
            {
                var step = cameFrom[current.ToCanonical()];
                path.Add(step.move);
                current = step.parent;
            }

            path.Reverse();
            return path;
        }
    }
}
