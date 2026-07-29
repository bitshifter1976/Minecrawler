using System;
using UnityEngine;

public sealed class MineGameHud : MonoBehaviour
{
    public const float HeaderHeight = 100f;
    public const float StatusHeight = 80f;
    public const float TopMargin = 0f;
    public const float BottomMargin = 10f;
    public const float LevelGap = 0f;

    private GUIStyle headerStyle;
    private GUIStyle valueStyle;
    private GUIStyle statusStyle;
    private GUIStyle overlayStyle;
    private GUIStyle overlayBoxStyle;

    private Texture2D panelTexture;
    private Texture2D badgeTexture;
    private Texture2D statusTexture;

    private void OnGUI()
    {
        MineGameManager game = MineGameManager.Instance;
        if (game == null)
            return;

        CreateStyles();

        if (!TryGetLevelScreenBounds(game, out float left, out float right, out float top, out float bottom))
            return;

        DrawHeader(game, left, right);
        DrawStatus(game, left, right, bottom);
        DrawStateOverlay(game, left, right, top, bottom);
    }

    private void CreateStyles()
    {
        if (panelTexture == null)
        {
            panelTexture = CreateTexture(new Color(0.055f, 0.045f, 0.04f, 0.98f));
            badgeTexture = CreateTexture(new Color(0.15f, 0.115f, 0.07f, 0.98f));
            statusTexture = CreateTexture(new Color(0.10f, 0.08f, 0.055f, 0.98f));
        }

        headerStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.88f, 0.67f, 0.24f) }
        };

        valueStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 34,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        statusStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 44,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal = { textColor = new Color(0.94f, 0.94f, 0.90f) }
        };

        overlayStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.045f), 32, 68),
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

    private void DrawHeader(MineGameManager game, float left, float right)
    {
        float width = Mathf.Max(1f, right - left);
        float y = TopMargin;

        GUI.DrawTexture(
            new Rect(left, y, width, HeaderHeight),
            panelTexture
        );

        const float gap = 5f;

        float availableWidth =
            width - gap * 5f;

        float badgeWidth =
            availableWidth / 6f;

        float badgeHeight =
            HeaderHeight;

        float x = left;

        DrawBadge(new Rect(x, y, badgeWidth, badgeHeight), "LEVEL", $"{game.CurrentLevel}/{game.TotalLevels}");
        x += badgeWidth + gap;

        DrawBadge(new Rect(x, y, badgeWidth, badgeHeight), "SCORE", game.Score.ToString("N0"));
        x += badgeWidth + gap;

        DrawBadge(new Rect(x, y, badgeWidth, badgeHeight), "COAL", game.RemainingCoal.ToString());
        x += badgeWidth + gap;

        DrawBadge(new Rect(x, y, badgeWidth, badgeHeight), "ROCKS", game.RemainingObstacles.ToString());
        x += badgeWidth + gap;

        DrawBadge(new Rect(x, y, badgeWidth, badgeHeight), "MOVES", game.Moves.ToString());
        x += badgeWidth + gap;

        DrawBadge(
            new Rect(x, y, badgeWidth, badgeHeight),
            "TIME",
            FormatTime(game.PlayTimeTracker?.TimeElapsed ?? TimeSpan.Zero)
        );
    }

    private void DrawStatus(MineGameManager game, float left, float right, float levelBottomGuiY)
    {
        float width = Mathf.Max(1f, right - left);
        float y = levelBottomGuiY + LevelGap;

        GUI.DrawTexture(
            new Rect(left, y, width, StatusHeight),
            statusTexture
        );

        GUI.Label(
            new Rect(left + 10f, y + 2f, width - 20f, StatusHeight - 4f),
            $"MineCrawler {game.Version}   -   {game.Message}",
            statusStyle
        );
    }

    private void DrawBadge(Rect rect, string title, string value)
    {
        GUI.DrawTexture(rect, badgeTexture);

        float titleHeight = rect.height * 0.42f;

        GUI.Label(
            new Rect(rect.x, rect.y, rect.width, titleHeight),
            title,
            headerStyle
        );

        GUI.Label(
            new Rect(rect.x, rect.y + titleHeight, rect.width, rect.height - titleHeight),
            value,
            valueStyle
        );
    }

    private void DrawStateOverlay(
        MineGameManager game,
        float left,
        float right,
        float levelTopGuiY,
        float levelBottomGuiY)
    {
        string overlayText = game.State switch
        {
            GameState.Loading => "Loading level...",
            GameState.LevelReady =>
                $"LEVEL {game.CurrentLevel}\n\n" +
                "Press any key key to start!",
            GameState.Paused => "PAUSED",
            GameState.LevelCompleted =>
                $"LEVEL {game.CurrentLevel} COMPLETE\n\n" +
                $"Score: {game.Score:N0}",
            GameState.GameOver =>
                $"GAME OVER\n\n{game.Message}\n\n" +
                $"Score: {game.Score:N0}",
            GameState.Victory =>
                $"CONGRATULATIONS!\n\n" +
                $"You completed all {game.TotalLevels} levels! Final score: {game.Score:N0}",
            _ => null
        };

        if (string.IsNullOrEmpty(overlayText))
            return;

        float levelWidth = right - left;
        float maxWidth = Mathf.Min(820f, levelWidth - 30f);
        float calculatedHeight =
            overlayStyle.CalcHeight(new GUIContent(overlayText), maxWidth - 20f);

        float levelHeight = levelBottomGuiY - levelTopGuiY;
        float boxHeight = Mathf.Min(calculatedHeight + 30f, levelHeight - 20f);

        Rect boxRect = new(
            left + (levelWidth - maxWidth) * 0.5f,
            levelTopGuiY + (levelHeight - boxHeight) * 0.5f,
            maxWidth,
            boxHeight
        );

        GUI.Box(boxRect, GUIContent.none, overlayBoxStyle);

        GUI.Label(
            new Rect(boxRect.x + 10f, boxRect.y + 10f, boxRect.width - 20f, boxRect.height - 20f),
            overlayText,
            overlayStyle
        );
    }

    private static bool TryGetLevelScreenBounds(
        MineGameManager game,
        out float left,
        out float right,
        out float top,
        out float bottom)
    {
        left = right = top = bottom = 0f;

        Camera camera = Camera.main;

        if (camera == null ||
            game.Board == null ||
            game.Board.Width <= 0 ||
            game.Board.Height <= 0)
        {
            return false;
        }

        Vector3 leftBottom = camera.WorldToScreenPoint(
            new Vector3(-0.5f, -0.5f, 0f)
        );

        Vector3 rightTop = camera.WorldToScreenPoint(
            new Vector3(game.Board.Width - 0.5f, game.Board.Height - 0.5f, 0f)
        );

        left = Mathf.Clamp(Mathf.Min(leftBottom.x, rightTop.x), 0f, Screen.width);
        right = Mathf.Clamp(Mathf.Max(leftBottom.x, rightTop.x), 0f, Screen.width);

        top = Screen.height - Mathf.Max(leftBottom.y, rightTop.y);
        bottom = Screen.height - Mathf.Min(leftBottom.y, rightTop.y);

        return true;
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
        if (statusTexture != null) Destroy(statusTexture);
    }
}