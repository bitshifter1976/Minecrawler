using System;
using UnityEngine;

public sealed class MineGameHud : MonoBehaviour
{
    public const float HudHeight = 150f;
    public const float HudTop = 12f;
    public const float HudLevelGap = 0f;

    private GUIStyle headerStyle;
    private GUIStyle valueStyle;
    private GUIStyle messageStyle;
    private GUIStyle overlayStyle;
    private GUIStyle overlayBoxStyle;
    private Texture2D panelTexture;
    private Texture2D badgeTexture;

    private void OnGUI()
    {
        MineGameManager game = MineGameManager.Instance;
        if (game == null) return;

        CreateStyles();
        DrawTopHud(game);
        DrawStateOverlay(game);
    }

    private void CreateStyles()
    {
        if (panelTexture == null)
        {
            panelTexture = CreateTexture(new Color(0.055f, 0.045f, 0.04f, 0.97f));
            badgeTexture = CreateTexture(new Color(0.15f, 0.115f, 0.07f, 0.98f));
        }

        headerStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 42,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter,
            normal = { textColor = new Color(0.88f, 0.67f, 0.24f) }
        };

        valueStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 42,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        messageStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 42,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            normal = { textColor = new Color(0.93f, 0.93f, 0.90f) }
        };

        overlayStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.045f), 36, 72),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        overlayBoxStyle ??= new GUIStyle(GUI.skin.box)
        {
            normal = { background = panelTexture }
        };
    }

    private void DrawTopHud(MineGameManager game)
    {
        Camera camera = Camera.main;

        if (camera == null ||
            game.Board == null ||
            game.Board.Width <= 0)
        {
            return;
        }

        // Die tatsächlichen äußeren Kanten des Levels liegen jeweils
        // eine halbe Welt-Einheit außerhalb der Grid-Mittelpunkte.
        Vector3 leftScreen = camera.WorldToScreenPoint(
            new Vector3(-0.5f, 0f, 0f)
        );

        Vector3 rightScreen = camera.WorldToScreenPoint(
            new Vector3(game.Board.Width - 0.5f, 0f, 0f)
        );

        float hudX = Mathf.Clamp(
            Mathf.Min(leftScreen.x, rightScreen.x),
            0f,
            Screen.width
        );

        float hudRight = Mathf.Clamp(
            Mathf.Max(leftScreen.x, rightScreen.x),
            0f,
            Screen.width
        );

        float hudWidth = Mathf.Max(1f, hudRight - hudX);
        float top = HudTop;
        float panelHeight = HudHeight;

        // HUD exakt so breit wie das sichtbare Level.
        GUI.DrawTexture(
            new Rect(hudX, top, hudWidth, panelHeight),
            panelTexture
        );

        float outerPadding = Mathf.Max(4f, hudWidth * 0.006f);
        float gap = Mathf.Max(3f, hudWidth * 0.004f);

        bool showMessage = hudWidth >= 1050f;

        float messageWidth = showMessage
            ? hudWidth * 0.34f
            : 0f;

        float badgesAreaWidth =
            hudWidth -
            outerPadding * 2f -
            messageWidth -
            (showMessage ? gap : 0f);

        float badgeWidth =
            (badgesAreaWidth - gap * 5f) / 6f;

        float badgeHeight = panelHeight - 8f;
        float x = hudX + outerPadding;

        DrawBadge(
            new Rect(x, top + 4f, badgeWidth, badgeHeight),
            "Level",
            $"{game.CurrentLevel}/{game.TotalLevels}"
        );
        x += badgeWidth + gap;

        DrawBadge(
            new Rect(x, top + 4f, badgeWidth, badgeHeight),
            "Score",
            game.Score.ToString("N0")
        );
        x += badgeWidth + gap;

        DrawBadge(
            new Rect(x, top + 4f, badgeWidth, badgeHeight),
            "Coal",
            game.RemainingCoal.ToString()
        );
        x += badgeWidth + gap;

        DrawBadge(
            new Rect(x, top + 4f, badgeWidth, badgeHeight),
            "Rocks",
            game.RemainingObstacles.ToString()
        );
        x += badgeWidth + gap;

        DrawBadge(
            new Rect(x, top + 4f, badgeWidth, badgeHeight),
            "Moves",
            game.Moves.ToString()
        );
        x += badgeWidth + gap;

        DrawBadge(
            new Rect(x, top + 4f, badgeWidth, badgeHeight),
            "Time",
            FormatTime(
                game.PlayTimeTracker?.TimeElapsed ??
                TimeSpan.Zero
            )
        );

        if (!showMessage)
            return;

        x += badgeWidth + gap;

        GUI.Label(
            new Rect(
                x,
                top + 4f,
                Mathf.Max(
                    0f,
                    hudRight - x - outerPadding
                ),
                badgeHeight
            ),
            $"{game.Message} ({game.Version})",
            messageStyle
        );
    }

    private void DrawBadge(Rect rect, string title, string value)
    {
        GUI.DrawTexture(rect, badgeTexture);
        GUI.Label(new Rect(rect.x, rect.y + 2f, rect.width, 50f), title, headerStyle);
        GUI.Label(new Rect(rect.x, rect.y + 52f, rect.width, rect.height - 52f), value, valueStyle);
    }

    private void DrawStateOverlay(MineGameManager game)
    {
        string overlayText = game.State switch
        {
            GameState.Loading => "Loading level...",
            GameState.LevelReady =>
                $"LEVEL {game.CurrentLevel}\n\n" +
                "Collect all coal and destroy all rocks.\n" +
                "Then collect the key to open the exit.\n\n" +
                "Press any key, mouse button or gamepad button to start",
            GameState.Paused => "PAUSED",
            GameState.LevelCompleted =>
                $"LEVEL {game.CurrentLevel} COMPLETE\n\nScore: {game.Score:N0}",
            GameState.GameOver =>
                $"GAME OVER\n\n{game.Message}\n\nScore: {game.Score:N0}",
            GameState.Victory =>
                $"CONGRATULATIONS!\n\nYou completed all {game.TotalLevels} levels!\n\nFinal score: {game.Score:N0}",
            _ => null
        };

        if (string.IsNullOrEmpty(overlayText)) return;

        float maxWidth = Mathf.Min(820f, Screen.width - 40f);
        float calculatedHeight = overlayStyle.CalcHeight(new GUIContent(overlayText), maxWidth - 20f);
        float boxHeight = Mathf.Min(calculatedHeight + 20f, Screen.height - HudHeight - 10f);

        Rect boxRect = new(
            (Screen.width - maxWidth) * 0.5f,
            HudHeight + (Screen.height - HudHeight - boxHeight) * 0.5f,
            maxWidth,
            boxHeight
        );

        GUI.Box(boxRect, GUIContent.none, overlayBoxStyle);
        GUI.Label(
            new Rect(boxRect.x, boxRect.y + 15f, boxRect.width, boxRect.height - 30f),
            overlayText,
            overlayStyle
        );
    }

    private static string FormatTime(TimeSpan time) =>
        time.TotalHours >= 1
            ? $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}"
            : $"{time.Minutes:00}:{time.Seconds:00}";

    private static Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void OnDestroy()
    {
        if (panelTexture != null) Destroy(panelTexture);
        if (badgeTexture != null) Destroy(badgeTexture);
    }
}