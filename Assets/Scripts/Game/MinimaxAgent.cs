// MinimaxAgent.cs
using System;
using System.Collections.Generic;

public class MinimaxAgent : IAgent
{
    private readonly AIDifficulty cfg;
    private readonly Random rng = new Random();

    public MinimaxAgent(AIDifficulty config) { cfg = config; }

    public int ChooseMove(Board board, Mark myMark)
    {
        var legal = new List<int>();
        for (int i = 0; i < 9; i++) if (board[i] == Mark.Empty) legal.Add(i);
        if (legal.Count == 0) return -1;

        // 评估每个落点
        var scores = new List<(int idx, int score)>();
        foreach (var idx in legal)
        {
            var score = EvaluateAfterMove(board, idx, myMark, cfg.depthLimit);
            scores.Add((idx, score));
        }

        // 找到最佳分
        int best = int.MinValue;
        foreach (var s in scores) if (s.score > best) best = s.score;

        // mistakeRate：有概率选非最优
        bool makeMistake = (cfg.mistakeRate > 0f) && (rng.NextDouble() < cfg.mistakeRate);
        if (makeMistake)
        {
            // 从“不是最佳”的里随机挑一个
            var sub = scores.FindAll(s => s.score < best);
            if (sub.Count > 0) return sub[rng.Next(sub.Count)].idx;
            // 如果没有（全等优），退化到最佳
        }

        // 在最佳集合中挑选
        var bestMoves = scores.FindAll(s => s.score == best);
        if (cfg.randomizeAmongBest && bestMoves.Count > 0)
            return bestMoves[rng.Next(bestMoves.Count)].idx;

        return bestMoves.Count > 0 ? bestMoves[0].idx : scores[0].idx;
    }

    // —— 迷你极大极小 —— //
    private int EvaluateAfterMove(Board board, int moveIndex, Mark myMark, int depthLimit)
    {
        // 复制 cells，便于递归回溯
        var cells = new Mark[9];
        for (int i = 0; i < 9; i++) cells[i] = board[i];

        cells[moveIndex] = myMark;
        return Minimax(cells, false, myMark, depthLimit - 1);
    }

    private int Minimax(Mark[] cells, bool isMyTurn, Mark myMark, int depthLeft)
    {
        // 终局判定
        if (IsWin(cells, myMark)) return 10;
        if (IsWin(cells, Opp(myMark))) return -10;
        if (IsFull(cells) || depthLeft == 0) return 0;

        int best = isMyTurn ? int.MinValue : int.MaxValue;
        Mark current = isMyTurn ? myMark : Opp(myMark);

        for (int i = 0; i < 9; i++)
        {
            if (cells[i] != Mark.Empty) continue;
            cells[i] = current;
            int sc = Minimax(cells, !isMyTurn, myMark, depthLeft - 1);
            cells[i] = Mark.Empty;

            if (isMyTurn) best = Math.Max(best, sc);
            else best = Math.Min(best, sc);
        }
        return best;
    }

    private static Mark Opp(Mark m) => (m == Mark.X) ? Mark.O : Mark.X;

    private static bool IsFull(Mark[] c)
    {
        for (int i = 0; i < 9; i++) if (c[i] == Mark.Empty) return false;
        return true;
    }

    private static readonly int[][] Wins =
    {
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
