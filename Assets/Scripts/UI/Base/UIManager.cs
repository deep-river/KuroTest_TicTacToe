using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class UIManager : MonoBehaviour
{
    [Header("Canvas Layers")]
    public Transform baseRoot;   // Base Canvas 下的容器
    public Transform modalRoot;  // Modal Canvas 下的容器

    // 预设面板（拖引用或通过Resources/Addressables）
    [System.Serializable] public class PanelEntry { public string id; public UIPanelBase prefab; }
    public List<PanelEntry> panelPrefabs;

    readonly Stack<UIPanelBase> stack = new();

    Dictionary<string, UIPanelBase> cache = new();

    private void Awake()
    {
        Locator.UI = this;
    }

    public UIPanelBase Show(string id, object args = null)
    {
        var panel = Ensure(id);
        var root = panel.IsModal ? modalRoot : baseRoot;
        if (panel.transform.parent != root) panel.transform.SetParent(root, false);

        if (stack.Count > 0) stack.Peek().OnFocus(false);
        stack.Push(panel);
        panel.Show(args);
        panel.OnFocus(true);
        return panel;
    }

    public void CloseTop()
    {
        if (stack.Count == 0) return;
        var top = stack.Pop();
        top.Hide();
        if (stack.Count > 0) stack.Peek().OnFocus(true);
    }

    public void CloseExclusiveGroup(string groupId)
    {
        // 注意：用临时栈存储剩余面板，重新压回
        if (stack.Count == 0) return;

        var temp = new Stack<UIPanelBase>();
        while (stack.Count > 0)
        {
            var top = stack.Pop();
            if (top is ExclusivePanelBase ep && ep.ExclusiveGroupId == groupId)
            {
                top.Hide();
            }
            else
            {
                temp.Push(top);
            }
        }
        // 恢复剩余的面板
        while (temp.Count > 0) stack.Push(temp.Pop());
    }

    UIPanelBase Ensure(string id)
    {
        if (!cache.TryGetValue(id, out var panel))
        {
            var prefab = panelPrefabs.Find(p => p.id == id)?.prefab;
            var parent = prefab.IsModal ? modalRoot : baseRoot;
            panel = Instantiate(prefab, parent);
            cache[id] = panel;
        }
        return panel;
    }
}