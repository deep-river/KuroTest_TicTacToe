using UnityEngine;
using UnityEngine.UI;

public abstract class UIPanelBase : MonoBehaviour
{
    [SerializeField] string panelId { get; }    // Î¨Ò»ID
    [SerializeField] bool isModal { get; }      // ÊÇ·ñÕÚµ²ÏÂ²ã²¢À¹½Øµã»÷
    [Header("Optional")]
    [SerializeField] CanvasGroup canvasGroup;   // ¿ØÖÆÏÔÒþÓë½»»¥
    [SerializeField] GameObject focusBlocker;   // °ëÍ¸Ã÷ÕÚÕÖ£¨Modal£©

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

    // ¶¥²ã/Ê§½¹Í¨Öª
    public virtual void OnFocus(bool focused) { /* ¿ÉÑ¡£º¸ßÁÁ/ÔÝÍ£»¥¶¯ */ } 

    protected virtual void OnShow(object args = null) { }
    protected virtual void OnHide() { }
}