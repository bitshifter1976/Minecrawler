using UnityEngine;
using UnityEngine.InputSystem;

public sealed class MinerController : GridActor
{
    private MineControls controls;

    private Vector2Int moveDirection = Vector2Int.right;
    private float moveTimer;
    private MinerDustTrail dustTrail;
    private MinerLocomotiveAudio movementSound;

    private void Awake()
    {
        controls = new MineControls();
        moveDirection = Vector2Int.right;
        UpdateRotation();

        dustTrail =
            GetComponent<MinerDustTrail>();

        if (dustTrail == null)
            dustTrail = gameObject.AddComponent<MinerDustTrail>();

        gameObject.AddComponent<AudioSource>();
        if (movementSound == null)
            movementSound = gameObject.AddComponent<MinerLocomotiveAudio>();
    }

    private void OnEnable()
    {
        controls.Gameplay.Move.performed += OnMove;
        controls.Gameplay.Click.performed += OnPointerPressed;
        controls.Gameplay.Enable();

        moveTimer = 0f;
    }

    private void OnDisable()
    {
        if (controls == null)
            return;

        controls.Gameplay.Move.performed -= OnMove;
        controls.Gameplay.Click.performed -= OnPointerPressed;
        controls.Gameplay.Disable();
    }

    private void OnDestroy()
    {
        controls?.Dispose();
    }

    private void Update()
    {
        MineGameManager game = MineGameManager.Instance;

        if (game == null || !game.IsPlaying)
            return;

        moveTimer += Time.deltaTime;
        if (moveTimer < game.AutomaticMoveInterval)
            return;
        moveTimer -= game.AutomaticMoveInterval;

        Vector3 previousPosition = transform.position;
        if (game.TryMoveMiner(moveDirection) && transform.position != previousPosition)
        {
            dustTrail?.EmitMovementDust(previousPosition, moveDirection);
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        MineGameManager game = MineGameManager.Instance;
        game?.RequestLevelStart();
        Vector2 input = context.ReadValue<Vector2>();
        Vector2Int newDirection = Vector2Int.zero;
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            newDirection = input.x > 0f
                ? Vector2Int.right
                : Vector2Int.left;
        }
        else if (Mathf.Abs(input.y) > 0.01f)
        {
            newDirection = input.y > 0f
                ? Vector2Int.up
                : Vector2Int.down;
        }
        if (!IsOppositeDirection(newDirection))
        {
            moveDirection = newDirection;
            UpdateRotation();
        }
    }
    private void OnPointerPressed(InputAction.CallbackContext context)
    {
        Vector2 screenPosition =
            controls.Gameplay.Point.ReadValue<Vector2>();

        Camera camera = Camera.main;

        if (camera == null)
            return;

        var worldPosition = camera.ScreenToWorldPoint(screenPosition);
        var minerPosition = transform.position;
        var difference = new Vector2(worldPosition.x - minerPosition.x, worldPosition.y - minerPosition.y);
        Vector2Int newDirection = Vector2Int.zero;
        if (Mathf.Abs(difference.x) > Mathf.Abs(difference.y))
        {
            newDirection = difference.x > 0f
                ? Vector2Int.right
                : Vector2Int.left;
        }
        else
        {
            newDirection = difference.y > 0f
                ? Vector2Int.up
                : Vector2Int.down;
        }

        if (!IsOppositeDirection(newDirection))
        {
            moveDirection = newDirection;
            UpdateRotation();
        }

    }


    public void Bounce()
    {
        moveDirection = -moveDirection;
        moveTimer = 0f;
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        float angle = moveDirection switch
        {
            var d when d == Vector2Int.up => 180f,
            var d when d == Vector2Int.right => 90f,
            var d when d == Vector2Int.down => 0f,
            var d when d == Vector2Int.left => -90f,
            _ => 0f
        };
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    private bool IsOppositeDirection(Vector2Int newDirection)
    {
        return newDirection == -moveDirection;
    }
}