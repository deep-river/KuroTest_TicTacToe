using UnityEngine;
using UnityEngine.UI;

public class PauseMenuPanel : UIPanelBase
{
    [Header("Wiring")]
    [SerializeField] private Toggle showHintToggle;
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnEndMatch;
    [SerializeField] private string gameResultPanelId = "GameResultPanel"; // 结算面板 ID

    // 依赖：场景中的状态机。也可以手动在 Inspector 里拖引用
    [SerializeField] private GameStateManager game;

    private void Awake()
    {
        if (!game) game = FindObjectOfType<GameStateManager>();

        if (btnResume) btnResume.onClick.AddListener(OnResume);
        if (btnEndMatch) btnEndMatch.onClick.AddListener(OnEndMatch);
        if (showHintToggle) showHintToggle.onValueChanged.AddListener(OnHintToggle);
    }

    private void OnDestroy()
    {
        if (btnResume) btnResume.onClick.RemoveListener(OnResume);
        if (btnEndMatch) btnEndMatch.onClick.RemoveListener(OnEndMatch);
        if (showHintToggle) showHintToggle.onValueChanged.RemoveAllListeners();
    }

    protected override void OnShow(object args = null)
    {
        // 打开时把当前值同步给 UI（不触发回调）
        if (showHintToggle)
        {
            showHintToggle.SetIsOnWithoutNotify(game ? game.ShowHint : false);
        }
    }

    private void Update()
    {
        // 在暂停面板里按 Esc = 继续
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnResume();
        }
    }

    private void OnResume()
    {
        game?.Resume();
        Locator.UI?.CloseTop(); // 关闭当前面板
    }

    private void OnEndMatch()
    {
        game?.EndMatchEarly();     // 状态机进入结算状态
        Locator.UI?.CloseTop();    // 关闭当前面板
        Locator.UI?.Show(gameResultPanelId); // 打开结算面板（确保已在 UIManager 注册）
    }

    private void OnHintToggle(bool on)
    {
        game?.SetShowHint(on); // 实时生效
    }
}
