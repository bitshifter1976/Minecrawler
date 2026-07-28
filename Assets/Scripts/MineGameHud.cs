using System;
using UnityEngine;
using static Unity.VectorGraphics.VectorUtils;

public sealed class MineGameHud : MonoBehaviour
{
    const float width = 700f;
    const float height = 260f;
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;
    private GUIStyle centeredTitleStyle;
    private MineGameManager game;

    private void OnGUI()
    {
        game = MineGameManager.Instance;

        if (game == null)
            return;

        CreateStyles();
        DrawStatus(game);
        //DrawHelp();
        DrawStateOverlay(game);
    }

    private void CreateStyles()
    {
        labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 33,
                normal =
                {
                    textColor = Color.white
                }
            };

        titleStyle ??= new GUIStyle(labelStyle)
            {
                fontSize = 66,
                fontStyle = FontStyle.Bold
            };

        centeredTitleStyle ??= new GUIStyle(titleStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
    }

    private void DrawStatus(
        MineGameManager game)
    {
        GUI.Label(
            new Rect(26, 58, 840, 60),
            $"Score: {game.Score}   " +
            $"Coal: {game.Collected}   " +
            $"Remaining: {game.RemainingCoal}   " +
            $"Moves: {game.Moves}",
            labelStyle
        );
    }

    private void DrawHelp()
    {
        GUI.Label(
            new Rect(26, 132, Screen.width - 40, 60),
            $"[{game.State}]  L:{game.CurrentLevel}  Score:{game.Score}  Coal:{game.Collected}/{game.Collected + game.RemainingCoal}  Rocks:{game.RemainingObstacles}  Moves:{game.Moves}  Speed:{game.AutomaticMoveInterval:0.00}s",
            labelStyle
        );
    }

    private void DrawStateOverlay(MineGameManager game)
    {
        string overlayText = game.State switch
        {
            GameState.Loading =>
                "Loading level...",

            GameState.LevelReady =>
                $"LEVEL {game.CurrentLevel}\n\n" +
                "Collect all coal and destroy all breakable rocks.\n" +
                "The exit will open afterwards.\n\n" +
                "Press any key or gamepad button to start",

            GameState.Paused =>
                "PAUSED",

            GameState.LevelCompleted =>
                $"LEVEL {game.CurrentLevel} COMPLETE",

            GameState.GameOver =>
                "GAME OVER\n\n" + game.Message,

            GameState.Victory =>
                "CONGRATULATIONS!\n\n" +
                "You completed all 100 levels!",

            _ => null
        };

        if (overlayText == null)
            return;

        float width = Mathf.Min(820f, Screen.width - 40f);

        float textWidth = width - 80f;

        float calculatedTextHeight =
            centeredTitleStyle.CalcHeight(
                new GUIContent(overlayText),
                textWidth
            );

        float height = Mathf.Min(
            calculatedTextHeight + 80f,
            Screen.height - 40f
        );

        Rect boxRect = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height
        );

        Rect textRect = new Rect(
            boxRect.x + 40f,
            boxRect.y + 30f,
            boxRect.width - 80f,
            boxRect.height - 60f
        );

        GUI.Box(boxRect, string.Empty);
        GUI.Label(textRect, overlayText, centeredTitleStyle);
    }
}