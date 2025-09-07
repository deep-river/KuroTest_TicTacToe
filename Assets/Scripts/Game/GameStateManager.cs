// GameStateManager.cs
using System;
using System.Collections;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // —— 事件：UI/HUD/Recorder 订阅 —— //
    public event Action<int> OnRoundStarted;
    public event Action<Turn> OnTurnChanged;
    public event Action<int, Mark> OnMoveCommitted;
    public event Action<int> OnStepChanged;
    public event Action<GameResult, int[]> OnGameOver; // 附带胜利线（可空）

    // —— 依赖 —— //
    [Header("References")]
    [SerializeField] private DifficultyManager difficultyManager;
    [SerializeField] private BoardView boardView; // 你的渲染/点击组件（另行实现）

    // —— 状态 —— //
    private enum State { Idle, Ready, PlayerTurn, AgentTurn, Paused, GameOver }
    private State state = State.Idle;

    private Board board = new Board();
    private int roundIndex = 1;
    private int stepCount = 0;

    private Mark humanMark = Mark.X;
    private Mark aiMark => (humanMark == Mark.X) ? Mark.O : Mark.X;

    private Coroutine aiRoutine;

    // 供 Pause 菜单切换的提示开关
    public bool ShowHint { get; private set; }

    // —— 生命周期 —— //
    private void Awake()
    {
        // 你可以在这里订阅 BoardView 的点击事件，或让 BoardView 直接调用 TryPlayerMove
        if (boardView != null) boardView.Bind(this);
    }

    private void Start()
    {
        // 按需从 GameLaunchService 获取启动参数并设置 human/ai 先手
        // 这里先给一个默认：如果 AIStarts，玩家用 O，AI 用 X
        if (difficultyManager && difficultyManager.AIStarts) humanMark = Mark.O;

        StartNewRound();
    }

    // —— 外部控制 —— //
    public void StartNewRound()
    {
        state = State.Ready;
        stepCount = 0;
        board.Reset();
        boardView?.ResetView();

        OnRoundStarted?.Invoke(roundIndex);
        OnStepChanged?.Invoke(stepCount);

        // 决定先手
        if (humanMark == Mark.X) EnterPlayerTurn();
        else EnterAgentTurn();
    }

    public void SetShowHint(bool show)
    {
        ShowHint = show;
        // 这里可通知 BoardView 刷新推荐落点（留空）
    }

    public void Pause()
    {
        if (state == State.PlayerTurn || state == State.AgentTurn)
        {
            state = State.Paused;
            // 打开暂停菜单由按钮/UIManager负责，这里只改变状态
        }
    }

    public void Resume()
    {
        if (state != State.Paused) return;
        // 恢复到谁的回合：根据上一步判断
        // 简化：如果当前步数偶数 -> X 下一手，否则 O 下一手；结合先手计算回合
        var nextIsHuman = NextTurnIsHuman();
        if (nextIsHuman) EnterPlayerTurn(); else EnterAgentTurn();
    }

    public void EndMatchEarly()
    {
        // 外部“结束对局”请求（暂停菜单）
        if (state == State.GameOver) return;
        state = State.GameOver;
        OnGameOver?.Invoke(CalcFinalResultForEarlyEnd(), null);
    }

    // —— 输入（来自 BoardView） —— //
    public void TryPlayerMove(int index)
    {
        if (state != State.PlayerTurn) return;
        if (!board.IsCellEmpty(index)) return;

        CommitMove(index, humanMark);

        if (CheckEnd(humanMark)) return;

        EnterAgentTurn();
    }

    // —— 内部流程 —— //
    private void EnterPlayerTurn()
    {
        state = State.PlayerTurn;
        OnTurnChanged?.Invoke(Turn.PlayerTurn);
        // 可在此高亮可落子位/提示
    }

    private void EnterAgentTurn()
    {
        state = State.AgentTurn;
        OnTurnChanged?.Invoke(Turn.AgentTurn);

        if (aiRoutine != null) StopCoroutine(aiRoutine);
        aiRoutine = StartCoroutine(Co_AgentMove());
    }

    private IEnumerator Co_AgentMove()
    {
        yield return null; // 可加一点延时显得更自然：yield return new WaitForSeconds(0.1f);

        var agent = difficultyManager ? difficultyManager.GetAgent() : null;
        int move = agent?.ChooseMove(board, aiMark) ?? -1;

        if (state != State.AgentTurn) yield break;
        if (move < 0 || !board.IsCellEmpty(move))
        {
            // 兜底：随便找一个可下的位置
            foreach (var m in board.LegalMoves()) { move = m; break; }
        }

        CommitMove(move, aiMark);

        if (CheckEnd(aiMark)) yield break;

        EnterPlayerTurn();
    }

    private void CommitMove(int index, Mark mark)
    {
        board.ApplyMove(index, mark);
        stepCount++;
        OnMoveCommitted?.Invoke(index, mark);
        OnStepChanged?.Invoke(stepCount);
        boardView?.PlacePiece(index, mark);
    }

    private bool CheckEnd(Mark last)
    {
        if (Rules.IsWin(board, last))
        {
            state = State.GameOver;
            var line = Rules.GetWinningLine(board, last);
            var result = (last == humanMark) ? GameResult.HumanWin : GameResult.AIWin;
            OnGameOver?.Invoke(result, line);
            return true;
        }
        if (Rules.IsDraw(board))
        {
            state = State.GameOver;
            OnGameOver?.Invoke(GameResult.Draw, null);
            return true;
        }
        return false;
    }

    private bool NextTurnIsHuman()
    {
        // 依据先手与当前步数判断下一手属于谁
        // 先手为 X：偶数步轮到 X；若 human==X，偶数步是 Human
        bool nextIsX = (stepCount % 2 == 0);
        return (humanMark == Mark.X) ? nextIsX : !nextIsX;
    }

    private GameResult CalcFinalResultForEarlyEnd()
    {
        // 这里按需要定义：提前结束可算平局
        return GameResult.Draw;
    }
}
