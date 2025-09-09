using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStatPanelBinder : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameStateManager game;
    [SerializeField] private DifficultyManager difficulty;

    [Header("Texts (只填“数值部分”的 TMP_Text)")]
    [SerializeField] private TMP_Text roundValue;
    [SerializeField] private TMP_Text stepValue;
    [SerializeField] private TMP_Text scoreValue;      // 形如 "1 - 0"
    [SerializeField] private TMP_Text winRateValue;    // 形如 "50%"
    [SerializeField] private TMP_Text difficultyValue;

    [Header("Optional")]
    [SerializeField] private Button pauseButton;       // 顶部暂停按钮

    // 简单统计（当前会话）
    private int rounds;
    private int steps;
    private int playerWins;
    private int aiWins;
    private int draws;

    private void Awake()
    {
        if (!game) game = FindObjectOfType<GameStateManager>();
        if (!difficulty) difficulty = FindObjectOfType<DifficultyManager>();
        
        if (pauseButton) pauseButton.onClick.AddListener(OpenPauseMenu);
        ResetUI();
    }

    private void OnEnable()
    {
        if (!game) return;
        game.OnRoundStarted += HandleRoundStarted;
        game.OnStepChanged += HandleStepChanged;
        game.OnGameOver += HandleGameOver;
        difficulty.OnDifficultyChanged += (name) => SetText(difficultyValue, name);
    }

    private void OnDisable()
    {
        if (!game) return;
        game.OnRoundStarted -= HandleRoundStarted;
        game.OnStepChanged -= HandleStepChanged;
        game.OnGameOver -= HandleGameOver;
        difficulty.OnDifficultyChanged -= (name) => SetText(difficultyValue, name);

        if (pauseButton) pauseButton.onClick.RemoveListener(OpenPauseMenu);
    }

    private void HandleRoundStarted(int round)
    {
        rounds = round; // 若状态机传的是真实轮次，直接用；否则你也可以 rounds++ 自增
        steps = 0;
        SetText(roundValue, rounds.ToString());
        SetText(stepValue, "0");
        SetText(difficultyValue, difficulty ? difficulty.GetDisplayName() : "-");
    }

    private void HandleStepChanged(int step)
    {
        steps = step;
        SetText(stepValue, steps.ToString());
    }

    private void HandleGameOver(GameResult r, int[] _)
    {
        switch (r)
        {
            case GameResult.HumanWin: playerWins++; break;
            case GameResult.AIWin: aiWins++; break;
            case GameResult.Draw: draws++; break;
        }
        UpdateScoreAndWinRate();
    }

    private void UpdateScoreAndWinRate()
    {
        SetText(scoreValue, $"{playerWins} - {aiWins}");

        int dec = playerWins + aiWins; // 胜率不计平局
        float rate = (dec > 0) ? (playerWins * 100f / dec) : 0f;
        SetText(winRateValue, $"{rate:0}%");
    }

    private void OpenPauseMenu()
    {
        // 仅通知状态机“暂停”，具体显示面板交给场景其它入口或这里也可以直接打开
        game?.Pause();
        Locator.UI?.Show(PanelIds.PauseMenuPanel);
    }

    private void ResetUI()
    {
        SetText(roundValue, "0");
        SetText(stepValue, "0");
        SetText(scoreValue, "0 - 0");
        SetText(winRateValue, "0%");
        SetText(difficultyValue, difficulty ? difficulty.GetDisplayName() : "-");
    }

    private static void SetText(TMP_Text t, string v) { if (t) t.text = v; }
}
