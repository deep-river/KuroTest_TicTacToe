using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class SettingsPanel : UIPanelBase
{
    [Header("Buttons")]
    [SerializeField] private Button buttonZH;
    [SerializeField] private Button buttonEN;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Locale Codes (match your Locale assets)")]
    [SerializeField] private string zhCode = "zh-Hans"; // 或你项目里配置的 "zh" / "zh-CN"
    [SerializeField] private string enCode = "en";

    const string PP_LANG = "lang";
    const string PP_VOL = "masterVolume";

    // 快照 & 待应用
    string initialLang;
    float initialVolume;
    string pendingLang;
    float pendingVolume;

    void Awake()
    {
        if (buttonZH) buttonZH.onClick.AddListener(() => OnLangSelected(zhCode));
        if (buttonEN) buttonEN.onClick.AddListener(() => OnLangSelected(enCode));
        if (volumeSlider) volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        if (confirmButton) confirmButton.onClick.AddListener(OnConfirm);
        if (cancelButton) cancelButton.onClick.AddListener(OnCancel);
    }

    void OnDestroy()
    {
        if (buttonZH) buttonZH.onClick.RemoveAllListeners();
        if (buttonEN) buttonEN.onClick.RemoveAllListeners();
        if (volumeSlider) volumeSlider.onValueChanged.RemoveAllListeners();
        if (confirmButton) confirmButton.onClick.RemoveAllListeners();
        if (cancelButton) cancelButton.onClick.RemoveAllListeners();
    }

    protected override void OnShow(object args = null)
    {
        // 读取当前状态做快照
        initialLang = LocalizationSettings.SelectedLocale?.Identifier.Code ?? zhCode;
        initialVolume = PlayerPrefs.GetFloat(PP_VOL, AudioListener.volume);

        pendingLang = initialLang;
        pendingVolume = initialVolume;

        if (volumeSlider) volumeSlider.SetValueWithoutNotify(initialVolume);
        UpdateLangVisual(initialLang);
    }

    // —— 交互 —— //
    void OnLangSelected(string code)
    {
        pendingLang = code;
        SetLocale(code, persist: false);
        UpdateLangVisual(code);
    }

    void OnVolumeChanged(float v)
    {
        pendingVolume = Mathf.Clamp01(v);
        AudioListener.volume = pendingVolume; // 即时预览（不持久化）
    }

    void OnConfirm()
    {
        SetLocale(pendingLang, persist: true);
        PlayerPrefs.SetFloat(PP_VOL, pendingVolume);
        Locator.UI?.CloseTop();
    }

    void OnCancel()
    {
        // 恢复到打开前
        SetLocale(initialLang, persist: true);
        AudioListener.volume = initialVolume;
        PlayerPrefs.SetFloat(PP_VOL, initialVolume);
        Locator.UI?.CloseTop();
    }

    // —— 工具 —— //
    static void SetLocale(string code, bool persist)
    {
        var locale = LocalizationSettings.AvailableLocales
            .GetLocale(new LocaleIdentifier(code));
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale; // 全局生效，绑定的本地化组件会自动刷新
            if (persist) PlayerPrefs.SetString(PP_LANG, code);
        }
        else
        {
            Debug.LogWarning($"Locale '{code}' not found in AvailableLocales.");
        }
    }

    void UpdateLangVisual(string code)
    {
        if (buttonZH) buttonZH.interactable = code != zhCode;
        if (buttonEN) buttonEN.interactable = code != enCode;
    }
}
