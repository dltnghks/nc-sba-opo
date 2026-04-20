using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public sealed class PrototypeBallController : MonoBehaviour
{
    [SerializeField] private PaddleMovement paddle;
    [SerializeField] private Vector3 restingOffset = new(0f, 0.75f, 1.25f);
    [SerializeField] private Vector3 launchDirection = new(0.25f, 0f, 1f);
    [SerializeField] private float moveSpeed = 9f;
    [SerializeField] private float collisionSkin = 0.01f;

    private InputAction launchAction;
    private Vector3 velocity;
    private bool isLaunched;
    private SphereCollider sphereCollider;
    private float fixedY;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
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
        fixedY = transform.position.y;
        SnapToPaddle();
    }

    private void Update()
    {
        PrototypeRoundState roundState = FindAnyObjectByType<PrototypeRoundState>();
        if (roundState != null && roundState.IsRoundEnded)
        {
            return;
        }

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

        AdvanceBall(Time.deltaTime);
    }

    public void SetPaddle(PaddleMovement targetPaddle)
    {
        paddle = targetPaddle;
        isLaunched = false;
        velocity = Vector3.zero;
        SnapToPaddle();
    }

    public void ResetToPaddle()
    {
        isLaunched = false;
        velocity = Vector3.zero;
        SnapToPaddle();
    }

    private void Launch()
    {
        isLaunched = true;
        velocity = FlattenDirection(launchDirection) * moveSpeed;
    }

    private void AdvanceBall(float deltaTime)
    {
        float remainingDistance = velocity.magnitude * deltaTime;
        Vector3 direction = FlattenDirection(velocity);
        Vector3 position = transform.position;
        float radius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

        for (int i = 0; i < 3 && remainingDistance > 0f; i++)
        {
            if (!Physics.SphereCast(position, radius, direction, out RaycastHit hit, remainingDistance + collisionSkin, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            {
                position += direction * remainingDistance;
                remainingDistance = 0f;
                break;
            }

            float travelDistance = Mathf.Max(hit.distance - collisionSkin, 0f);
            position += direction * travelDistance;
            remainingDistance -= travelDistance;

            if (HandleHit(hit))
            {
                position.y = fixedY;
                transform.position = position;
                return;
            }

            direction = FlattenDirection(Vector3.Reflect(direction, hit.normal));
            velocity = direction * moveSpeed;
            position += hit.normal * collisionSkin;
            position.y = fixedY;
            remainingDistance = Mathf.Max(remainingDistance - collisionSkin, 0f);
        }

        position.y = fixedY;
        transform.position = position;
    }

    private bool HandleHit(in RaycastHit hit)
    {
        if (hit.collider.TryGetComponent(out PrototypeFailZone failZone))
        {
            failZone.TriggerFail(this);
            return true;
        }

        if (hit.collider.TryGetComponent(out PaddleMovement _))
        {
            PrototypeAudioManager audioManager = FindAnyObjectByType<PrototypeAudioManager>();
            if (audioManager != null)
            {
                audioManager.PlayPaddleHit();
            }
        }

        if (hit.collider.TryGetComponent(out PrototypeBrick brick))
        {
            bool destroyed = brick.ApplyHit();
            PrototypeAudioManager audioManager = FindAnyObjectByType<PrototypeAudioManager>();
            if (audioManager != null)
            {
                audioManager.PlayBrickHit(destroyed);
            }

            if (destroyed)
            {
                PrototypeSessionState sessionState = FindAnyObjectByType<PrototypeSessionState>();
                if (sessionState != null)
                {
                    sessionState.AddScore(brick.ScoreValue);
                }
            }
        }

        return false;
    }

    private void SnapToPaddle()
    {
        Vector3 snappedPosition = paddle.transform.position + restingOffset;
        snappedPosition.y = fixedY;
        transform.position = snappedPosition;
    }

    private static Vector3 FlattenDirection(Vector3 direction)
    {
        Vector3 flattened = Vector3.ProjectOnPlane(direction, Vector3.up);
        if (flattened.sqrMagnitude <= Mathf.Epsilon)
        {
            flattened = Vector3.forward;
        }

        return flattened.normalized;
    }
}
