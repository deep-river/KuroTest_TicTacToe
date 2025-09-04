using UnityEngine;

public abstract class ExclusivePanelBase : UIPanelBase
{
    [SerializeField] string exclusiveGroupId = "default"; // Inspector可配置

    public string ExclusiveGroupId => exclusiveGroupId;

    public override void Show(object args = null)
    {
        // 在显示前先清理同组面板
        Locator.UI.CloseExclusiveGroup(exclusiveGroupId);
        base.Show(args);
    }
}