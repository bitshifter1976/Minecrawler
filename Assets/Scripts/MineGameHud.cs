using System;
using UnityEngine;

public sealed class MineGameHud : MonoBehaviour
{
    // Diese Konstanten werden vom MineGameManager für das Kamera-Layout verwendet.
    public const float HeaderHeight = 100f;
    public const float StatusHeight = 80f;
    public const float TopMargin = 0f;
    public const float BottomMargin = 0f;
    public const float LevelGap = 0f;

    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;
    private const float HorizontalMargin = 12f;
    private const float BadgeGap = 5f;

    private GUIStyle titleStyle;
    private GUIStyle valueStyle;
    private GUIStyle statusStyle;
    private GUIStyle overlayStyle;
    private GUIStyle overlayBoxStyle;

    private Texture2D panelTexture;
    private Texture2D badgeTexture;
    private Texture2D statusTexture;

    private float scale = 1f;
    private float lastStyleScale = -1f;

    private void OnGUI()
    {
        MineGameManager game = MineGameManager.Instance;
        if (game == null)
            return;

        UpdateScale();
        EnsureResources();

        HudLayout layout = HudLayout.Create(scale);

        DrawHeader(game, layout.HeaderRect);
        DrawStatus(game, layout.StatusRect);
        DrawStateOverlay(game, layout.BoardRect);
    }

    private void UpdateScale()
    {
        float widthScale = Screen.width / ReferenceWidth;
        float heightScale = Screen.height / ReferenceHeight;

        scale = Mathf.Clamp(Mathf.Min(widthScale, heightScale), 0.55f, 1.6f);
    }

    private void EnsureResources()
    {
        if (panelTexture == null)
        {
            panelTexture = CreateTexture(
                new Color(0.055f, 0.045f, 0.04f, 0.98f),
                "MineGameHud Panel");

            badgeTexture = CreateTexture(
                new Color(0.15f, 0.115f, 0.07f, 0.98f),
                "MineGameHud Badge");

            statusTexture = CreateTexture(
                new Color(0.10f, 0.08f, 0.055f, 0.98f),
                "MineGameHud Status");
        }

        if (Mathf.Approximately(lastStyleScale, scale) &&
            titleStyle != null &&
            valueStyle != null &&
            statusStyle != null &&
            overlayStyle != null &&
            overlayBoxStyle != null)
        {
            return;
        }

        lastStyleScale = scale;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaleFont(28),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip
        };
        titleStyle.normal.textColor = new Color(0.88f, 0.67f, 0.24f);

        valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaleFont(34),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip
        };
        valueStyle.normal.textColor = Color.white;

        statusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaleFont(34),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            clipping = TextClipping.Clip
        };
        statusStyle.normal.textColor = new Color(0.94f, 0.94f, 0.90f);

        overlayStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaleFont(42),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            clipping = TextClipping.Clip
        };
        overlayStyle.normal.textColor = Color.white;

        overlayBoxStyle = new GUIStyle(GUI.skin.box);
        overlayBoxStyle.normal.background = panelTexture;
    }

    private int ScaleFont(int baseSize)
    {
        return Mathf.Max(10, Mathf.RoundToInt(baseSize * scale));
    }

    private void DrawHeader(MineGameManager game, Rect rect)
    {
        GUI.DrawTexture(rect, panelTexture, ScaleMode.StretchToFill);

        const int badgeCount = 6;
        float gap = BadgeGap * scale;
        float badgeWidth = Mathf.Max(
            1f,
            (rect.width - gap * (badgeCount - 1)) / badgeCount);

        string[] titles =
        {
            "LEVEL", "SCORE", "COAL", "ROCKS", "MOVES", "TIME"
        };

        string[] values =
        {
            $"{game.CurrentLevel}/{game.TotalLevels}",
            game.Score.ToString("N0"),
            game.RemainingCoal.ToString(),
            game.RemainingObstacles.ToString(),
            game.Moves.ToString(),
            FormatTime(game.PlayTimeTracker?.TimeElapsed ?? TimeSpan.Zero)
        };

        float x = rect.x;

        for (int i = 0; i < badgeCount; i++)
        {
            DrawBadge(
                new Rect(x, rect.y, badgeWidth, rect.height),
                titles[i],
                values[i]);

            x += badgeWidth + gap;
        }
    }

    private void DrawBadge(Rect rect, string title, string value)
    {
        GUI.DrawTexture(rect, badgeTexture, ScaleMode.StretchToFill);

        float titleHeight = rect.height * 0.42f;

        GUI.Label(
            new Rect(rect.x, rect.y, rect.width, titleHeight),
            title,
            titleStyle);

        GUI.Label(
            new Rect(
                rect.x,
                rect.y + titleHeight,
                rect.width,
                rect.height - titleHeight),
            value,
            valueStyle);
    }

    private void DrawStatus(MineGameManager game, Rect rect)
    {
        GUI.DrawTexture(rect, statusTexture, ScaleMode.StretchToFill);

        GUI.Label(
            new Rect(
                rect.x,
                rect.y,
                Mathf.Max(1f, rect.width),
                Mathf.Max(1f, rect.height)),
            $"MineCrawler {game.Version}   -   {game.Message}",
            statusStyle);
    }

    private void DrawStateOverlay(MineGameManager game, Rect boardRect)
    {
        string text = GetOverlayText(game);
        if (string.IsNullOrEmpty(text))
            return;

        float margin = 20f * scale;
        float padding = 16f * scale;

        float maximumWidth = Mathf.Min(
            820f * scale,
            Mathf.Max(1f, boardRect.width - margin * 2f));

        float textWidth = Mathf.Max(1f, maximumWidth - padding * 2f);
        float contentHeight = overlayStyle.CalcHeight(new GUIContent(text), textWidth);

        float boxHeight = Mathf.Min(
            contentHeight + padding * 2f,
            Mathf.Max(1f, boardRect.height - margin * 2f));

        Rect boxRect = new Rect(
            boardRect.x + (boardRect.width - maximumWidth) * 0.5f,
            boardRect.y + (boardRect.height - boxHeight) * 0.5f,
            maximumWidth,
            boxHeight);

        GUI.Box(boxRect, GUIContent.none, overlayBoxStyle);

        GUI.Label(
            new Rect(
                boxRect.x + padding,
                boxRect.y + padding,
                Mathf.Max(1f, boxRect.width - padding * 2f),
                Mathf.Max(1f, boxRect.height - padding * 2f)),
            text,
            overlayStyle);
    }

    private static string GetOverlayText(MineGameManager game)
    {
        return game.State switch
        {
            GameState.Loading =>
                "Loading level...",

            GameState.LevelReady =>
                $"LEVEL {game.CurrentLevel}\n\nPress any key to start!",

            GameState.Paused =>
                "PAUSED",

            GameState.LevelCompleted =>
                $"LEVEL {game.CurrentLevel} COMPLETE\n\nScore: {game.Score:N0}",

            GameState.GameOver =>
                $"GAME OVER\n\n{game.Message}\n\nScore: {game.Score:N0}",

            GameState.Victory =>
                "CONGRATULATIONS!\n\n" +
                $"You completed all {game.TotalLevels} levels!\n\n" +
                $"Final score: {game.Score:N0}",

            _ => null
        };
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}"
            : $"{time.Minutes:00}:{time.Seconds:00}";
    }

    private static Texture2D CreateTexture(Color color, string textureName)
    {
        Texture2D texture = new Texture2D(1, 1)
        {
            name = textureName,
            hideFlags = HideFlags.HideAndDontSave
        };

        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void OnDestroy()
    {
        DestroyTexture(panelTexture);
        DestroyTexture(badgeTexture);
        DestroyTexture(statusTexture);
    }

    private static void DestroyTexture(Texture2D texture)
    {
        if (texture == null)
            return;

        if (Application.isPlaying)
            Destroy(texture);
        else
            DestroyImmediate(texture);
    }

    private readonly struct HudLayout
    {
        public Rect HeaderRect { get; }
        public Rect BoardRect { get; }
        public Rect StatusRect { get; }

        private HudLayout(Rect headerRect, Rect boardRect, Rect statusRect)
        {
            HeaderRect = headerRect;
            BoardRect = boardRect;
            StatusRect = statusRect;
        }

        public static HudLayout Create(float uiScale)
        {
            float width = Mathf.Max(1f, Screen.width);

            // Die Höhen bleiben identisch zu den Konstanten, weil der
            // MineGameManager genau diesen Platz für die Kamera reserviert.
            Rect header = new Rect(
                0f,
                TopMargin,
                width,
                HeaderHeight);

            float boardTop = TopMargin + HeaderHeight + LevelGap;
            float boardBottom =
                Screen.height - StatusHeight - BottomMargin - LevelGap;

            Rect board = new Rect(
                0f,
                boardTop,
                width,
                Mathf.Max(1f, boardBottom - boardTop));

            Rect status = new Rect(
                0f,
                Screen.height - StatusHeight - BottomMargin,
                width,
                StatusHeight);

            return new HudLayout(header, board, status);
        }
    }
}