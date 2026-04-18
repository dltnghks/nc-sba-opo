using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PaddleMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float minX = -7f;
    [SerializeField] private float maxX = 7f;

    private InputAction moveAction;

    private void Awake()
    {
        moveAction = new InputAction(name: "Move", type: InputActionType.Value);
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/leftStick/x");
        moveAction.AddBinding("<Gamepad>/dpad/x");
    }

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
    }

    private void Update()
    {
        float input = moveAction.ReadValue<float>();
        float nextX = transform.position.x + (input * moveSpeed * Time.deltaTime);
        float clampedX = Mathf.Clamp(nextX, minX, maxX);

        Vector3 position = transform.position;
        position.x = clampedX;
        transform.position = position;
    }
}
