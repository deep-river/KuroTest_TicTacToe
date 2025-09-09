using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugDifficultyPanel : UIPanelBase
{
    [Header("UI")]
    [SerializeField] private TMP_InputField jsonInput; // 多行、等宽字体
    [SerializeField] private Button btnApply;
    [SerializeField] private Button btnCancel;
    [SerializeField] private TMP_Text errorLabel;      // 可选：显示校验错误

    private DifficultyManager diff;

    private void Awake()
    {
        diff = FindObjectOfType<DifficultyManager>();

        if (btnApply) btnApply.onClick.AddListener(OnApply);
        if (btnCancel) btnCancel.onClick.AddListener(() => Locator.UI?.CloseTop());
    }

    protected override void OnShow(object args = null)
    {
        // 进入时拉取当前会话内的 JSON 文本
        if (jsonInput)
            jsonInput.text = DifficultyTableResources.GetCurrentJson();

        if (errorLabel) errorLabel.text = string.Empty;
    }

    private void OnApply()
    {
        if (diff == null || jsonInput == null) return;

        if (diff.ApplyJsonAndReapply(jsonInput.text, out var err))
        {
            if (errorLabel) errorLabel.text = string.Empty;
            Locator.UI?.CloseTop(); // 成功则关闭
        }
        else
        {
            if (errorLabel) errorLabel.text = $"解析失败：{err}";
        }
    }
}
