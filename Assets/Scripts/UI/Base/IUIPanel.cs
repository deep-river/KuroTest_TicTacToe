public interface IUIPanel
{
    string PanelId { get; }         // ΨһID���� "StartScreen"
    bool IsModal { get; }           // �Ƿ��ڵ��²㲢���ص��
    void Show(object args = null);  // �볡���ɴ�������
    void Hide();                    // �˳�
    void OnFocus(bool focused);     // ����/ʧ��֪ͨ
}