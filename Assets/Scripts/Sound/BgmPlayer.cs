using UnityEngine;
using System.Collections;

/// <summary>
/// 全局 BGM 播放器：
/// - 跨场景常驻（DontDestroyOnLoad）
/// - 单例防重（只保留首个）
/// - 循环播放同一首曲子
/// - WebGL/移动浏览器的自动播放限制：在首次用户交互时解锁播放
/// </summary>
[DisallowMultipleComponent]
public class BgmPlayer : MonoBehaviour
{
    private static BgmPlayer _instance;

    [Header("References")]
    [SerializeField] private AudioSource source;     // 挂在同物体上的 AudioSource
    [SerializeField] private AudioClip clip;         // 可在 Inspector 指定；若留空则使用 source.clip

    [Header("Behavior")]
    [SerializeField] private bool playOnStart = true;        // 进入游戏自动播放
    [SerializeField] private bool respectMasterVolume = true; // 读取 PlayerPrefs("MasterVolume")

    private void Awake()
    {
        // 单例防重
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 引用兜底
        if (!source) source = GetComponent<AudioSource>();
        if (!source) source = gameObject.AddComponent<AudioSource>();

        // 基础配置
        if (clip) source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;         // 统一用代码控制
        source.ignoreListenerPause = true;  // 避免 Listener 暂停影响（可按需关闭）

        // 可选：应用全局音量
        if (respectMasterVolume)
        {
            float v = PlayerPrefs.GetFloat("MasterVolume", 1f);
            AudioListener.volume = Mathf.Clamp01(v);
        }

        if (playOnStart) TryPlayOrDeferForAutoplayPolicy();
    }

    /// <summary>外部可调用，确保开始播放（若浏览器策略允许）。</summary>
    public void Play() => TryPlayOrDeferForAutoplayPolicy();

    /// <summary>外部可调用，停止播放（本项目你说不需要控制，可留空不用）。</summary>
    public void Stop()
    {
        if (source && source.isPlaying) source.Stop();
    }

    private void TryPlayOrDeferForAutoplayPolicy()
    {
        if (!source || !source.clip) return;

#if UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS
        // 部分平台/浏览器禁止无交互自动播放：在首次点击/按键后启动
        if (!source.isPlaying)
            StartCoroutine(Co_PlayAfterFirstUserGesture());
#else
        if (!source.isPlaying) source.Play();
#endif
    }

    private IEnumerator Co_PlayAfterFirstUserGesture()
    {
        // 如果已经在播就不等
        if (source.isPlaying) yield break;

        // 先尝试直接播一次（有些平台已允许）
        source.Play();
        if (source.isPlaying) yield break;

        // 等待一次用户手势
        while (!Input.GetMouseButtonDown(0) && Input.touchCount == 0 && !Input.anyKeyDown)
            yield return null;

        if (!source.isPlaying) source.Play();
    }
}
