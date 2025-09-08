// ModeSelectPanel.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameMode { Quick, Endless }
public enum AIDifficultyId { Easy, Normal, Hard }

public class ModeSelectPanel : UIPanelBase
{
    [Header("UI - Mode Buttons")]
    [SerializeField] private Button btnQuick;
    [SerializeField] private Button btnEndless;

    [Header("UI - Mode Details (Quick only)")]
    [SerializeField] private GameObject modeDetailsSection;

    [Header("UI - Difficulty Buttons")]
    [SerializeField] private Button btnEasy;
    [SerializeField] private Button btnNormal;
    [SerializeField] private Button btnHard;

    [Header("UI - Rounds Buttons (Best-Of)")]
    [SerializeField] private Button btnBO3;
    [SerializeField] private Button btnBO5;
    [SerializeField] private Button btnBO7;

    [Header("UI - Bottom Buttons")]
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnCancel;

    [Header("Visual")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene"; // 启动游戏场景的名称

    // —— 状态 —— //
    private GameMode _mode;
    private AIDifficultyId _difficulty;
    private int _bestOf;

    // 组集合，方便统一处理
    private Button[] _modeGroup;
    private Button[] _diffGroup;
    private Button[] _boGroup;

    private void Awake()
    {
        // 组注册
        _modeGroup = new[] { btnQuick, btnEndless };
        _diffGroup = new[] { btnEasy, btnNormal, btnHard };
        _boGroup = new[] { btnBO3, btnBO5, btnBO7 };

        // 绑定回调
        if (btnQuick) btnQuick.onClick.AddListener(() => SelectMode(GameMode.Quick));
        if (btnEndless) btnEndless.onClick.AddListener(() => SelectMode(GameMode.Endless));

        if (btnEasy) btnEasy.onClick.AddListener(() => SelectDifficulty(AIDifficultyId.Easy));
        if (btnNormal) btnNormal.onClick.AddListener(() => SelectDifficulty(AIDifficultyId.Normal));
        if (btnHard) btnHard.onClick.AddListener(() => SelectDifficulty(AIDifficultyId.Hard));

        if (btnBO3) btnBO3.onClick.AddListener(() => SelectBestOf(3));
        if (btnBO5) btnBO5.onClick.AddListener(() => SelectBestOf(5));
        if (btnBO7) btnBO7.onClick.AddListener(() => SelectBestOf(7));

        if (btnConfirm) btnConfirm.onClick.AddListener(OnConfirm);
        if (btnCancel) btnCancel.onClick.AddListener(OnCancel);
    }

    protected override void OnShow(object args = null)
    {
        // 默认选项：快速 / 普通难度 / BO3
        SelectMode(GameMode.Quick);
        SelectDifficulty(AIDifficultyId.Normal);
        SelectBestOf(3);
    }

    protected override void OnHide()
    {
        //（可选）清理/复位
    }

    private void SelectMode(GameMode m)
    {
        _mode = m;
        SetGroupVisual(_modeGroup, m == GameMode.Quick ? btnQuick : btnEndless);

        // 快速 → 显示细节；无尽 → 隐藏细节
        if (modeDetailsSection) modeDetailsSection.SetActive(_mode == GameMode.Quick);
    }

    private void SelectDifficulty(AIDifficultyId id)
    {
        _difficulty = id;
        Button selected = id switch
        {
            AIDifficultyId.Easy => btnEasy,
            AIDifficultyId.Normal => btnNormal,
            _ => btnHard
        };
        SetGroupVisual(_diffGroup, selected);
    }

    private void SelectBestOf(int bo)
    {
        _bestOf = bo;
        Button selected = (bo == 3) ? btnBO3 : (bo == 5 ? btnBO5 : btnBO7);
        SetGroupVisual(_boGroup, selected);
    }

    private void SetGroupVisual(IList<Button> group, Button selected)
    {
        if (group == null) return;

        foreach (var b in group)
        {
            if (!b) continue;
            var img = b.image;
            if (img)
            {
                img.sprite = (b == selected) ? selectedSprite : normalSprite;
            }
            //（可选）交互态：选中按钮禁用点击更直观
            b.interactable = (b != selected);
        }
    }

    private void OnConfirm()
    {
        // 构造参数并启动游戏
        var p = new GameLaunchParams
        {
            mode = _mode,
            difficulty = (_mode == GameMode.Quick) ? _difficulty : AIDifficultyId.Easy, // 无尽起点：Easy
            bestOf = (_mode == GameMode.Quick) ? _bestOf : 0,
            aiStarts = false // 先手规则可后续扩展
        };

        GameLaunchService.Launch(p, gameSceneName);
    }

    private void OnCancel()
    {
        // 恢复默认并关闭
        SelectMode(GameMode.Quick);
        SelectDifficulty(AIDifficultyId.Normal);
        SelectBestOf(3);
        Locator.UI?.CloseTop();
    }
}
