using UnityEngine;
using UnityEngine.UI;

public abstract class UIPanelBase : MonoBehaviour, IUIPanel
{
    [SerializeField] string panelId;
    [SerializeField] bool isModal;
    [Header("Optional")]
    [SerializeField] CanvasGroup canvasGroup;     // ���������뽻��
    [SerializeField] GameObject focusBlocker;     // ��͸�����֣�Modal��

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
    public virtual void OnFocus(bool focused) { /* ��ѡ������/��ͣ���� */ }

    protected virtual void OnShow(object args) { }
    protected virtual void OnHide() { }
}