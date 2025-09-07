public interface IAgent
{
    // AI 接口，后续可替换为 Minimax 实现
    /// 返回要落子的格子索引 0..8
    int ChooseMove(Board board, Mark myMark);
}