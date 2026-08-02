using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public sealed class MineGameManager : MonoBehaviour
{
    public static readonly string Version = "v1.4";

    private IDisposable startInputSubscription;
    private const int LevelCount = 100;
    private const float LevelCompleteDelay = 2.5f;
    private MineControls controls;
    private MineBoard board;

    private Coroutine levelTransitionCoroutine;
    private AudioSource audioSource;
    private AudioSource audioSourceFx;
    private AudioListener audioListener;
    private int currentLevelIndex;
    private int score;
    private int collected;
    private int moves;
    private PlayTimeTracker playTimeTracker;
    private string message = "Collect all coal chunks.";
    private bool exitingLevel;
    private Vector2Int exitDirection;
    private int lastScreenWidth;
    private int lastScreenHeight;
    private bool lastShowHud;
    private bool lastShowStatusbar;

    private GameSettings settings;

    public static MineGameManager Instance
    {
        get;
        private set;
    }

    public GameState State
    {
        get;
        private set;
    } = GameState.Loading;

    public bool IsPlaying => State == GameState.Playing;

    public bool IsWaitingForStart => State == GameState.LevelReady;

    public bool IsExiting => exitingLevel;

    public int CurrentLevel => currentLevelIndex + 1;

    public int TotalLevels => LevelCount;

    public int Score => score;

    public int Collected => collected;

    public int RemainingCoal => board?.RemainingCoal ?? 0;

    public int Moves => moves;

    public string Message => message;

    public MineBoard Board => board;

    public int RemainingObstacles => board?.RemainingObstacles ?? 0;

    public int RemainingBosses => board?.RemainingBosses ?? 0;

    public int BossHitPoints => board?.ActiveBoss?.HitPoints ?? 0;

    public int BossMaximumHitPoints => board?.ActiveBoss?.MaximumHitPoints ?? 0;

    public string BossName => board?.ActiveBoss?.BossName ?? string.Empty;

    public PlayTimeTracker PlayTimeTracker => playTimeTracker;

    public float AutomaticMoveInterval
    {
        get
        {
            float progress = currentLevelIndex / 99f;
            return Mathf.Lerp(0.55f, 0.11f, progress);
        }
    }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        settings = GameSettings.Load();
        settings.LastSceneIndex = 1;

        playTimeTracker = GetComponent<PlayTimeTracker>();
        if (playTimeTracker == null)
            playTimeTracker = gameObject.AddComponent<PlayTimeTracker>();
        playTimeTracker.TotalPlayTime = settings.Playtime;

        controls = new MineControls();
        var audioSources = FindObjectsByType<AudioSource>();
        audioSource = audioSources.Length > 0 ? audioSources[0] : null;
        audioSourceFx = audioSources.Length > 1 ? audioSources[1] : null;

        gameObject.AddComponent<FallingRockSpawner>();
        gameObject.AddComponent<WaterDropSpawner>();
        gameObject.AddComponent<FireFlySpawner>();

        Sprite fallbackSprite = CreateSquareSprite();
        var wallSprite = Resources.Load<Sprite>("Art/Wall");
        if (wallSprite == null)
        {
            Debug.LogWarning("No wall sprite assigned. Using the fallback square sprite.");
            wallSprite = fallbackSprite;
        }
        var rockSprite = Resources.Load<Sprite>("Art/Rock");
        if (rockSprite == null)
        {
            Debug.LogWarning("No rock sprite assigned. Using the fallback square sprite.");
            rockSprite = fallbackSprite;
        }
        var coalSprite = Resources.Load<Sprite>("Art/Coal");
        if (coalSprite == null)
        {
            Debug.LogWarning("No coal sprite assigned. Using the fallback square sprite.");
            coalSprite = fallbackSprite;
        }
        var floorSprite = Resources.Load<Sprite>("Art/Floor");
        if (floorSprite == null)
        {
            Debug.LogWarning("No floor sprite assigned. Using the fallback square sprite.");
            floorSprite = fallbackSprite;
        }
        var doorClosedSprite = Resources.Load<Sprite>("Art/DoorClosed");
        if (doorClosedSprite == null)
        {
            Debug.LogWarning("No door closed sprite assigned. Using the fallback square sprite.");
            doorClosedSprite = fallbackSprite;
        }
        var doorOpenSprite = Resources.Load<Sprite>("Art/DoorOpen");
        if (doorOpenSprite == null)
        {
            Debug.LogWarning("No door open sprite assigned. Using the fallback square sprite.");
            doorOpenSprite = fallbackSprite;
        }
        var cartSprite = Resources.Load<Sprite>("Art/Cart");
        if (cartSprite == null)
        {
            Debug.LogWarning("No cart sprite assigned. Using the fallback square sprite.");
            cartSprite = fallbackSprite;
        }
        var minerSprite = Resources.Load<Sprite>("Art/Miner");
        if (minerSprite == null)
        {
            Debug.LogWarning("No miner sprite assigned. Using the fallback square sprite.");
            minerSprite = fallbackSprite;
        }
        var keySprite = Resources.Load<Sprite>("Art/Key");
        if (keySprite == null)
        {
            Debug.LogWarning("No key sprite assigned. Using the fallback square sprite.");
            keySprite = fallbackSprite;
        }

        var bossSprite = Resources.Load<Sprite>("Art/Boss");
        if (bossSprite == null)
        {
            Debug.LogWarning("No boss sprite assigned. Using the fallback square sprite.");
            bossSprite = fallbackSprite;
        }

        var bossProjectileSprite =
            Resources.Load<Sprite>("Art/BossProjectile");

        board = new MineBoard(
            transform,
            fallbackSprite,
            wallSprite,
            rockSprite,
            coalSprite,
            floorSprite,
            doorClosedSprite,
            doorOpenSprite,
            cartSprite,
            minerSprite,
            keySprite,
            bossSprite,
            bossProjectileSprite);

        if (PlayerPrefs.GetInt("ContinueGame") == 1)
        {
            score = settings.Score;
            playTimeTracker.TotalPlayTime = settings.Playtime;
            moves = settings.Moves;
        }
        currentLevelIndex = settings.CurrentLevel - 1;
        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, LevelCount - 1);

        LoadLevel(currentLevelIndex);
        SetupCamera();
    }

    private void OnEnable()
    {
        controls.Gameplay.Restart.performed += OnRestart;
        controls.Gameplay.Options.performed += (callback) => SceneManager.LoadScene(2);
        controls.Gameplay.Enable();
        playTimeTracker.SetPause(false);
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
        controls.Gameplay.Restart.performed -= OnRestart;
        controls.Gameplay.Options.performed -= (callback) => SceneManager.LoadScene(2);
        playTimeTracker.SetPause(true);
    }

    private void OnDestroy()
    {
        board.RemoveKey();
        StopLevelTransition();
        DisposeLevelStartInput();

        controls?.Dispose();

        if (Instance == this)
            Instance = null;
    }

    private void OnRestart(InputAction.CallbackContext context)
    {
        switch (State)
        {
            case GameState.Loading:
                return;

            case GameState.LevelReady:
                StartLevel();
                break;

            case GameState.Victory:
                StartNewGame();
                break;

            default:
                RestartCurrentLevel();
                break;
        }
    }

    private void ArmContinueInput()
    {
        DisposeLevelStartInput();

        startInputSubscription =
            InputSystem.onAnyButtonPress.Call(control =>
            {
                if (control.device is not Keyboard &&
                    control.device is not Gamepad &&
                    control.device is not Mouse)
                {
                    return;
                }

                switch (State)
                {
                    case GameState.LevelReady:
                        StartLevel();
                        break;

                    case GameState.GameOver:
                        RestartCurrentLevel();
                        break;

                    case GameState.Victory:
                        StartNewGame();
                        break;
                }
            });
    }


    private void RestartCurrentLevel()
    {
        if (State == GameState.Loading)
            return;

        score =
            PlayerPrefs.GetInt(
                "Score",
                0);

        currentLevelIndex =
            PlayerPrefs.GetInt(
                "CurrentLevel",
                currentLevelIndex);

        LoadLevel(currentLevelIndex);
    }

    public void RequestLevelStart()
    {
        if (State == GameState.LevelReady)
            StartLevel();
    }

    public void StartLevel()
    {
        if (State != GameState.LevelReady)
            return;

        DisposeLevelStartInput();
        exitingLevel = false;
        exitDirection = Vector2Int.zero;
        State = GameState.Playing;
        message = "Collect all coal and destroy all rocks. Then collect the key to open the exit.";
        if (audioSource.clip == null)
        {
            audioSource.clip = Resources.Load<AudioClip>("Audio/MinecrawlerNoVoice");
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            audioSource.UnPause();
        }
    }

    private void DisposeLevelStartInput()
    {
        startInputSubscription?.Dispose();
        startInputSubscription = null;
    }

    public void StartNewGame()
    {
        Time.timeScale = 1f;

        currentLevelIndex = 0;
        score = 0;
        collected = 0;
        moves = 0;

        SaveCurrentLevel();

        LoadLevel(currentLevelIndex);
    }

    public void PauseGame()
    {
        if (State != GameState.Playing)
            return;

        playTimeTracker.SetPause(true);
        audioSource.Pause();
        State = GameState.Paused;
        Time.timeScale = 0f;

        message = "Game paused.";
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused)
            return;

        playTimeTracker.SetPause(false);
        audioSource.UnPause();
        Time.timeScale = 1f;
        State = GameState.Playing;

        message = board.ExitOpen
            ? "The exit is now open!"
            : $"{board.RemainingCoal} coal chunks remaining.";
    }

    public void TogglePause()
    {
        if (State == GameState.Playing)
            PauseGame();
        else if (State == GameState.Paused)
            ResumeGame();
    }

    private void LoadLevel(int levelIndex)
    {
        DisposeLevelStartInput();
        State = GameState.Loading;
        Time.timeScale = 1f;

        StopLevelTransition();

        board.RemoveKey();
        currentLevelIndex = Mathf.Clamp(levelIndex, 0, LevelCount - 1);
        exitingLevel = false;
        exitDirection = Vector2Int.zero;
        board.Tail.Clear();

        MineLevelData level = MineLevelLoader.Load(currentLevelIndex);

        if (level == null)
        {
            TriggerGameOver($"Failed to load level {currentLevelIndex + 1}.");
            return;
        }

        if (!board.Build(level, currentLevelIndex + 1))
        {
            TriggerGameOver($"Level {currentLevelIndex + 1} is invalid.");
            return;
        }

        State = GameState.LevelReady;
        message = "Collect all coal and destroy all rocks. Then collect the key to open the exit.";
        ArmContinueInput();
    }

    public bool TryMoveMiner(Vector2Int direction)
    {
        if (State != GameState.Playing || board.Miner == null || direction == Vector2Int.zero)
        {
            return false;
        }

        Vector2Int currentPosition = board.Miner.GridPosition;

        if (exitingLevel)
        {
            MoveThroughExit(currentPosition);
            return true;
        }

        Vector2Int targetPosition = currentPosition + direction;

        if (!board.IsInside(targetPosition))
        {
            TriggerGameOver("You left the mine through the wall!");
            return false;
        }

        LevelBoss boss =
            board.FindBossBlockingMove(
                currentPosition,
                targetPosition);
        if (boss != null)
        {
            moves++;

            bool destroyed = boss.Hit();

            board.Miner.Bounce();

            Debug.Log(
                $"Boss collision registered. HP now " +
                $"{boss.HitPoints}/{boss.MaximumHitPoints}");

            message =
                destroyed
                    ? "Boss destroyed!"
                    : $"Boss hit! {boss.HitPoints}/{boss.MaximumHitPoints} HP remaining.";

            if (audioSourceFx != null)
            {
                AudioClip hitClip =
                    Resources.Load<AudioClip>("Audio/bossHit");

                if (hitClip != null)
                    audioSourceFx.PlayOneShot(hitClip, 0.8f);
            }

            if (destroyed)
            {
                int bossTier = Mathf.Max(1, CurrentLevel / 10);
                score += 1000 * bossTier;

                if (audioSourceFx != null)
                {
                    AudioClip destroyedClip =
                        Resources.Load<AudioClip>("Audio/bossDestroyed");

                    if (destroyedClip != null)
                        audioSourceFx.PlayOneShot(destroyedClip);
                }

                Camera.main?
                    .GetComponent<CameraShake>()?
                    .Shake(0.45f, 0.16f);

                boss.PlayDestroyedEffect();
                board.RemoveBoss(boss);
                message = "Boss destroyed!";
                CheckLevelCleared();
            }
            return true;
        }

        if (board.IsWall(targetPosition))
        {
            TriggerGameOver("You crashed into a wall!");
            return false;
        }

        if (board.IsExit(targetPosition))
        {
            if (TryEnterExit(targetPosition) == false)
            {
                TriggerGameOver("The exit is still closed!");
                return false;
            }
        }

        if (board.Tail.Contains(targetPosition))
        {
            TriggerGameOver("You crashed into your own tail!");
            return false;
        }


        var obstacle = board.GetObstacle(targetPosition);
        if (obstacle != null)
        {
            audioSourceFx.PlayOneShot(Resources.Load<AudioClip>("Audio/destroy"), 0.25f);
            board.RemoveObstacle(obstacle);
            score += 25;
            moves++;
            CheckLevelCleared();
            return true;
        }

        Vector2Int newTailPosition =
        board.Tail.GetNewSegmentPosition(currentPosition);

        board.Miner.SetGridPosition(targetPosition);
        board.Tail.Move(currentPosition);

        moves++;

        if (board.SpawnedKey != null && board.SpawnedKey.GridPosition == targetPosition)
        {
            audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/keyPickup"));
            board.CollectKey();
            audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/openDoor"), 2f);
        }

        CoalPickup pickup = board.GetCoal(targetPosition);

        if (pickup != null)
        {
            audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/pickup"));
            CollectCoal(pickup, newTailPosition);
        }

        return true;
    }

    private void TriggerGameOver(string reason)
    {
        if (State != GameState.Playing)
            return;

        audioSource?.Pause();

        AudioClip gameOverClip =
            Resources.Load<AudioClip>(
                "Audio/gameOver");

        if (audioSourceFx != null &&
            gameOverClip != null)
        {
            audioSourceFx.PlayOneShot(
                gameOverClip);
        }

        State = GameState.GameOver;

        message =
            reason +
            " Press any key, mouse button or gamepad button to restart.";

        ArmContinueInput();

        Debug.Log(
            $"Game Over: {reason}");
    }

    private bool TryEnterExit(Vector2Int targetPosition)
    {
        if (!board.ExitOpen)
        {
            message = $"The exit is closed. {board.RemainingCoal} coal and {board.RemainingObstacles} rocks remaining.";
            return false;
        }

        exitDirection = GetExitDirection(targetPosition);

        if (exitDirection == Vector2Int.zero)
        {
            Debug.LogError("The exit must be located in the outer wall.");
            return false;
        }

        Vector2Int previousMinerPosition = board.Miner.GridPosition;

        board.Miner.SetGridPosition(targetPosition);
        board.Tail.Move(previousMinerPosition);

        moves++;
        exitingLevel = true;
        message = "Leaving the mine...";

        return true;
    }

    private void MoveThroughExit(Vector2Int currentMinerPosition)
    {
        Vector2Int nextMinerPosition = currentMinerPosition + exitDirection;

        board.Miner.SetGridPosition(nextMinerPosition);
        board.Tail.Move(currentMinerPosition);
        board.Tail.RemoveOutside(board.Width, board.Height);

        moves++;

        bool minerIsOutside = !board.IsInside(board.Miner.GridPosition);

        if (minerIsOutside)
        {
            SpriteRenderer minerRenderer =
                board.Miner.GetComponent<SpriteRenderer>();

            if (minerRenderer != null)
                minerRenderer.enabled = false;
        }

        if (minerIsOutside && board.Tail.IsEmpty)
        {
            exitingLevel = false;
            CompleteLevel();
        }
    }

    private Vector2Int GetExitDirection(Vector2Int exitPosition)
    {
        if (exitPosition.x == 0)
            return Vector2Int.left;

        if (exitPosition.x == board.Width - 1)
            return Vector2Int.right;

        if (exitPosition.y == 0)
            return Vector2Int.down;

        if (exitPosition.y == board.Height - 1)
            return Vector2Int.up;

        return Vector2Int.zero;
    }

    private void CollectCoal(CoalPickup pickup, Vector2Int newTailPosition)
    {
        if (!board.RemoveCoal(pickup))
            return;

        collected++;
        score += 100 + collected * 10;
        TailSegment segment = board.CreateTailSegment(newTailPosition);
        board.Tail.Add(segment);

        CheckLevelCleared();
    }

    private void CheckLevelCleared()
    {
        if (board.IsLevelCleared)
        {
            board.SpawnKey();
            message = "The exit key has appeared!";
            return;
        }

        message = GetRemainingObjectivesMessage();
    }


    private string GetRemainingObjectivesMessage()
    {
        if (board == null)
            return string.Empty;

        if (board.RemainingBosses > 0)
        {
            return
                $"{board.RemainingCoal} coal, " +
                $"{board.RemainingObstacles} rocks and " +
                $"{board.RemainingBosses} boss remaining.";
        }

        return
            $"{board.RemainingCoal} coal and " +
            $"{board.RemainingObstacles} rocks remaining.";
    }

    public void BossProjectileHit()
    {
        TriggerGameOver("You were hit by the boss!");
    }

    private void CompleteLevel()
    {
        if (State != GameState.Playing)
            return;

        audioSource.Pause();
        audioSourceFx.PlayOneShot(Resources.Load<AudioClip>("Audio/victory"));
        State = GameState.LevelCompleted;

        message = $"Level {currentLevelIndex + 1} complete!";

        StopLevelTransition();

        levelTransitionCoroutine = StartCoroutine(LevelCompletedRoutine());
    }

    private IEnumerator LevelCompletedRoutine()
    {
        yield return new WaitForSecondsRealtime(LevelCompleteDelay);

        levelTransitionCoroutine = null;

        if (currentLevelIndex >= LevelCount - 1)
        {
            Victory();
            yield break;
        }

        currentLevelIndex++;
        SaveCurrentLevel();
        LoadLevel(currentLevelIndex);
    }

    private void StopLevelTransition()
    {
        if (levelTransitionCoroutine == null)
            return;

        StopCoroutine(levelTransitionCoroutine);
        levelTransitionCoroutine = null;
    }

    private void Victory()
    {
        DisposeLevelStartInput();

        State = GameState.Victory;

        score =
            Math.Clamp(
                score - moves * 3,
                0,
                int.MaxValue);

        SaveCurrentLevel();

        message =
            "Congratulations! You completed all levels. " +
            "Press any key, mouse button or gamepad button to start a new game.";

        ArmContinueInput();
    }

    private void SaveCurrentLevel()
    {
        PlayerPrefs.SetInt("CurrentLevel", currentLevelIndex);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.Save();
    }

    private static Sprite CreateSquareSprite()
    {
        Texture2D texture = new(1, 1);
        texture.name = "Runtime Square";
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private void LateUpdate()
    {
        settings ??= GameSettings.Load();
        if (Screen.width == lastScreenWidth &&
            Screen.height == lastScreenHeight &&
            lastShowHud==settings.ShowHud &&
            lastShowStatusbar==settings.ShowStatusbar)
        {
            return;
        }

        UpdateCamera();
    }

    private void SetupCamera()
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            audioListener = cameraObject.AddComponent<AudioListener>();
            audioListener.enabled = true;
            cameraObject.AddComponent<CameraShake>();
        }

        camera.orthographic = true;
        camera.backgroundColor = new Color(0.06f, 0.05f, 0.05f);

        UpdateCamera();
    }

    private void UpdateCamera()
    {
        Camera camera = Camera.main;

        if (camera == null || board.Width <= 0 || board.Height <= 0)
            return;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        settings ??= GameSettings.Load();
        lastShowHud=settings.ShowHud;
        lastShowStatusbar=settings.ShowStatusbar;

        camera.rect = new Rect(0f, 0f, 1f, 1f);

        const float horizontalPaddingWorld = 0.35f;
        const float verticalPaddingWorld = 0f;

        float screenWidth = Mathf.Max(1f, Screen.width);
        float screenHeight = Mathf.Max(1f, Screen.height);
        float screenAspect = screenWidth / screenHeight;

        settings ??= GameSettings.Load();
        float gameplayTopPixels = MineGameHud.TopMargin +
            (settings.ShowHud ? MineGameHud.HeaderHeight + MineGameHud.LevelGap : 0f);

        float gameplayBottomPixels =
            (settings.ShowStatusbar ? MineGameHud.StatusHeight + MineGameHud.LevelGap : 0f) +
            MineGameHud.BottomMargin;

        float gameplayHeightPixels =
            Mathf.Max(
                1f,
                screenHeight -
                gameplayTopPixels -
                gameplayBottomPixels
            );

        float gameplayHeightFraction =
            gameplayHeightPixels / screenHeight;

        float requiredWorldWidth =
            board.Width + horizontalPaddingWorld * 2f;

        float requiredWorldHeight =
            board.Height + verticalPaddingWorld * 2f;

        float sizeForWidth =
            requiredWorldWidth /
            (2f * screenAspect);

        float sizeForHeight =
            requiredWorldHeight /
            (2f * gameplayHeightFraction);

        camera.orthographicSize =
            Mathf.Max(sizeForWidth, sizeForHeight);

        float levelCenterX =
            (board.Width - 1) * 0.5f;

        float levelCenterY =
            (board.Height - 1) * 0.5f;

        float gameplayCenterPixelsFromBottom =
            gameplayBottomPixels +
            gameplayHeightPixels * 0.5f;

        float gameplayCenterNormalized =
            gameplayCenterPixelsFromBottom / screenHeight;

        float cameraY =
            levelCenterY -
            (gameplayCenterNormalized - 0.5f) *
            2f *
            camera.orthographicSize;

        camera.transform.position = new Vector3(
            levelCenterX,
            cameraY,
            -10f
        );
    }
}