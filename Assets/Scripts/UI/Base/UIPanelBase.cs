using UnityEngine;
using UnityEngine.UI;

public abstract class UIPanelBase : MonoBehaviour
{
    [SerializeField] string panelId { get; }    // 唯一ID
    [SerializeField] bool isModal { get; }      // 是否遮挡下层并拦截点击
    [Header("Optional")]
    [SerializeField] CanvasGroup canvasGroup;   // 控制显隐与交互
    [SerializeField] GameObject focusBlocker;   // 半透明遮罩（Modal）

    public string PanelId => panelId;
    public bool IsModal => isModal;

    public virtual void Show(object args = null)
    {
        gameObject.SetActive(true);
        if (canvasGroup) { canvasGroup.alpha = 1; canvasGroup.blocksRaycasts = true; canvasGroup.interactable = true; }
        if (focusBlocker) focusBlocker.SetActive(isModal);
        OnShow(args);
    }
    public virtual void Hide()
    {
        OnHide();
        if (canvasGroup) { canvasGroup.alpha = 0; canvasGroup.blocksRaycasts = false; canvasGroup.interactable = false; }
        gameObject.SetActive(false);
    }

    // 顶层/失焦通知
    public virtual void OnFocus(bool focused) { /* 可选：高亮/暂停互动 */ } 

    protected virtual void OnShow(object args = null) { }
    protected virtual void OnHide() { }
}