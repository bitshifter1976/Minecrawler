using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private GUIStyle hudButtonStyle;
    private GUIStyle iconButtonStyle;
    private GUIStyle overlayTitleStyle;
    private GUIStyle overlayInfoStyle;

    private Font hudDisplayFont;

    private Texture2D panelTexture;
    private Texture2D badgeTexture;
    private Texture2D statusTexture;
    private Texture2D woodPanelTexture;
    private Texture2D woodenPostTexture;
    private Texture2D hudButtonNormalTexture;
    private Texture2D hudButtonHoverTexture;
    private Texture2D hudButtonActiveTexture;
    private Texture2D pauseIconTexture;
    private Texture2D gearIconTexture;

    private float scale = 1f;
    private float lastStyleScale = -1f;
    private Rect timeBadgeRect;

    private void OnGUI()
    {
        MineGameManager game = MineGameManager.Instance;
        if (game == null)
            return;

        GUI.skin.font =
            Resources.Load<Font>(
                "Fonts/CinzelDecorative-Black");

        if (hudDisplayFont == null)
        {
            hudDisplayFont =
                Resources.Load<Font>(
                    "Fonts/CinzelDecorative-Black");

            if (hudDisplayFont == null)
            {
                hudDisplayFont =
                    Resources.Load<Font>(
                        "Fonts/CinzelDecorative-Black");
            }
        }

        UpdateScale();
        EnsureResources();

        HudLayout layout = HudLayout.Create(scale);

        // Schwarze Letterbox-Bereiche oberhalb und unterhalb des Levels
        // werden zuerst mit WoodenPlate gefüllt.
        DrawWoodenBoardGaps(game, layout);

        // Danach folgen seitliche Balken, HUD und Statusleiste.
        DrawSidePosts(game, layout.BoardRect);
        DrawHeader(game, layout.HeaderRect);
        DrawStatus(game, layout.StatusRect);
        DrawHudButtons(game, layout.StatusRect);
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

            hudButtonNormalTexture =
                CreateMetalButtonTexture(
                    96,
                    new Color(0.10f, 0.065f, 0.025f, 1f),
                    new Color(0.49f, 0.29f, 0.08f, 1f),
                    new Color(0.95f, 0.65f, 0.20f, 1f),
                    "HUD Button Normal");

            hudButtonHoverTexture =
                CreateMetalButtonTexture(
                    96,
                    new Color(0.16f, 0.09f, 0.025f, 1f),
                    new Color(0.72f, 0.42f, 0.09f, 1f),
                    new Color(1f, 0.83f, 0.38f, 1f),
                    "HUD Button Hover");

            hudButtonActiveTexture =
                CreateMetalButtonTexture(
                    96,
                    new Color(0.055f, 0.035f, 0.018f, 1f),
                    new Color(0.35f, 0.19f, 0.045f, 1f),
                    new Color(0.77f, 0.48f, 0.13f, 1f),
                    "HUD Button Active");

            pauseIconTexture =
                CreatePauseIconTexture(
                    64,
                    new Color(1f, 0.84f, 0.38f, 1f));

            gearIconTexture =
                CreateGearIconTexture(
                    64,
                    new Color(1f, 0.84f, 0.38f, 1f));
        }

        if (Mathf.Approximately(lastStyleScale, scale) &&
            titleStyle != null &&
            valueStyle != null &&
            statusStyle != null &&
            overlayStyle != null &&
            overlayBoxStyle != null &&
            hudButtonStyle != null &&
            iconButtonStyle != null)
        {
            return;
        }

        lastStyleScale = scale;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            font =
                hudDisplayFont != null
                    ? hudDisplayFont
                    : GUI.skin.font,

            fontSize = ScaleFont(34),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip
        };
        titleStyle.normal.textColor = Color.white;

        valueStyle = new GUIStyle(GUI.skin.label)
        {
            font =
                hudDisplayFont != null
                    ? hudDisplayFont
                    : GUI.skin.font,

            fontSize = ScaleFont(29),
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

        overlayTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaleFont(46),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = false,
            clipping = TextClipping.Overflow
        };
        overlayTitleStyle.normal.textColor = Color.white;

        overlayInfoStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaleFont(22),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = false,
            clipping = TextClipping.Overflow
        };
        overlayInfoStyle.normal.textColor = new Color(0.94f,0.92f,0.84f);

        overlayStyle = overlayTitleStyle;

        overlayBoxStyle = new GUIStyle(GUI.skin.box);
        overlayBoxStyle.normal.background = panelTexture;

        hudButtonStyle = new GUIStyle(GUI.skin.button);

        iconButtonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
            border = new RectOffset(
                ScaleFont(14),
                ScaleFont(14),
                ScaleFont(14),
                ScaleFont(14))
        };

        iconButtonStyle.normal.background =
            hudButtonNormalTexture;

        iconButtonStyle.hover.background =
            hudButtonHoverTexture;

        iconButtonStyle.active.background =
            hudButtonActiveTexture;

        iconButtonStyle.focused.background =
            hudButtonHoverTexture;
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
            Rect badgeRect =
                new Rect(
                    x,
                    rect.y,
                    badgeWidth,
                    rect.height);

            if (i == badgeCount - 1)
                timeBadgeRect = badgeRect;

            DrawBadge(
                badgeRect,
                titles[i],
                values[i],
                1f);

            x += badgeWidth + gap;
        }
    }


    private static readonly Color BurnGlow =
        new Color(1f, 0.36f, 0.04f, 0.22f);

    private static readonly Color BurnShadow =
        new Color(0.025f, 0.012f, 0.004f, 0.98f);

    private static readonly Color BurnBody =
        new Color(0.19f, 0.065f, 0.014f, 1f);

    private static readonly Color BurnEdge =
        new Color(0.90f, 0.42f, 0.08f, 1f);

    private static readonly Color BrassShadow =
        new Color(0.07f, 0.035f, 0.008f, 0.96f);

    private static readonly Color BrassDark =
        new Color(0.42f, 0.22f, 0.045f, 1f);

    private static readonly Color BrassMain =
        new Color(0.93f, 0.70f, 0.25f, 1f);

    private static readonly Color BrassHighlight =
        new Color(1f, 0.91f, 0.58f, 0.82f);

    private void DrawBurnedHeading(
        Rect rect,
        string text,
        GUIStyle style)
    {
        Color previousColor = GUI.color;

        float glowOffset =
            Mathf.Max(
                1f,
                2f * scale);

        GUI.color = BurnGlow;

        Vector2[] offsets =
        {
            new(-glowOffset, 0f),
            new(glowOffset, 0f),
            new(0f, -glowOffset),
            new(0f, glowOffset),
            new(-glowOffset, -glowOffset),
            new(glowOffset, -glowOffset),
            new(-glowOffset, glowOffset),
            new(glowOffset, glowOffset)
        };

        foreach (Vector2 offset in offsets)
        {
            GUI.Label(
                new Rect(
                    rect.x + offset.x,
                    rect.y + offset.y,
                    rect.width,
                    rect.height),
                text,
                style);
        }

        GUI.color = BurnShadow;

        GUI.Label(
            new Rect(
                rect.x + 2f * scale,
                rect.y + 3f * scale,
                rect.width,
                rect.height),
            text,
            style);

        GUI.color = BurnBody;

        GUI.Label(
            new Rect(
                rect.x + 1f * scale,
                rect.y + 1f * scale,
                rect.width,
                rect.height),
            text,
            style);

        GUI.color = BurnEdge;
        GUI.Label(rect, text, style);

        GUI.color = previousColor;
    }

    private void DrawBrassValue(
        Rect rect,
        string text,
        GUIStyle style)
    {
        Color previousColor = GUI.color;

        // Strong drop shadow.
        GUI.color = BrassShadow;

        GUI.Label(
            new Rect(
                rect.x + 2f * scale,
                rect.y + 3f * scale,
                rect.width,
                rect.height),
            text,
            style);

        // Dark brass bevel.
        GUI.color = BrassDark;

        GUI.Label(
            new Rect(
                rect.x + 1f * scale,
                rect.y + 1f * scale,
                rect.width,
                rect.height),
            text,
            style);

        // Main metal color.
        GUI.color = BrassMain;
        GUI.Label(rect, text, style);

        // Small upper-left highlight creates an inset metallic edge.
        GUI.color = BrassHighlight;

        GUI.Label(
            new Rect(
                rect.x - 0.7f * scale,
                rect.y - 0.7f * scale,
                rect.width,
                rect.height),
            text,
            style);

        GUI.color = previousColor;
    }

    private void DrawBadge(
        Rect rect,
        string title,
        string value,
        float contentWidthFactor)
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

        Rect contentRect =
            new Rect(
                rect.x,
                rect.y,
                rect.width *
                Mathf.Clamp01(
                    contentWidthFactor),
                rect.height);

        DrawBurnedHeading(
            new Rect(
                contentRect.x,
                textStartY,
                contentRect.width,
                titleHeight),
            title,
            titleStyle);

        DrawBrassValue(
            new Rect(
                contentRect.x,
                textStartY +
                titleHeight +
                lineGap,
                contentRect.width,
                valueHeight),
            value,
            valueStyle);
    }


    private void DrawHudButtons(
        MineGameManager game,
        Rect statusRect)
    {
        if (iconButtonStyle == null)
            return;

        float buttonSize =
            Mathf.Clamp(
                statusRect.height * 0.88f,
                68f,
                84f);

        float gap =
            Mathf.Clamp(
                14f * scale,
                10f,
                22f);

        float rightPadding =
            Mathf.Clamp(
                18f * scale,
                12f,
                30f);

        float y =
            statusRect.y +
            (statusRect.height -
             buttonSize) *
            0.5f;

        Rect optionsRect =
            new Rect(
                statusRect.xMax -
                rightPadding -
                buttonSize,
                y,
                buttonSize,
                buttonSize);

        Rect pauseRect =
            new Rect(
                optionsRect.x -
                gap -
                buttonSize,
                y,
                buttonSize,
                buttonSize);

        bool pauseEnabled =
            game.State == GameState.Playing ||
            game.State == GameState.Paused;

        bool previousEnabled =
            GUI.enabled;

        GUI.enabled =
            pauseEnabled;

        if (GUI.Button(
                pauseRect,
                GUIContent.none,
                iconButtonStyle))
        {
            game.TogglePause();
        }

        DrawButtonIcon(
            pauseRect,
            pauseIconTexture,
            pauseEnabled
                ? Color.white
                : new Color(
                    0.42f,
                    0.37f,
                    0.28f,
                    0.72f));

        GUI.enabled =
            previousEnabled;

        if (GUI.Button(
                optionsRect,
                GUIContent.none,
                iconButtonStyle))
        {
            SceneManager.LoadScene(2);
        }

        DrawButtonIcon(
            optionsRect,
            gearIconTexture,
            Color.white);

        if (game.State == GameState.Paused)
        {
            DrawPausedIndicator(
                pauseRect);
        }
    }

    private void DrawButtonIcon(
        Rect buttonRect,
        Texture2D icon,
        Color tint)
    {
        if (icon == null)
            return;

        float inset =
            buttonRect.width * 0.13f;

        Rect iconRect =
            new Rect(
                buttonRect.x + inset,
                buttonRect.y + inset,
                buttonRect.width - inset * 2f,
                buttonRect.height - inset * 2f);

        Color previousColor =
            GUI.color;

        GUI.color =
            tint;

        GUI.DrawTexture(
            iconRect,
            icon,
            ScaleMode.ScaleToFit,
            true);

        GUI.color =
            previousColor;
    }

    private void DrawPausedIndicator(
        Rect pauseRect)
    {
        float size =
            Mathf.Max(
                5f,
                pauseRect.width * 0.13f);

        Rect indicator =
            new Rect(
                pauseRect.xMax -
                size * 0.85f,
                pauseRect.y +
                size * 0.05f,
                size,
                size);

        Color previousColor =
            GUI.color;

        GUI.color =
            new Color(
                0.30f,
                1f,
                0.38f,
                1f);

        GUI.DrawTexture(
            indicator,
            Texture2D.whiteTexture);

        GUI.color =
            previousColor;
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

        float reservedButtonWidth =
            Mathf.Clamp(
                rect.height * 2.18f,
                180f,
                230f);

        GUI.Label(
            new Rect(
                rect.x + 18f * scale,
                rect.y,
                Mathf.Max(
                    1f,
                    rect.width -
                    reservedButtonWidth -
                    36f * scale),
                Mathf.Max(
                    1f,
                    rect.height)),
            game.Message,
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



    private void DrawWoodenBoardGaps(
        MineGameManager game,
        HudLayout layout)
    {
        if (woodPanelTexture == null ||
            !TryGetBoardScreenBounds(
                game,
                out Rect boardScreenRect))
        {
            return;
        }

        float gameplayTop =
            layout.BoardRect.y;

        float gameplayBottom =
            layout.BoardRect.yMax;

        float visibleBoardTop =
            Mathf.Clamp(
                boardScreenRect.y,
                gameplayTop,
                gameplayBottom);

        float visibleBoardBottom =
            Mathf.Clamp(
                boardScreenRect.yMax,
                gameplayTop,
                gameplayBottom);

        float topGapHeight =
            Mathf.Max(
                0f,
                visibleBoardTop -
                gameplayTop);

        float bottomGapHeight =
            Mathf.Max(
                0f,
                gameplayBottom -
                visibleBoardBottom);

        if (topGapHeight > 0.5f)
        {
            Rect topGap =
                new Rect(
                    layout.BoardRect.x,
                    gameplayTop,
                    layout.BoardRect.width,
                    topGapHeight + 1f);

            DrawWoodenGapPlate(
                topGap,
                false);
        }

        if (bottomGapHeight > 0.5f)
        {
            Rect bottomGap =
                new Rect(
                    layout.BoardRect.x,
                    visibleBoardBottom - 1f,
                    layout.BoardRect.width,
                    bottomGapHeight + 1f);

            DrawWoodenGapPlate(
                bottomGap,
                true);
        }
    }

    private void DrawWoodenGapPlate(
        Rect rect,
        bool flipVertically)
    {
        if (woodPanelTexture == null ||
            rect.width <= 0f ||
            rect.height <= 0f)
        {
            return;
        }

        Rect textureCoordinates =
            flipVertically
                ? new Rect(
                    0f,
                    1f,
                    1f,
                    -1f)
                : new Rect(
                    0f,
                    0f,
                    1f,
                    1f);

        GUI.DrawTextureWithTexCoords(
            rect,
            woodPanelTexture,
            textureCoordinates,
            true);

        // Leichte Abdunklung, damit die Füllflächen optisch zum Rahmen passen.
        Color previousColor =
            GUI.color;

        GUI.color =
            new Color(
                0f,
                0f,
                0f,
                0.16f);

        GUI.DrawTexture(
            rect,
            Texture2D.whiteTexture,
            ScaleMode.StretchToFill);

        GUI.color =
            previousColor;
    }

    private static bool TryGetBoardScreenBounds(
        MineGameManager game,
        out Rect boardRect)
    {
        boardRect = default;

        Camera camera =
            Camera.main;

        if (camera == null ||
            game?.Board == null ||
            game.Board.Width <= 0 ||
            game.Board.Height <= 0)
        {
            return false;
        }

        Vector3 bottomLeft =
            camera.WorldToScreenPoint(
                new Vector3(
                    -0.5f,
                    -0.5f,
                    0f));

        Vector3 topRight =
            camera.WorldToScreenPoint(
                new Vector3(
                    game.Board.Width - 0.5f,
                    game.Board.Height - 0.5f,
                    0f));

        float left =
            Mathf.Min(
                bottomLeft.x,
                topRight.x);

        float right =
            Mathf.Max(
                bottomLeft.x,
                topRight.x);

        // Screen coordinates start at the bottom;
        // IMGUI coordinates start at the top.
        float top =
            Screen.height -
            Mathf.Max(
                bottomLeft.y,
                topRight.y);

        float bottom =
            Screen.height -
            Mathf.Min(
                bottomLeft.y,
                topRight.y);

        boardRect =
            Rect.MinMaxRect(
                left,
                top,
                right,
                bottom);

        return true;
    }

    private void DrawSidePosts(
        MineGameManager game,
        Rect gameplayRect)
    {
        if (woodenPostTexture == null)
            return;

        if (!TryGetBoardScreenBounds(
                game,
                out Rect boardScreenRect))
        {
            return;
        }

        float boardLeft =
            Mathf.Clamp(
                boardScreenRect.xMin,
                gameplayRect.xMin,
                gameplayRect.xMax);

        float boardRight =
            Mathf.Clamp(
                boardScreenRect.xMax,
                gameplayRect.xMin,
                gameplayRect.xMax);

        float leftWidth =
            Mathf.Max(
                0f,
                boardLeft -
                gameplayRect.xMin);

        float rightWidth =
            Mathf.Max(
                0f,
                gameplayRect.xMax -
                boardRight);

        // Kleine Überlappung verhindert schwarze 1-Pixel-Nähte.
        float overlap =
            Mathf.Max(
                2f,
                3f * scale);

        if (leftWidth > 0.5f)
        {
            Rect leftPostRect =
                new Rect(
                    gameplayRect.xMin,
                    0f,
                    leftWidth + overlap,
                    Screen.height);

            GUI.DrawTexture(
                leftPostRect,
                woodenPostTexture,
                ScaleMode.StretchToFill,
                true);
        }

        if (rightWidth > 0.5f)
        {
            Rect rightPostRect =
                new Rect(
                    boardRight - overlap,
                    0f,
                    rightWidth + overlap,
                    Screen.height);

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
    }

    private void DrawStateOverlay(
        MineGameManager game,
        Rect boardRect)
    {
        GetOverlayContent(
            game,
            out string title,
            out string subtitle,
            out string detail);

        if (string.IsNullOrEmpty(title))
            return;

        float boxWidth =
            Mathf.Clamp(
                boardRect.width * 0.46f,
                560f * scale,
                940f * scale);

        boxWidth =
            Mathf.Min(
                boxWidth,
                boardRect.width -
                64f * scale);

        int bodyLines = 0;

        if (!string.IsNullOrEmpty(subtitle))
            bodyLines++;

        if (!string.IsNullOrEmpty(detail))
            bodyLines++;

        float horizontalPadding =
            Mathf.Clamp(
                46f * scale,
                28f,
                70f);

        float verticalPadding =
            Mathf.Clamp(
                24f * scale,
                18f,
                42f);

        float titleHeight =
            Mathf.Clamp(
                72f * scale,
                54f,
                106f);

        float bodyHeight =
            Mathf.Clamp(
                38f * scale,
                28f,
                56f);

        float gap =
            Mathf.Clamp(
                10f * scale,
                7f,
                18f);

        float boxHeight =
            verticalPadding * 2f +
            titleHeight +
            bodyLines * bodyHeight +
            bodyLines * gap;

        Rect boxRect =
            new Rect(
                boardRect.x +
                (boardRect.width -
                 boxWidth) * 0.5f,
                boardRect.y +
                (boardRect.height -
                 boxHeight) * 0.5f,
                boxWidth,
                boxHeight);

        GUI.Box(
            boxRect,
            GUIContent.none,
            overlayBoxStyle);

        float contentWidth =
            boxRect.width -
            horizontalPadding * 2f;

        float y =
            boxRect.y +
            verticalPadding;

        Rect titleRect =
            new Rect(
                boxRect.x +
                horizontalPadding,
                y,
                contentWidth,
                titleHeight);

        DrawFittedOverlayLine(
            titleRect,
            title,
            overlayTitleStyle,
            ScaleFont(46),
            ScaleFont(28));

        y =
            titleRect.yMax +
            gap;

        if (!string.IsNullOrEmpty(subtitle))
        {
            Rect subtitleRect =
                new Rect(
                    boxRect.x +
                    horizontalPadding,
                    y,
                    contentWidth,
                    bodyHeight);

            DrawFittedOverlayLine(
                subtitleRect,
                subtitle,
                overlayInfoStyle,
                ScaleFont(24),
                ScaleFont(16));

            y =
                subtitleRect.yMax +
                gap;
        }

        if (!string.IsNullOrEmpty(detail))
        {
            Rect detailRect =
                new Rect(
                    boxRect.x +
                    horizontalPadding,
                    y,
                    contentWidth,
                    bodyHeight);

            DrawFittedOverlayLine(
                detailRect,
                detail,
                overlayInfoStyle,
                ScaleFont(22),
                ScaleFont(15));
        }
    }

    private static void GetOverlayContent(
        MineGameManager game,
        out string title,
        out string subtitle,
        out string detail)
    {
        title = null;
        subtitle = null;
        detail = null;

        switch (game.State)
        {
            case GameState.Loading:
                title = "LOADING LEVEL...";
                break;

            case GameState.LevelReady:
                title =
                    $"LEVEL {game.CurrentLevel}";

                subtitle =
                    "PRESS ANY KEY TO START!";
                break;

            case GameState.Paused:
                title = "PAUSED";
                subtitle =
                    "PRESS RESUME TO CONTINUE";
                break;

            case GameState.LevelCompleted:
                title =
                    $"LEVEL {game.CurrentLevel} COMPLETE";

                subtitle =
                    $"SCORE: {game.Score:N0}";
                break;

            case GameState.GameOver:
                title = "GAME OVER";
                subtitle = game.Message;
                detail =
                    $"SCORE: {game.Score:N0}";
                break;

            case GameState.Victory:
                title = "CONGRATULATIONS!";
                subtitle =
                    $"YOU COMPLETED ALL {game.TotalLevels} LEVELS!";

                detail =
                    $"FINAL SCORE: {game.Score:N0}";
                break;
        }
    }

    private static void DrawFittedOverlayLine(
        Rect rect,
        string text,
        GUIStyle baseStyle,
        int maximumFontSize,
        int minimumFontSize)
    {
        if (string.IsNullOrEmpty(text))
            return;

        GUIStyle fittedStyle =
            new GUIStyle(
                baseStyle)
            {
                wordWrap = false,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleCenter
            };

        GUIContent content =
            new GUIContent(
                text);

        for (int fontSize =
                 Mathf.Max(
                     minimumFontSize,
                     maximumFontSize);
             fontSize >= minimumFontSize;
             fontSize--)
        {
            fittedStyle.fontSize =
                fontSize;

            if (fittedStyle.CalcSize(
                    content).x <=
                rect.width)
            {
                break;
            }
        }

        GUI.Label(
            rect,
            content,
            fittedStyle);
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}"
            : $"{time.Minutes:00}:{time.Seconds:00}";
    }

    private static Texture2D CreateMetalButtonTexture(
        int size,
        Color centerColor,
        Color ringColor,
        Color highlightColor,
        string textureName)
    {
        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false)
            {
                name = textureName,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags =
                    HideFlags.HideAndDontSave
            };

        Vector2 center =
            new Vector2(
                (size - 1) * 0.5f,
                (size - 1) * 0.5f);

        float radius =
            size * 0.46f;

        for (int y = 0;
             y < size;
             y++)
        {
            for (int x = 0;
                 x < size;
                 x++)
            {
                Vector2 point =
                    new Vector2(
                        x,
                        y);

                float distance =
                    Vector2.Distance(
                        point,
                        center);

                if (distance > radius)
                {
                    texture.SetPixel(
                        x,
                        y,
                        Color.clear);

                    continue;
                }

                float normalized =
                    distance /
                    radius;

                Color color =
                    normalized > 0.77f
                        ? ringColor
                        : centerColor;

                float topLight =
                    Mathf.Clamp01(
                        1f -
                        (point.y /
                         size));

                color =
                    Color.Lerp(
                        color,
                        highlightColor,
                        topLight *
                        (normalized > 0.66f
                            ? 0.34f
                            : 0.10f));

                if (normalized < 0.14f)
                {
                    color =
                        Color.Lerp(
                            color,
                            Color.black,
                            0.18f);
                }

                texture.SetPixel(
                    x,
                    y,
                    color);
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreatePauseIconTexture(
        int size,
        Color color)
    {
        Texture2D texture =
            CreateTransparentTexture(
                size,
                "HUD Pause Icon");

        int barWidth =
            Mathf.Max(
                3,
                size / 7);

        int barHeight =
            Mathf.RoundToInt(
                size * 0.58f);

        int top =
            (size -
             barHeight) /
            2;

        int leftOne =
            Mathf.RoundToInt(
                size * 0.27f);

        int leftTwo =
            Mathf.RoundToInt(
                size * 0.57f);

        FillTextureRect(
            texture,
            leftOne,
            top,
            barWidth,
            barHeight,
            color);

        FillTextureRect(
            texture,
            leftTwo,
            top,
            barWidth,
            barHeight,
            color);

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateGearIconTexture(
        int size,
        Color color)
    {
        Texture2D texture =
            CreateTransparentTexture(
                size,
                "HUD Gear Icon");

        Vector2 center =
            new Vector2(
                (size - 1) * 0.5f,
                (size - 1) * 0.5f);

        float outerRadius =
            size * 0.34f;

        float innerRadius =
            size * 0.15f;

        for (int y = 0;
             y < size;
             y++)
        {
            for (int x = 0;
                 x < size;
                 x++)
            {
                Vector2 delta =
                    new Vector2(
                        x,
                        y) -
                    center;

                float distance =
                    delta.magnitude;

                float angle =
                    Mathf.Atan2(
                        delta.y,
                        delta.x);

                float toothWave =
                    Mathf.Abs(
                        Mathf.Cos(
                            angle * 4f));

                float toothRadius =
                    Mathf.Lerp(
                        outerRadius * 0.82f,
                        outerRadius,
                        toothWave);

                bool ring =
                    distance <= toothRadius &&
                    distance >= innerRadius;

                if (ring)
                {
                    texture.SetPixel(
                        x,
                        y,
                        color);
                }
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateTransparentTexture(
        int size,
        string textureName)
    {
        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false)
            {
                name = textureName,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags =
                    HideFlags.HideAndDontSave
            };

        Color[] pixels =
            new Color[
                size *
                size];

        texture.SetPixels(
            pixels);

        return texture;
    }

    private static void FillTextureRect(
        Texture2D texture,
        int x,
        int y,
        int width,
        int height,
        Color color)
    {
        for (int py = y;
             py < y + height;
             py++)
        {
            for (int px = x;
                 px < x + width;
                 px++)
            {
                if (px < 0 ||
                    py < 0 ||
                    px >= texture.width ||
                    py >= texture.height)
                {
                    continue;
                }

                texture.SetPixel(
                    px,
                    py,
                    color);
            }
        }
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
        DestroyTexture(hudButtonNormalTexture);
        DestroyTexture(hudButtonHoverTexture);
        DestroyTexture(hudButtonActiveTexture);
        DestroyTexture(pauseIconTexture);
        DestroyTexture(gearIconTexture);
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