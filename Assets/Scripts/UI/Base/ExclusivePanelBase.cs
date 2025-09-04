using UnityEngine;

public abstract class ExclusivePanelBase : UIPanelBase
{
    [SerializeField] string exclusiveGroupId = "default"; // Inspector������

    public string ExclusiveGroupId => exclusiveGroupId;

    public override void Show(object args = null)
    {
        // ����ʾǰ������ͬ�����
        Locator.UI.CloseExclusiveGroup(exclusiveGroupId);
        base.Show(args);
    }
}