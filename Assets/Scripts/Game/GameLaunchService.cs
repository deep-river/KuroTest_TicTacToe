// GameLaunchService.cs
using UnityEngine.SceneManagement;

public static class GameLaunchService
{
    private static GameLaunchParams _pending;

    public static void Launch(GameLaunchParams p, string sceneName)
    {
        _pending = p;
        SceneManager.LoadScene(sceneName);
    }

    public static bool TryConsume(out GameLaunchParams p)
    {
        p = _pending;
        _pending = null;
        return p != null;
    }
}
