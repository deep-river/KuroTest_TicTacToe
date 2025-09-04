using UnityEngine;

public static class Locator
{
    private static UIManager _ui;

    public static UIManager UI
    {
        get
        {
            if (_ui == null)
            {
                _ui = Object.FindObjectOfType<UIManager>();
                if (_ui == null)
                {
                    Debug.LogError("UIManager not found in scene!");
                }
            }
            return _ui;
        }
        set { _ui = value; }
    }
}