public struct AIDifficulty
{
    public AIDifficultyId id;
    public string displayName;
    public int depthLimit;            // 对井字棋意义不大，但可用于“降级”
    public float mistakeRate;         // 0..1，选非最优的概率
    public bool randomizeAmongBest;   // 在等优解中随机
}
