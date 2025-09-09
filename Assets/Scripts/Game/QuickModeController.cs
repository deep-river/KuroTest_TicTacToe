using UnityEngine;

public class QuickPointsController : MonoBehaviour
{
    [SerializeField] private GameStateManager game;
    [SerializeField] private DifficultyManager diff;

    private int bestOf;          // 总局数 N
    private int roundsPlayed;    // 已完成局数（含和局）
    private int winsP, winsA;    // 玩家/AI 胜场
    private int draws;           // 和局数
    private bool seriesOver;

    private void Awake()
    {
        if (!game) game = FindObjectOfType<GameStateManager>();
        if (!diff) diff = FindObjectOfType<DifficultyManager>();
    }

    private void OnEnable()
    {
        if (!diff || diff.Mode != GameMode.Quick)
        {
            enabled = false; // 仅 Quick 模式启用
            return;
        }

        bestOf = Mathf.Max(3, diff.BestOf > 0 ? diff.BestOf : 3);
        roundsPlayed = winsP = winsA = draws = 0;
        seriesOver = false;

        if (game) game.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        if (game) game.OnGameOver -= OnGameOver;
    }

    private void OnGameOver(GameResult result, int[] _)
    {
        if (seriesOver) return;

        roundsPlayed++;
        switch (result)
        {
            case GameResult.HumanWin: winsP++; break;
            case GameResult.AIWin: winsA++; break;
            case GameResult.Draw: draws++; break;
        }

        // 胜场多数胜
        int halfWins = (bestOf / 2) + 1;          // 多数胜所需胜场
        int r = bestOf - roundsPlayed;            // 剩余局数

        bool majorityByWins = (winsP >= halfWins) || (winsA >= halfWins);
        bool mathematicallyOver =
            (winsP > winsA + r) ||                // 即便剩余全胜，AI 也追不上玩家胜场
            (winsA > winsP + r);                  // 反之亦然

        if (majorityByWins || mathematicallyOver || roundsPlayed >= bestOf)
        {
            FinishSeries();
        }
        // 否则不做事：交给 RoundResultTipPanelController 点击后自动 StartNewRound()
    }

    private void FinishSeries()
    {
        seriesOver = true;

        // 平局“积分”为0（可用于显示，不影响判定）
        float ptsP = winsP; // draw=0
        float ptsA = winsA;

        // 最终判定按“有效对局（胜+负）”的胜场比较，忽略平局
        GameResult seriesByWins =
            (winsP > winsA) ? GameResult.HumanWin :
            (winsA > winsP) ? GameResult.AIWin :
                              GameResult.Draw;

        var summary = new QuickSeriesSummary
        {
            bestOf = bestOf,
            winsPlayer = winsP,
            winsAI = winsA,
            draws = draws,
            pointsPlayer = ptsP,   // 仅展示用途：平局=0
            pointsAI = ptsA,
            seriesResult = seriesByWins
        };

        // 打开结算面板；若面板支持 args，可展示更详细信息
        Locator.UI?.Show(PanelIds.GameResultPanel, summary);
    }

    // —— 可选：传给 GameResultPanel 的结构（面板若不读 args 也不影响显示）—— //
    public class QuickSeriesSummary
    {
        public int bestOf;
        public int winsPlayer, winsAI, draws;
        public float pointsPlayer, pointsAI;
        public GameResult seriesResult;
    }
}
