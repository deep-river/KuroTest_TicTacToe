using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class RoundResultTipPanelController : MonoBehaviour
{
    [Header("Panel Refs (被动视图)")]
    [SerializeField] private GameObject panelRoot; // RoundResultTipPanel 节点（初始可隐藏）
    [SerializeField] private TMP_Text tipText;     // Text_RoundResultTip
    [SerializeField] private GameObject mask;      // 全屏遮罩（Raycast Target 勾上）

    [Header("Localization")]
    [SerializeField] private string tableName = "GameTextTable";
    [SerializeField] private string keyWin = "UI提示文本-获胜";
    [SerializeField] private string keyLose = "UI提示文本-失败";
    [SerializeField] private string keyDraw = "UI提示文本-和局";

    [Header("Behavior")]
    [SerializeField] private float minHoldSeconds = 1f;

    private GameStateManager game;
    private bool isShowing;
    private bool canDismiss;

    private void Awake()
    {
        game = FindObjectOfType<GameStateManager>();
        if (game != null) game.OnGameOver += OnRoundOver;

        // 保证初始隐藏（不影响本脚本激活与协程运行）
        SetPanelActive(false);
    }

    private void OnDestroy()
    {
        if (game != null) game.OnGameOver -= OnRoundOver;
    }

    private void Update()
    {
        if (!isShowing || !canDismiss) return;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 || Input.anyKeyDown)
        {
            Dismiss();
        }
    }

    private void OnRoundOver(GameResult result, int[] _)
    {
        // 防重复：如果已在展示中，忽略
        if (isShowing) return;
        StartCoroutine(Co_Show(result));
    }

    private IEnumerator Co_Show(GameResult result)
    {
        // 文本本地化
        string key = result switch
        {
            GameResult.HumanWin => keyWin,
            GameResult.AIWin => keyLose,
            _ => keyDraw
        };
        string localized = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
        if (tipText) tipText.text = localized;

        // 开启面板并锁定点击
        SetPanelActive(true);
        isShowing = true;
        canDismiss = false;

        // 强制展示一段时间
        yield return new WaitForSeconds(minHoldSeconds);
        canDismiss = true;
    }

    private void Dismiss()
    {
        isShowing = false;
        canDismiss = false;
        SetPanelActive(false);

        // 若终局结算面板已弹出，则不再开新回合
        var resultPanel = FindObjectOfType<GameResultPanel>(true);
        bool finalShowing = resultPanel != null && resultPanel.gameObject.activeInHierarchy;
        if (!finalShowing)
        {
            game?.StartNewRound();
        }
    }

    private void SetPanelActive(bool on)
    {
        if (panelRoot) panelRoot.SetActive(on);
        if (mask) mask.SetActive(on);
    }
}
