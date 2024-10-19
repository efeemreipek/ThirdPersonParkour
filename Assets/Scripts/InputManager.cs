using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    private GameControls gameControls;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction dropAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    protected override void Awake()
    {
        base.Awake();

        gameControls = new GameControls();
    }
    private void OnEnable()
    {
        moveAction = gameControls.Player.Move;
        lookAction = gameControls.Player.Look;
        jumpAction = gameControls.Player.Jump;
        dropAction = gameControls.Player.Drop;
        sprintAction = gameControls.Player.Sprint;
        crouchAction = gameControls.Player.Crouch;

        gameControls.Enable();
    }
    private void OnDisable()
    {
        gameControls.Disable();
    }

    public Vector2 MoveInput => moveAction.ReadValue<Vector2>();
    public Vector2 LookInput => lookAction.ReadValue<Vector2>();
    public bool JumpInput => jumpAction.WasPressedThisFrame();
    public bool DropInput => dropAction.WasPressedThisFrame();
    public bool SprintInput => sprintAction.IsPressed();
    public bool CrouchInput => crouchAction.WasPressedThisFrame();
}
