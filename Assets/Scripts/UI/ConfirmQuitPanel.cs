using UnityEngine;
using UnityEngine.UI;

public class ConfirmQuitPanel : UIPanelBase
{
    [Header("Buttons")]
    [SerializeField] private Button confirmButton;    // “确认”按钮
    [SerializeField] private Button cancelButton;     // “取消”按钮

    private void Awake()
    {
        if (confirmButton) confirmButton.onClick.AddListener(OnConfirm);
        if (cancelButton) cancelButton.onClick.AddListener(OnCancel);
    }

    private void OnDestroy()
    {
        if (confirmButton) confirmButton.onClick.RemoveListener(OnConfirm);
        if (cancelButton) cancelButton.onClick.RemoveListener(OnCancel);
    }

    private void OnConfirm()
    {
        // 退出应用（仅在打包后有效）
        Application.Quit();

        // 如果在编辑模式下，模拟退出（方便编辑器调试）
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnCancel()
    {
        Locator.UI?.CloseTop();
    }
}
