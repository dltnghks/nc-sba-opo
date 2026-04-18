using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PrototypeBallController : MonoBehaviour
{
    [SerializeField] private PaddleMovement paddle;
    [SerializeField] private Vector3 restingOffset = new(0f, 0.75f, 1.25f);
    [SerializeField] private Vector3 launchDirection = new(0.25f, 0f, 1f);
    [SerializeField] private float moveSpeed = 9f;

    private InputAction launchAction;
    private Vector3 velocity;
    private bool isLaunched;

    private void Awake()
    {
        launchAction = new InputAction(name: "Launch", type: InputActionType.Button);
        launchAction.AddBinding("<Keyboard>/space");
        launchAction.AddBinding("<Mouse>/leftButton");
        launchAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void OnEnable()
    {
        launchAction.Enable();
    }

    private void OnDisable()
    {
        launchAction.Disable();
    }

    private void OnDestroy()
    {
        launchAction.Dispose();
    }

    private void Start()
    {
        SnapToPaddle();
    }

    private void Update()
    {
        if (paddle == null)
        {
            return;
        }

        if (!isLaunched)
        {
            SnapToPaddle();

            if (launchAction.WasPressedThisFrame())
            {
                Launch();
            }

            return;
        }

        transform.position += velocity * Time.deltaTime;
    }

    public void SetPaddle(PaddleMovement targetPaddle)
    {
        paddle = targetPaddle;
        isLaunched = false;
        velocity = Vector3.zero;
        SnapToPaddle();
    }

    private void Launch()
    {
        isLaunched = true;
        velocity = launchDirection.normalized * moveSpeed;
    }

    private void SnapToPaddle()
    {
        transform.position = paddle.transform.position + restingOffset;
    }
}
