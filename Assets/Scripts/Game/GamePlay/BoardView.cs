using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardView : MonoBehaviour
{
    [Header("Cells & Pieces")]
    [Tooltip("九宫格 0..8（推荐从左到右、从上到下）")]
    [SerializeField] private Transform[] cells = new Transform[9];

    [Tooltip("棋子实例的父节点")]
    [SerializeField] private Transform piecesRoot;

    [Tooltip("X 棋子的预制体")]
    [SerializeField] private GameObject pieceXPrefab;

    [Tooltip("O 棋子的预制体")]
    [SerializeField] private GameObject pieceOPrefab;

    [Header("Hint (可选)")]
    [SerializeField] private GameObject hintMarkerPrefab; // 可选：下一步提示图标
    private GameObject hintMarkerInstance;

    // 运行期引用
    private GameStateManager game;               // 由 GameStateManager.Bind(this) 设置
    private readonly Dictionary<int, GameObject> spawnedPieces = new(); // index -> piece

    /// <summary>由 GameStateManager 调用，建立绑定。</summary>
    public void Bind(GameStateManager state)
    {
        game = state;
    }

    /// <summary>清空棋盘上的棋子与提示。</summary>
    public void ResetView()
    {
        foreach (var kv in spawnedPieces)
            if (kv.Value) Destroy(kv.Value);
        spawnedPieces.Clear();

        ClearHint();
    }

    /// <summary>在 index 处放置 X/O 棋子（由状态机调用）。</summary>
    public void PlacePiece(int index, Mark mark)
    {
        if (!IsValidIndex(index))
        {
            Debug.LogWarning($"BoardView.PlacePiece: invalid index {index}");
            return;
        }

        var prefab = (mark == Mark.X) ? pieceXPrefab : pieceOPrefab;
        if (!prefab)
        {
            Debug.LogError($"BoardView: missing prefab for {(mark == Mark.X ? "X" : "O")}");
            return;
        }

        // 如果该格已有棋子（通常不会发生），先清掉
        if (spawnedPieces.TryGetValue(index, out var old) && old)
            Destroy(old);

        var cell = cells[index];
        var go = Instantiate(prefab, piecesRoot ? piecesRoot : transform);
        go.transform.position = cell.position;
        go.transform.rotation = cell.rotation;

        spawnedPieces[index] = go;

        // 可选TODO：做一个轻微弹出的动效（如缩放 0.8 -> 1.0），此处略
    }

    /// <summary>供 BoardCell 调用；玩家点击某格子。</summary>
    public void OnCellClickedFromChild(int index)
    {
        // 基础拦截：若鼠标在 UI 上，则不处理世界物体点击
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 交给状态机（状态机会再次校验是否是玩家回合、该格是否可下）
        game?.TryPlayerMove(index);
    }

    // —— Hint（可选） —— //
    public void ShowHintAt(int index)
    {
        if (!IsValidIndex(index) || hintMarkerPrefab == null) return;

        if (!hintMarkerInstance)
            hintMarkerInstance = Instantiate(hintMarkerPrefab, transform);

        hintMarkerInstance.transform.position = cells[index].position;
        hintMarkerInstance.SetActive(true);
    }

    public void ClearHint()
    {
        if (hintMarkerInstance) hintMarkerInstance.SetActive(false);
    }

    // —— 工具 —— //
    private bool IsValidIndex(int i) => i >= 0 && i < 9 && cells != null && cells.Length == 9 && cells[i] != null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (cells == null || cells.Length != 9)
            cells = new Transform[9]; // 保持数组长度为9，避免误配
    }
#endif
}
