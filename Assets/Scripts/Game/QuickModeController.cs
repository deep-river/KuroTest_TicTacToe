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

        // —— 累计战果 —— //
        roundsPlayed++;
        switch (result)
        {
            case GameResult.HumanWin: winsP++; break;
            case GameResult.AIWin: winsA++; break;
            case GameResult.Draw: draws++; break;
        }

        // —— 计算积分 —— //
        float ptsP = winsP + draws * 0.5f;
        float ptsA = winsA + draws * 0.5f;
        float half = bestOf * 0.5f;   // 多数分阈值
        int r = bestOf - roundsPlayed; // 剩余局数

        // —— 提前终结判定（积分多数胜 或 数学锁死）—— //
        bool majorityReached = (ptsP > half) || (ptsA > half);
        bool mathematicallyOver =
            (ptsP + r < ptsA) ||   // 玩家最多加 r 分也追不上 AI
            (ptsA + r < ptsP);     // AI 最多加 r 分也追不上玩家

        if (majorityReached || mathematicallyOver || roundsPlayed >= bestOf)
        {
            FinishSeries(ptsP, ptsA);
        }
        // 否则不做事：让 RoundResultTipPanelController 等玩家点击后开启下一回合
    }

    private void FinishSeries(float ptsP, float ptsA)
    {
        seriesOver = true;

        // 系列赛结果（用于传给结算面板；面板也可自行从 Session 统计）
        var summary = new QuickSeriesSummary
        {
            bestOf = bestOf,
            winsPlayer = winsP,
            winsAI = winsA,
            draws = draws,
            pointsPlayer = ptsP,
            pointsAI = ptsA,
            seriesResult = ptsP > ptsA ? GameResult.HumanWin
                          : ptsA > ptsP ? GameResult.AIWin
                          : GameResult.Draw
        };

        // 打开结算面板；如你的 GameResultPanel 支持 args，可传递 summary
        Locator.UI?.Show("GameResultPanel", summary);
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
