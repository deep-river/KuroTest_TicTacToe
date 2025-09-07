using System;
using System.Collections.Generic;
using System.Linq;

public class Board
{
    private readonly Mark[] cells = new Mark[9]; // 0..8

    // 定义索引器，允许通过 board[i] 的方式访问Board类的实例，并通过下标获取格子状态
    // TODO: 下标访问存在越界风险
    public Mark this[int i] => cells[i];

    public void Reset() { Array.Fill(cells, Mark.Empty); }
    public bool IsCellEmpty(int idx) => idx >= 0 && idx < 9 && cells[idx] == Mark.Empty;

    public IEnumerable<int> LegalMoves()
    {
        for (int i = 0; i < 9; i++) if (cells[i] == Mark.Empty) yield return i;
    }

    public void ApplyMove(int idx, Mark mark)
    {
        // TODO: throw error? 这里逻辑改为不执行可能更好，由玩家/AI决定不执行
        if (!IsCellEmpty(idx)) throw new InvalidOperationException($"Cell {idx} is not empty");
        cells[idx] = mark;
    }

    public bool IsFull() => cells.All(c => c != Mark.Empty);
}