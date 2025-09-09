using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class GameResultPanel : UIPanelBase
{
    [Header("Wiring - Value Texts")]
    [SerializeField] private TMP_Text roundCountValue;   // Text_RoundCountValue
    [SerializeField] private TMP_Text stepCountValue;    // Text_StepCountValue（最后一局步数）
    [SerializeField] private TMP_Text scoreValue;        // Text_ScoreValue（形如 2 - 1）
    [SerializeField] private TMP_Text winrateValue;      // Text_WinrateValue（形如 67%）

    [Header("Result Text (Quick only)")]
    [SerializeField] private GameObject resultTextRoot;  // ResultText 节点（无尽模式隐藏）
    [SerializeField] private TMP_Text resultText;        // 实际显示文本

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;

    [Header("Scene Names")]
    [SerializeField] private string startSceneName = "StartScreen";

    [Header("Mode")]
    [SerializeField] private bool isEndlessMode = false; // 无尽模式时隐藏 ResultText

    [Header("Localization")]
    [SerializeField] private string tableName = "GameTextTable";
    [SerializeField] private string keyWin = "UI提示文本-获胜";
    [SerializeField] private string keyLose = "UI提示文本-失败";
    [SerializeField] private string keyDraw = "UI提示文本-和局";

    private GameRecorder recorder;
    public static event Action OnResultConfirmed; // 🔔结算确认事件

    private void Awake()
    {
        recorder = FindObjectOfType<GameRecorder>();
        if (confirmButton) confirmButton.onClick.AddListener(OnConfirm);
    }

    private void OnDestroy()
    {
        if (confirmButton) confirmButton.onClick.RemoveListener(OnConfirm);
    }

    protected override void OnShow(object args = null)
    {
        // 刷数据
        RefreshFields();
    }

    private void RefreshFields()
    {
        if (!recorder)
        {
            SetText(roundCountValue, "-");
            SetText(stepCountValue, "-");
            SetText(scoreValue, "-");
            SetText(winrateValue, "-");
            if (resultTextRoot) resultTextRoot.SetActive(!isEndlessMode);
            if (resultText) resultText.text = "-";
            return;
        }

        var session = recorder.Session;
        var last = recorder.LastMatch;

        // 回合数 = 本次会话内完成的对局数
        int rounds = session != null ? session.matches.Count : (last != null ? 1 : 0);
        SetText(roundCountValue, rounds.ToString());

        // 最后一局步数
        SetText(stepCountValue, (last != null ? last.moves.Count.ToString() : "0"));

        // 比分 / 胜率（不计平局）
        int p = session?.playerWins ?? 0;
        int a = session?.aiWins ?? 0;
        SetText(scoreValue, $"{p} - {a}");
        int dec = p + a;
        float rate = dec > 0 ? (p * 100f / dec) : 0f;
        SetText(winrateValue, $"{rate:0}%");

        // Quick 模式显示“获胜/失败/和局”，Endless 隐藏
        if (resultTextRoot) resultTextRoot.SetActive(!isEndlessMode);

        if (!isEndlessMode && resultText)
        {
            string key = (last != null) ? last.result switch
            {
                "Win" => keyWin,
                "Lose" => keyLose,
                _ => keyDraw
            } : keyDraw;

            string localized = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            resultText.text = localized;
        }
    }

    private void OnConfirm()
    {
        OnResultConfirmed?.Invoke();
        // 返回 StartScreen
        SceneManager.LoadScene(startSceneName);
    }

    private static void SetText(TMP_Text t, string v) { if (t) t.text = v; }
}
