using System;
using UnityEngine;

/// <summary>
/// Compatibility wrapper for older code.
/// New code should call BossExplosionController.Create directly.
/// </summary>
public static class BossExplosionAnimation
{
    public static void Create(
        Vector3 position,
        Transform parent = null,
        float customScale = 2.25f,
        Action onSequenceMilestone = null)
    {
        BossExplosionController.Create(
            position,
            parent,
            customScale,
            onSequenceMilestone);
    }
}
