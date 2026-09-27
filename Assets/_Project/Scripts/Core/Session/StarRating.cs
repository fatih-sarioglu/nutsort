namespace NutSort.Core
{
    public static class StarRating
    {
        public const int MaxStars = 3;

        // 2 stars if up to 50% more moves than min
        public static int Calculate(int moves, int minMoves)
        {
            if (minMoves <= 0 || moves <= minMoves)
                return 3;

            int twoStarLimit = minMoves + (minMoves + 1) / 2;
            return moves <= twoStarLimit ? 2 : 1;
        }
    }
}
