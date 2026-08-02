using System;
using UnityEngine;

public sealed class MineGameHud : MonoBehaviour
{
    // Diese Konstanten werden vom MineGameManager für das Kamera-Layout verwendet.
    public const float HeaderHeight = 170f;
    public const float StatusHeight = 90f;
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
    private Texture2D woodPanelTexture;
    private Texture2D woodenPostTexture;

    private float scale = 1f;
    private float lastStyleScale = -1f;

    private void OnGUI()
    {
        MineGameManager game = MineGameManager.Instance;
        if (game == null)
            return;

        GUI.skin.font = Resources.Load<Font>("Fonts/RUBIK-LIGHT SDF");

        UpdateScale();
        EnsureResources();

        HudLayout layout = HudLayout.Create(scale);

        // Zuerst die seitlichen Balken zeichnen.
        // Danach liegen obere HUD- und Statusleiste sichtbar darüber.
        DrawSidePosts(layout.BoardRect);
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
            Sprite woodPanelSprite =
                Resources.Load<Sprite>("Art/WoodenPlate");

            woodPanelTexture =
                woodPanelSprite != null
                    ? woodPanelSprite.texture
                    : null;

            Sprite woodenPostSprite =
                Resources.Load<Sprite>("Art/WoodenPost");

            woodenPostTexture =
                woodenPostSprite != null
                    ? woodenPostSprite.texture
                    : null;

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
            fontSize = ScaleFont(32),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip
        };
        titleStyle.normal.textColor = new Color(0.88f, 0.67f, 0.24f);

        valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaleFont(42),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip
        };
        valueStyle.normal.textColor = Color.white;

        statusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaleFont(30),
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
        GUI.DrawTexture(
            rect,
            woodPanelTexture != null
                ? woodPanelTexture
                : panelTexture,
            ScaleMode.StretchToFill);

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
        Rect panelRect =
            new Rect(
                rect.x,
                rect.y - 5f * scale,
                rect.width,
                rect.height + 10f * scale);

        GUI.DrawTexture(
            panelRect,
            woodPanelTexture != null
                ? woodPanelTexture
                : badgeTexture,
            ScaleMode.StretchToFill);

        Color previousColor = GUI.color;

        GUI.color =
            new Color(0f, 0f, 0f, 0.24f);

        GUI.DrawTexture(
            panelRect,
            Texture2D.whiteTexture,
            ScaleMode.StretchToFill);

        GUI.color = previousColor;

        // The wooden frame is intentionally taller than the text block.
        // Both lines sit lower inside the plate and are closer together.
        float topPadding =
            24f * scale;

        float titleHeight =
            42f * scale;

        float lineGap =
            -6f * scale;

        float valueHeight =
            58f * scale;

        float textBlockHeight =
            titleHeight +
            lineGap +
            valueHeight;

        float availableHeight =
            Mathf.Max(
                1f,
                rect.height -
                topPadding);

        float textStartY =
            rect.y +
            topPadding +
            Mathf.Max(
                0f,
                (availableHeight -
                 textBlockHeight) *
                0.35f);

        GUI.Label(
            new Rect(
                rect.x,
                textStartY,
                rect.width,
                titleHeight),
            title,
            titleStyle);

        GUI.Label(
            new Rect(
                rect.x,
                textStartY +
                titleHeight +
                lineGap,
                rect.width,
                valueHeight),
            value,
            valueStyle);
    }

    private void DrawStatus(MineGameManager game, Rect rect)
    {
        GUI.DrawTexture(
            rect,
            woodPanelTexture != null
                ? woodPanelTexture
                : statusTexture,
            ScaleMode.StretchToFill);

        Color previousColor = GUI.color;

        GUI.color =
            new Color(0f, 0f, 0f, 0.30f);

        GUI.DrawTexture(
            rect,
            Texture2D.whiteTexture,
            ScaleMode.StretchToFill);

        GUI.color = previousColor;

        GUI.Label(
            new Rect(
                rect.x,
                rect.y,
                Mathf.Max(1f, rect.width),
                Mathf.Max(1f, rect.height)),
            $"MineCrawler {MineGameManager.Version}   -   {game.Message}",
            statusStyle);
    }


    private void DrawBossHealth(
        MineGameManager game,
        Rect boardRect)
    {
        if (game.RemainingBosses <= 0)
            return;

        int maximumHp =
            Mathf.Max(1, game.BossMaximumHitPoints);

        int currentHp =
            Mathf.Clamp(
                game.BossHitPoints,
                0,
                maximumHp);

        float ratio =
            (float)currentHp / maximumHp;

        float barWidth =
            Mathf.Min(
                boardRect.width * 0.68f,
                880f * scale);

        float barHeight =
            Mathf.Max(54f, 64f * scale);

        Rect outerRect = new(
            boardRect.x +
            (boardRect.width - barWidth) * 0.5f,
            boardRect.y + 14f * scale,
            barWidth,
            barHeight);

        Rect innerRect = new(
            outerRect.x + 6f * scale,
            outerRect.y + 6f * scale,
            Mathf.Max(
                0f,
                (outerRect.width - 12f * scale) * ratio),
            outerRect.height - 12f * scale);

        Color previousColor = GUI.color;

        GUI.color =
            new Color(0.03f, 0.02f, 0.02f, 0.98f);

        GUI.DrawTexture(
            outerRect,
            Texture2D.whiteTexture);

        GUI.color =
            Color.Lerp(
                new Color(0.90f, 0.08f, 0.04f),
                new Color(0.20f, 0.86f, 0.20f),
                ratio);

        GUI.DrawTexture(
            innerRect,
            Texture2D.whiteTexture);

        GUI.color = Color.white;

        GUIStyle bossStyle =
            new GUIStyle(valueStyle)
            {
                fontSize = ScaleFont(27),
                alignment = TextAnchor.MiddleCenter
            };

        bossStyle.normal.textColor = Color.white;

        GUI.Label(
            outerRect,
            $"{game.BossName}   HP {currentHp}/{maximumHp}",
            bossStyle);

        GUI.color = previousColor;
    }


    private void DrawSidePosts(Rect boardRect)
    {
        if (woodenPostTexture == null)
            return;

        // Die Pfosten gehen über die komplette Höhe:
        // von ganz oben bis ganz unten.
        float postWidth =
            Mathf.Clamp(
                90f * scale,
                48f,
                96f);

        Rect leftPostRect =
            new Rect(
                0f,
                0f,
                postWidth,
                Screen.height);

        Rect rightPostRect =
            new Rect(
                Screen.width - postWidth,
                0f,
                postWidth,
                Screen.height);

        GUI.DrawTexture(
            leftPostRect,
            woodenPostTexture,
            ScaleMode.StretchToFill,
            true);

        // Rechte Seite sauber gespiegelt.
        GUI.DrawTextureWithTexCoords(
            rightPostRect,
            woodenPostTexture,
            new Rect(
                1f,
                0f,
                -1f,
                1f),
            true);
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
                $"LEVEL {game.CurrentLevel}\n\nPress any key, mouse button or gamepad button to start!",

            GameState.Paused =>
                "PAUSED",

            GameState.LevelCompleted =>
                $"LEVEL {game.CurrentLevel} COMPLETE\n\nScore: {game.Score:N0}",

            GameState.GameOver =>
                $"GAME OVER\n\n{game.Message}\n\nScore: {game.Score:N0}",

            GameState.Victory =>
                "CONGRATULATIONS!\n\n" +
                $"You completed all {game.TotalLevels} levels!\n\n" +
                $"Final score: {game.Score:N0}\n\n" +
                "Press any key, mouse button or gamepad button to start a new game.",

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