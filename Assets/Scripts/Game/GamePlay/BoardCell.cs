using UnityEngine;

/// <summary>
/// 挂在每一个 Cell 的 GameObject 上（需要 Collider2D）
/// 负责把点击转发给 BoardView
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BoardCell : MonoBehaviour
{
    [SerializeField] private int index;         // 0..8
    [SerializeField] private BoardView boardView;

    private void Reset()
    {
        // 尝试自动抓取 BoardView（父级查找）
        if (!boardView) boardView = GetComponentInParent<BoardView>();
    }

    private void OnMouseDown()
    {
        if (!enabled || !gameObject.activeInHierarchy) return;
        // 左键点击（可选过滤）
        if (!Input.GetMouseButtonDown(0)) return;

        boardView?.OnCellClickedFromChild(index);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (index < 0 || index > 8)
            Debug.LogWarning($"BoardCell on {name} has invalid index {index}. Should be 0..8.");
        if (!boardView)
            boardView = GetComponentInParent<BoardView>();
    }
#endif
}
