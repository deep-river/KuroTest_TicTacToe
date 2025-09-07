public static class Rules
{
    public static readonly int[][] Wins =
    {
        new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
        new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
        new[]{0,4,8}, new[]{2,4,6}
    };

    public static bool IsWin(Board b, Mark m)
    {
        foreach (var w in Wins)
            if (b[w[0]] == m && b[w[1]] == m && b[w[2]] == m) return true;
        return false;
    }

    public static int[] GetWinningLine(Board b, Mark m)
    {
        foreach (var w in Wins)
            if (b[w[0]] == m && b[w[1]] == m && b[w[2]] == m) return w;
        return null;
    }

    public static bool IsDraw(Board b)
    {
        return !IsWin(b, Mark.X) && !IsWin(b, Mark.O) && b.IsFull();
    }
}