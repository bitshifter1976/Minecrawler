using UnityEngine;

public sealed class MineGameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        if (FindAnyObjectByType<MineGameManager>() != null)
            return;

        var settings = GameSettings.Load();
        settings.LastSceneIndex = 1;
        settings.Save();
        GameObject gameObject = new("MineGame");
        gameObject.AddComponent<PlayTimeTracker>();
        gameObject.AddComponent<MineGameManager>();
        gameObject.AddComponent<MineGameHud>();
    }
}
