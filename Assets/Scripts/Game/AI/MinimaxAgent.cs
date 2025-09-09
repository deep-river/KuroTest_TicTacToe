using System;
using System.Collections.Generic;

public class MinimaxAgent : IAgent
{
    private readonly AIDifficulty cfg;
    private readonly Random rng = new Random();

    public MinimaxAgent(AIDifficulty config) { cfg = config; }

    // 中心 > 角 > 边，用很小的偏置来打破“平局同分”
    //   索引布局: 0 1 2
    //             3 4 5
    //             6 7 8
    private static readonly int[] PositionalBias = {
        2, 1, 2,
        1, 3, 1,
        2, 1, 2
    }; // center=3, corners=2, edges=1

    public int ChooseMove(Board board, Mark myMark)
    {
        var legal = new List<int>(9);
        for (int i = 0; i < 9; i++)
            if (board[i] == Mark.Empty) legal.Add(i);
        if (legal.Count == 0) return -1;

        // 评分：先按极大极小分数比较，再用“位置偏好”打破同分
        int bestKey = int.MinValue;
        var bestMoves = new List<int>();

        foreach (var idx in legal)
        {
            int baseScore = EvaluateAfterMove(board, idx, myMark, cfg.depthLimit);
            // 放大主评分，叠加很小的偏置，确保不会改变赢/输排序，只在同分时生效
            int key = baseScore * 100 + PositionalBias[idx];

            if (key > bestKey)
            {
                bestKey = key;
                bestMoves.Clear();
                bestMoves.Add(idx);
            }
            else if (key == bestKey)
            {
                bestMoves.Add(idx);
            }
        }

        // 软难度：有意犯错
        bool makeMistake = (cfg.mistakeRate > 0f) && (rng.NextDouble() < cfg.mistakeRate);
        if (makeMistake)
        {
            var sub = new List<int>();
            foreach (var m in legal)
                if (!bestMoves.Contains(m)) sub.Add(m);
            if (sub.Count > 0) return sub[rng.Next(sub.Count)];
        }

        if (bestMoves.Count == 0) return legal[0];
        if (cfg.randomizeAmongBest && bestMoves.Count > 1)
            return bestMoves[rng.Next(bestMoves.Count)];
        return bestMoves[0];
    }

    // —— 极大极小（带“快赢优先/慢输次之”） —— //

    private int EvaluateAfterMove(Board board, int moveIndex, Mark myMark, int depthLimit)
    {
        // 在本地副本上搜索，避免污染外部棋盘
        var cells = new Mark[9];
        for (int i = 0; i < 9; i++) cells[i] = board[i];

        // 当前根层我方先走，下一层轮到对手
        cells[moveIndex] = myMark;
        return Minimax(cells, isMyTurn: false, myMark: myMark, depthLeft: depthLimit - 1, ply: 1);
    }

    /// <summary>
    /// Win  = +10 - ply（越快赢越好）
    /// Lose = -10 + ply（越晚输越好）
    /// Draw = 0
    /// </summary>
    private int Minimax(Mark[] cells, bool isMyTurn, Mark myMark, int depthLeft, int ply)
    {
        if (IsWin(cells, myMark)) return 10 - ply;
        if (IsWin(cells, Opp(myMark))) return -10 + ply;
        if (IsFull(cells) || depthLeft == 0) return 0;

        int best = isMyTurn ? int.MinValue : int.MaxValue;
        Mark current = isMyTurn ? myMark : Opp(myMark);

        for (int i = 0; i < 9; i++)
        {
            if (cells[i] != Mark.Empty) continue;
            cells[i] = current;
            int sc = Minimax(cells, !isMyTurn, myMark, depthLeft - 1, ply + 1);
            cells[i] = Mark.Empty; // 撤销
            best = isMyTurn ? Math.Max(best, sc) : Math.Min(best, sc);
        }
        return best;
    }

    private static Mark Opp(Mark m) => (m == Mark.X ? Mark.O : Mark.X);

    private static bool IsFull(Mark[] c)
    {
        for (int i = 0; i < 9; i++) if (c[i] == Mark.Empty) return false;
        return true;
    }

    private static readonly int[][] Wins = {
        new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
        new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
        new[]{0,4,8}, new[]{2,4,6}
    };
    private static bool IsWin(Mark[] c, Mark m)
    {
        foreach (var w in Wins)
            if (c[w[0]] == m && c[w[1]] == m && c[w[2]] == m) return true;
        return false;
    }
}
