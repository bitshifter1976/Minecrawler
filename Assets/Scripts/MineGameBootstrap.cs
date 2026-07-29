using UnityEngine;

public sealed class MineGameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        if (FindAnyObjectByType<MineGameManager>() != null)
            return;

        GameObject gameObject = new("MineGame");
        gameObject.AddComponent<PlayTimeTracker>();
        gameObject.AddComponent<MineGameManager>();
        gameObject.AddComponent<MineGameHud>();
    }
}
