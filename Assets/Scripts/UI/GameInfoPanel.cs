using UnityEngine;
using UnityEngine.UI;

public class GameInfoPanel : UIPanelBase
{
    [Header("Button reference")]
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(OnCloseClicked);
    }
    private void OnDestroy()
    {
        if (closeButton) closeButton.onClick.RemoveListener(OnCloseClicked);
    }
    private void OnCloseClicked()
    {
        Locator.UI?.CloseTop();
    }
}
