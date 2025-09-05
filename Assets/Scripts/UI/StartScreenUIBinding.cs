using UnityEngine;
using UnityEngine.UI;

public class StartScreenUIBinding : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button aboutBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private Button playerBtn;

    private void Start()
    {
        if (playBtn) playBtn.onClick.AddListener(OnPlayClicked);
        if (settingsBtn) settingsBtn.onClick.AddListener(OnSettingsClicked);
        if (aboutBtn) aboutBtn.onClick.AddListener(OnAboutClicked);
        if (quitBtn) quitBtn.onClick.AddListener(OnQuitClicked);
        if (playerBtn) playerBtn.onClick.AddListener(OnPlayerClicked);
    }

    // —— 按钮回调 —— //
    private void OnPlayClicked()
    {
        // 打开“模式/难度选择”的面板
        Locator.UI?.Show("ModeSelectPanel");
    }

    private void OnSettingsClicked()
    {
        Locator.UI?.Show("SettingsPanel");
    }

    private void OnAboutClicked()
    {
        Locator.UI?.Show("GameInfoPanel");
    }

    private void OnQuitClicked()
    {
        Locator.UI?.Show("ConfirmQuitPanel");
    }

    private void OnPlayerClicked()
    {
        Locator.UI?.Show("PlayerInfoPanel");
    }
}
