using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public sealed class MineGameManager : MonoBehaviour
{
    private IDisposable startInputSubscription;
    private const int LevelCount = 100;
    private const float LevelCompleteDelay = 1.5f;

    private MineControls controls;
    private MineBoard board;
    private MineTail tail;

    private Coroutine levelTransitionCoroutine;
    private AudioSource audioSource;
    private AudioListener audioListener;
    private int currentLevelIndex;
    private int score;
    private int collected;
    private int moves;

    private string message = "Collect all coal chunks.";
    private bool exitingLevel;
    private Vector2Int exitDirection;

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

        controls = new MineControls();
        audioSource = FindAnyObjectByType<AudioSource>();

        tail = new MineTail();
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

        board = new MineBoard(transform, fallbackSprite, wallSprite, rockSprite, coalSprite, floorSprite, doorClosedSprite, doorOpenSprite, cartSprite, minerSprite);

        currentLevelIndex = 0;
        if (PlayerPrefs.GetInt("ContinueGame") == 1 && PlayerPrefs.HasKey("CurrentLevel"))
        {
            currentLevelIndex = PlayerPrefs.GetInt("CurrentLevel", 0);
        }
        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, LevelCount - 1);

        LoadLevel(currentLevelIndex);
        SetupCamera();
    }

    private void OnEnable()
    {
        controls.Gameplay.Restart.performed += OnRestart;
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
        controls.Gameplay.Restart.performed -= OnRestart;
    }

    private void OnDestroy()
    {
        StopLevelTransition();
        DisposeLevelStartInput();

        controls?.Dispose();

        if (Instance == this)
            Instance = null;
    }

    private void OnRestart(InputAction.CallbackContext context)
    {
        if (State == GameState.LevelReady)
        {
            StartLevel();
            return;
        }

        RestartLevel();
    }

    private void ArmLevelStartInput()
    {
        DisposeLevelStartInput();

        startInputSubscription =
            InputSystem.onAnyButtonPress.Call(control =>
            {
                if (control.device is Keyboard ||
                    control.device is Gamepad)
                {
                    StartLevel();
                }
            });
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
        message = "Collect all coal and destroy all breakable rocks. The exit will open afterwards.";
        audioSource.clip = Resources.Load<AudioClip>("Audio/ambience");
        audioSource.loop = true;
        audioSource.Play();
    }

    private void DisposeLevelStartInput()
    {
        startInputSubscription?.Dispose();
        startInputSubscription = null;
    }

    public void RestartLevel()
    {
        switch (State)
        {
            case GameState.Loading:
                return;

            case GameState.Victory:
                StartNewGame();
                break;

            default:
                LoadLevel(currentLevelIndex);
                break;
        }
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

        State = GameState.Paused;
        Time.timeScale = 0f;

        message = "Game paused.";
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused)
            return;

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
        State = GameState.Loading;
        Time.timeScale = 1f;

        StopLevelTransition();
        currentLevelIndex = Mathf.Clamp(levelIndex, 0, LevelCount - 1);
        exitingLevel = false;
        exitDirection = Vector2Int.zero;
        tail.Clear();

        MineLevelData level = MineLevelLoader.Load(currentLevelIndex);

        if (level == null)
        {
            TriggerGameOver($"Failed to load level {currentLevelIndex + 1}.");
            return;
        }

        if (!board.Build(level))
        {
            TriggerGameOver($"Level {currentLevelIndex + 1} is invalid.");
            return;
        }

        State = GameState.LevelReady;
        message = "Collect all coal and destroy all breakable rocks. The exit will open afterwards.";
        ArmLevelStartInput();
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

        if (tail.Contains(targetPosition))
        {
            TriggerGameOver("You crashed into your own tail!");
            return false;
        }

        var obstacle = board.GetObstacle(targetPosition);
        if (obstacle != null)
        {
            audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/destroy"), 0.25f);
            board.RemoveObstacle(obstacle);
            score += 25;
            moves++;
            CheckLevelCleared();
            return true;
        }

        Vector2Int newTailPosition = tail.GetNewSegmentPosition(currentPosition);
        board.Miner.SetGridPosition(targetPosition);

        tail.Move(currentPosition);

        moves++;

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

        audioSource.Stop();
        audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/gameOver"));
        State = GameState.GameOver;
        message = reason + "\n\nPress Restart.";
        DisposeLevelStartInput();
        State = GameState.GameOver;
        message = reason + " Press R to restart.";

        Debug.Log($"Game Over: {reason}");
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
        tail.Move(previousMinerPosition);

        moves++;
        exitingLevel = true;
        message = "Leaving the mine...";

        return true;
    }

    private void MoveThroughExit(Vector2Int currentMinerPosition)
    {
        Vector2Int nextMinerPosition = currentMinerPosition + exitDirection;

        board.Miner.SetGridPosition(nextMinerPosition);
        tail.Move(currentMinerPosition);
        tail.RemoveOutside(board.Width, board.Height);

        moves++;

        bool minerIsOutside = !board.IsInside(board.Miner.GridPosition);

        if (minerIsOutside)
        {
            SpriteRenderer minerRenderer =
                board.Miner.GetComponent<SpriteRenderer>();

            if (minerRenderer != null)
                minerRenderer.enabled = false;
        }

        if (minerIsOutside && tail.IsEmpty)
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
        tail.Add(segment);

        CheckLevelCleared();
    }

    private void OpenExit()
    {
        board.OpenExit();
        message = "All coal collected - the exit is now open!";
    }

    private void CheckLevelCleared()
    {
        if (board.IsLevelCleared)
        {
            OpenExit();
            return;
        }

        message = $"{board.RemainingCoal} coal and {board.RemainingObstacles} rocks remaining.";
    }

    private void CompleteLevel()
    {
        if (State != GameState.Playing)
            return;

        audioSource.Stop();
        audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/victory"));
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
        PlayerPrefs.SetInt("CurrentLevel", 0);
        PlayerPrefs.Save();
        message = "Congratulations! You completed all 100 levels. Press R to start a new game.";
    }

    private void SaveCurrentLevel()
    {
        PlayerPrefs.SetInt("CurrentLevel", currentLevelIndex);
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
        }

        camera.orthographic = true;
        camera.backgroundColor = new Color(0.06f, 0.05f, 0.05f);

        UpdateCamera();
    }

    private void UpdateCamera()
    {
        Camera camera = Camera.main;

        if (camera == null || board.Width <= 0 || board.Height <= 0)
        {
            return;
        }

        camera.transform.position = new Vector3((board.Width - 1) * 0.5f, (board.Height - 1) * 0.5f, -10f);
        float horizontalSize = board.Width / (2f * camera.aspect);
        float verticalSize = board.Height / 2f;
        camera.orthographicSize = Mathf.Max(horizontalSize, verticalSize) + 0.8f;
    }
}