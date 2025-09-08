public class GameLaunchParams
{
    public GameMode mode;
    public AIDifficultyId difficulty; // Quick 用
    public int bestOf;                // Quick：3/5/7；Endless：0
    public bool aiStarts;             // 先手（可拓展）
}
