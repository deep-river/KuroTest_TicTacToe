using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("Default (when no launch params)")]
    [SerializeField] private bool aiStarts = false;
    [SerializeField] private AIDifficultyId defaultQuickDifficulty = AIDifficultyId.Normal;

    // 运行期
    public GameMode Mode { get; private set; } = GameMode.Quick;
    public bool AIStarts => aiStarts;

    private IAgent currentAgent;
    private AIDifficulty currentCfg;

    private void Awake()
    {
        // 读取启动参数（若从 StartScreen 来）
        if (GameLaunchService.TryConsume(out var p) && p != null)
        {
            Mode = p.mode;
            aiStarts = p.aiStarts;

            if (Mode == GameMode.Quick)
            {
                ApplyDifficulty(p.difficulty);
            }
            else // Endless 起点
            {
                ApplyDifficulty(AIDifficultyId.Easy);
            }
        }
        else
        {
            // 未通过面板进来的 fallback
            Mode = GameMode.Quick;
            ApplyDifficulty(defaultQuickDifficulty);
        }
    }

    public IAgent GetAgent() => currentAgent;

    public string GetDisplayName() => currentCfg.displayName;

    public void ApplyDifficulty(AIDifficultyId id)
    {
        currentCfg = BuildPreset(id);
        currentAgent = new MinimaxAgent(currentCfg);
    }

    private static AIDifficulty BuildPreset(AIDifficultyId id)
    {
        switch (id)
        {
            case AIDifficultyId.Easy:
                return new AIDifficulty
                {
                    id = id,
                    displayName = "Easy",
                    depthLimit = 9,
                    mistakeRate = 0.35f,
                    randomizeAmongBest = true
                };
            case AIDifficultyId.Normal:
                return new AIDifficulty
                {
                    id = id,
                    displayName = "Normal",
                    depthLimit = 9,
                    mistakeRate = 0.10f,
                    randomizeAmongBest = true
                };
            default: // Hard
                return new AIDifficulty
                {
                    id = id,
                    displayName = "Hard",
                    depthLimit = 9,
                    mistakeRate = 0.0f,
                    randomizeAmongBest = false
                };
        }
    }
}
