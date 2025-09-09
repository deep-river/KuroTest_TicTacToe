// EndlessModeController.cs
using UnityEngine;

public class EndlessModeController : MonoBehaviour
{
    [SerializeField] private GameStateManager game;
    [SerializeField] private DifficultyManager diff;

    [Header("Streak thresholds")]
    [SerializeField] private int promoteStreak = 2; // 连胜N升
    [SerializeField] private int demoteStreak = 2; // 连败N降

    private int streakWin;
    private int streakLose;

    // 细颗粒阶梯（示例：7 级，从更弱到完美）
    private LadderLevel[] ladder =
    {
        new LadderLevel(LadderTier.Easy,   0.50f),
        new LadderLevel(LadderTier.Easy,   0.35f),
        new LadderLevel(LadderTier.Normal, 0.20f),
        new LadderLevel(LadderTier.Normal, 0.10f),
        new LadderLevel(LadderTier.Normal, 0.05f),
        new LadderLevel(LadderTier.Hard,   0.02f),
        new LadderLevel(LadderTier.Hard,   0.00f),
    };
    private int idx; // 当前阶梯索引

    private void Awake()
    {
        if (!game) game = FindObjectOfType<GameStateManager>();
        if (!diff) diff = FindObjectOfType<DifficultyManager>();
    }

    private void OnEnable()
    {
        if (!diff || diff.Mode != GameMode.Endless) { enabled = false; return; }

        // 以 DifficultyManager 的当前显示档作为起点（默认 Easy）
        idx = StartIndexFromCurrentTier(diff.GetDisplayName());
        ApplyCurrentLevel();

        game.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        if (game) game.OnGameOver -= OnGameOver;
    }

    private void OnGameOver(GameResult result, int[] _)
    {
        switch (result)
        {
            case GameResult.HumanWin:
                streakWin++; streakLose = 0;
                if (streakWin >= promoteStreak) { idx = Mathf.Min(idx + 1, ladder.Length - 1); streakWin = 0; ApplyCurrentLevel(); }
                break;
            case GameResult.AIWin:
                streakLose++; streakWin = 0;
                if (streakLose >= demoteStreak) { idx = Mathf.Max(idx - 1, 0); streakLose = 0; ApplyCurrentLevel(); }
                break;
            case GameResult.Draw:
                // 平局：不变（或按需清零 streak）
                break;
        }
        // 不弹结算，让 RoundResultTipPanelController 点击后继续下一局
    }

    private void ApplyCurrentLevel()
    {
        var lv = ladder[idx];
        diff.ApplyLadderLevel(lv); // 内部会触发 OnDifficultyChanged → HUD 难度文本刷新
    }

    private static int StartIndexFromCurrentTier(string display)
    {
        // 避免强耦合，这里粗略映射
        if (display != null)
        {
            if (display.ToLower().Contains("hard")) return 5;
            if (display.ToLower().Contains("Medium")) return 3;
        }
        return 1; // Easy 中段作为默认起点
    }

    // —— 数据结构 —— //
    public enum LadderTier { Easy, Normal, Hard }

    [System.Serializable]
    public struct LadderLevel
    {
        public LadderTier tier;
        public float mistakeRate; // 0..1

        public LadderLevel(LadderTier t, float rate) { tier = t; mistakeRate = Mathf.Clamp01(rate); }
    }
}
