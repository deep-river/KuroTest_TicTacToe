using UnityEngine;
using System;

public class DifficultyManager : MonoBehaviour
{
    [Header("Default (when no launch params)")]
    [SerializeField] private bool aiStarts = false;
    [SerializeField] private AIDifficultyId defaultQuickDifficulty = AIDifficultyId.Normal;

    public GameMode Mode { get; private set; } = GameMode.Quick;
    public bool AIStarts => aiStarts;
    public int BestOf { get; private set; } = 3;

    public event Action<string> OnDifficultyChanged;

    private IAgent currentAgent;
    private AIDifficulty currentCfg;

    private void Awake()
    {
        // 读取 Resources 表
        DifficultyTableResources.ReloadFromResources();

        // 启动参数
        if (GameLaunchService.TryConsume(out var p) && p != null)
        {
            Mode = p.mode;
            aiStarts = p.aiStarts;
            BestOf = (Mode == GameMode.Quick && p.bestOf > 0) ? p.bestOf : 3;

            if (Mode == GameMode.Quick) ApplyDifficulty(p.difficulty);
            else ApplyDifficulty(AIDifficultyId.Easy);
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
        var db = DifficultyTableResources.Get();
        if (!db.TryGetValue(id, out var cfg))
        {
            Debug.LogWarning($"[DifficultyManager] Difficulty '{id}' not found, fallback to default.");
            cfg = DifficultyTableResources.Get()[defaultQuickDifficulty];
        }
        currentCfg = cfg;
        currentAgent = new MinimaxAgent(currentCfg);
        OnDifficultyChanged?.Invoke(currentCfg.displayName);
    }

    // Endless 阶梯沿用现有的 LadderLevel 逻辑（仍显示三档名）
    public void ApplyLadderLevel(EndlessModeController.LadderLevel lv)
    {
        var baseId = lv.tier switch
        {
            EndlessModeController.LadderTier.Easy => AIDifficultyId.Easy,
            EndlessModeController.LadderTier.Normal => AIDifficultyId.Normal,
            _ => AIDifficultyId.Hard
        };
        var baseCfg = DifficultyTableResources.Get()[baseId];

        currentCfg = new AIDifficulty
        {
            id = baseCfg.id,
            displayName = baseCfg.displayName,
            depthLimit = baseCfg.depthLimit,
            mistakeRate = Mathf.Clamp01(lv.mistakeRate),
            randomizeAmongBest = baseCfg.randomizeAmongBest
        };
        currentAgent = new MinimaxAgent(currentCfg);
        OnDifficultyChanged?.Invoke(currentCfg.displayName);
    }

    // —— 会话内重载：从 Resources 还原默认表 + 重新应用当前档 —— //
    public void ReapplyCurrentDifficulty()
    {
        DifficultyTableResources.ReloadFromResources();
        ApplyDifficulty(currentCfg.id); // 保持当前显示档位（Quick）
        // Endless 情况下，可选择在下一局生效，或由控制器再次调用 ApplyLadderLevel
    }

    // —— 会话内直接套用来自调试面板的 JSON —— //
    public bool ApplyJsonAndReapply(string json, out string error)
    {
        if (!DifficultyTableResources.TryApplyJsonAtRuntime(json, out error))
            return false;

        // 基于新的表重新实例化当前档的 agent
        ApplyDifficulty(currentCfg.id);
        return true;
    }
}
