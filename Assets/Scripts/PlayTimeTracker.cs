using System;
using UnityEngine;

public class PlayTimeTracker : MonoBehaviour
{
    private float totalPlayTime = 0f;
    private bool isPaused = false;

    public bool IsPaused => isPaused;
    public TimeSpan TimeElapsed => TimeSpan.FromSeconds(totalPlayTime);

    private void Update()
    {
        // Only track time if the game state is active
        if (!isPaused)
        {
            totalPlayTime += Time.unscaledDeltaTime;
        }
    }

    // Call this method to toggle the pause state from other scripts
    public void SetPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
    }
}