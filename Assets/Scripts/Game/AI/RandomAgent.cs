// 一个极简随机 AI（仅用于打通流程）
using System;
using System.Linq;
public class RandomAgent : IAgent
{
    private readonly System.Random rng = new System.Random();
    public int ChooseMove(Board board, Mark myMark)
    {
        var legal = board.LegalMoves().ToArray();
        if (legal.Length == 0) return -1;
        return legal[rng.Next(legal.Length)];
    }
}