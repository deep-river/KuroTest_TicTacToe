using UnityEngine;
using System;

public class DifficultyManager : MonoBehaviour
{
    [Header("Default (when no launch params)")]
    [SerializeField] private bool aiStarts = false;
    [SerializeField] private AIDifficultyId defaultQuickDifficulty = AIDifficultyId.Normal;

    // —— 公开属性 —— //
    public GameMode Mode { get; private set; } = GameMode.Quick;
    public bool AIStarts => aiStarts;
    public int BestOf { get; private set; } = 3;  // ★ Quick 用

    // —— 事件：HUD 难度文本动态刷新 —— //
    public event Action<string> OnDifficultyChanged; // ★ 传 displayName

    private IAgent currentAgent;
    private AIDifficulty currentCfg;

    private void Awake()
    {
        if (GameLaunchService.TryConsume(out var p) && p != null)
        {
            Mode = p.mode;
            aiStarts = p.aiStarts;
            BestOf = (Mode == GameMode.Quick && p.bestOf > 0) ? p.bestOf : 3;

            if (Mode == GameMode.Quick) ApplyDifficulty(p.difficulty);
            else ApplyDifficulty(AIDifficultyId.Easy); // Endless 起点
        }
        else
        {
            Mode = GameMode.Quick;
            BestOf = 3;
            ApplyDifficulty(defaultQuickDifficulty);
        }
    }

    public IAgent GetAgent() => currentAgent;
    public string GetDisplayName() => currentCfg.displayName;

    public void ApplyDifficulty(AIDifficultyId id)
    {
        currentCfg = BuildPreset(id);
        currentAgent = new MinimaxAgent(currentCfg);
        OnDifficultyChanged?.Invoke(currentCfg.displayName); // ★ 通知 HUD
    }

    // ★ Endless 阶梯使用：更细粒度，但 UI 保持三档显示
    public void ApplyLadderLevel(EndlessModeController.LadderLevel lv)
    {
        var mappedId = lv.tier switch
        {
            EndlessModeController.LadderTier.Easy => AIDifficultyId.Easy,
            EndlessModeController.LadderTier.Normal => AIDifficultyId.Normal,
            _ => AIDifficultyId.Hard
        };

        // 在对应档位基础上覆写 mistakeRate（depth=9，randomizeAmongBest=true 更像“人”）
        currentCfg = new AIDifficulty
        {
            id = mappedId,
            displayName = mappedId switch { AIDifficultyId.Easy => "Easy", AIDifficultyId.Normal => "Medium", _ => "Hard" },
            depthLimit = 9,
            mistakeRate = Mathf.Clamp01(lv.mistakeRate),
            randomizeAmongBest = true
        };
        currentAgent = new MinimaxAgent(currentCfg);
        OnDifficultyChanged?.Invoke(currentCfg.displayName); // ★
    }

    private static AIDifficulty BuildPreset(AIDifficultyId id)
    {
        switch (id)
        {
            case AIDifficultyId.Easy:
                return new AIDifficulty { id = id, displayName = "Easy", depthLimit = 5, mistakeRate = 0.35f, randomizeAmongBest = true };
            case AIDifficultyId.Normal:
                return new AIDifficulty { id = id, displayName = "Medium", depthLimit = 9, mistakeRate = 0.10f, randomizeAmongBest = true };
            default:
                return new AIDifficulty { id = id, displayName = "Hard", depthLimit = 9, mistakeRate = 0.00f, randomizeAmongBest = false };
        }
    }
}
