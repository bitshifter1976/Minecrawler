using UnityEngine;

public static class MineLevelLoader
{
    public static MineLevelData Load(int levelIndex)
    {
        string levelPath =
            $"Levels/level{levelIndex + 1:000}";

        TextAsset levelAsset =
            Resources.Load<TextAsset>(levelPath);

        if (levelAsset == null)
        {
            Debug.LogError(
                $"Leveldatei '{levelPath}' wurde nicht gefunden.\n" +
                $"Erwarteter Pfad: Assets/Resources/{levelPath}.txt"
            );

            return null;
        }

        string normalizedText = levelAsset.text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .TrimEnd('\n');

        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            Debug.LogError(
                $"Leveldatei '{levelPath}' ist leer."
            );

            return null;
        }

        string[] rows = normalizedText.Split('\n');

        return new MineLevelData(rows);
    }
}