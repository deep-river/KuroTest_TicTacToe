public interface IUIPanel
{
    string PanelId { get; }         // 唯一ID：如 "StartScreen"
    bool IsModal { get; }           // 是否遮挡下层并拦截点击
    void Show(object args = null);  // 入场（可带参数）
    void Hide();                    // 退场
    void OnFocus(bool focused);     // 顶层/失焦通知
}