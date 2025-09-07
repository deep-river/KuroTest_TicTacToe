using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    // 最小占位，后续接入真正的难度/Minimax
    [SerializeField] private bool aiStarts = false; // 可由 GameLaunchParams 决定
    public bool AIStarts => aiStarts;

    private IAgent currentAgent;

    private void Awake()
    {
        // 先用随机AI占位；后续替换为 Minimax + 参数
        currentAgent = new RandomAgent();
    }

    public IAgent GetAgent() => currentAgent;
    public string GetDisplayName() => "Random"; // HUD 可显示；之后换成 AIDifficulty.displayName
}
