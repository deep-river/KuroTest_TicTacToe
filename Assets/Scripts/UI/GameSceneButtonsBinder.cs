using UnityEngine;
using UnityEngine.UI;

public class GameSceneButtonsBinder : MonoBehaviour
{
    [SerializeField] private GameStateManager game;
    [Header("Buttons")]
    [SerializeField] private Button btnPause;
    [SerializeField] private Button btnDebug;

    [Header("Debug Visibility")]
    [Tooltip("是否在发行构建中也展示调试按钮。通常关闭。")]
    [SerializeField] private bool showDebugInRelease = false;

    private void Awake()
    {
        if (!game) game = FindObjectOfType<GameStateManager>();

        if (btnPause) btnPause.onClick.AddListener(OnPauseClicked);
        if (btnDebug) btnDebug.onClick.AddListener(OnDebugClicked);

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        if (!showDebugInRelease && btnDebug)
            btnDebug.gameObject.SetActive(false);
#endif
    }

    private void OnDestroy()
    {
        if (btnPause) btnPause.onClick.RemoveListener(OnPauseClicked);
        if (btnDebug) btnDebug.onClick.RemoveListener(OnDebugClicked);
    }

    private void OnPauseClicked()
    {
        game?.Pause();
        Locator.UI?.Show(PanelIds.PauseMenuPanel);
    }

    private void OnDebugClicked()
    {
        // 打开你做好的调试面板（例如前面实现的 DebugDifficultyPanel）
        Locator.UI?.Show(PanelIds.DifficultyDebugPanel);
    }
}
