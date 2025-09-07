using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameResultPanelBinder : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameRecorder recorder;
    [SerializeField] private DifficultyManager difficulty; // 可显示难度名（也可从记录里读）

    [Header("UI")]
    [SerializeField] private TMP_Text resultValue;
    [SerializeField] private TMP_Text stepsValue;
    [SerializeField] private TMP_Text durationValue;
    [SerializeField] private TMP_Text difficultyValue;
    [SerializeField] private Button btnBackToMenu;
    [SerializeField] private Button btnPlayAgain;

    [Header("Scene")]
    [SerializeField] private string startSceneName = "StartScreen";
    [SerializeField] private string gameSceneName = "Game"; // 再来一局时使用

    private void Awake()
    {
        if (!recorder) recorder = FindObjectOfType<GameRecorder>();
        if (!difficulty) difficulty = FindObjectOfType<DifficultyManager>();

        if (btnBackToMenu) btnBackToMenu.onClick.AddListener(BackToMenu);
        if (btnPlayAgain) btnPlayAgain.onClick.AddListener(RestartGame);
    }

    private void OnEnable()
    {
        // 打开时刷新一次数据
        Refresh();
    }

    private void OnDestroy()
    {
        if (btnBackToMenu) btnBackToMenu.onClick.RemoveListener(BackToMenu);
        if (btnPlayAgain) btnPlayAgain.onClick.RemoveListener(RestartGame);
    }

    private void Refresh()
    {
        var m = recorder ? recorder.LastMatch : null;
        if (m == null)
        {
            SetText(resultValue, "-");
            SetText(stepsValue, "-");
            SetText(durationValue, "-");
            SetText(difficultyValue, difficulty ? difficulty.GetDisplayName() : "-");
            return;
        }

        SetText(resultValue, m.result); // 可在本地化时用 key 映射
        SetText(stepsValue, m.moves.Count.ToString());
        SetText(durationValue, $"{m.durationSec:0.0}s");
        SetText(difficultyValue, m.difficulty);
        // 若要展示 winningLine，可在这里高亮 BoardView（可选）
    }

    private void BackToMenu()
    {
        SceneManager.LoadScene(startSceneName);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private static void SetText(TMP_Text t, string v) { if (t) t.text = v; }
}
