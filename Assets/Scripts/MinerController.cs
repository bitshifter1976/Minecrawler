using UnityEngine;
using UnityEngine.InputSystem;

public sealed class MinerController : GridActor
{
    private MineControlsGenerated controls;

    private Vector2Int moveDirection = Vector2Int.right;
    private float moveTimer;

    private void Awake()
    {
        controls = new MineControlsGenerated();
        moveDirection = Vector2Int.right;
        UpdateRotation();
    }

    private void OnEnable()
    {
        controls.Gameplay.Move.performed += OnMove;
        controls.Gameplay.Enable();

        moveTimer = 0f;
    }

    private void OnDisable()
    {
        if (controls == null)
            return;

        controls.Gameplay.Move.performed -= OnMove;
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

        game.TryMoveMiner(moveDirection);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        MineGameManager game = MineGameManager.Instance;
        game?.RequestLevelStart();
        Vector2 input = context.ReadValue<Vector2>();
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            moveDirection = input.x > 0f
                ? Vector2Int.right
                : Vector2Int.left;
        }
        else if (Mathf.Abs(input.y) > 0.01f)
        {
            moveDirection = input.y > 0f
                ? Vector2Int.up
                : Vector2Int.down;
        }
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
}