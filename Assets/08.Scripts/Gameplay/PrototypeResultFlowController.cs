using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class PrototypeResultFlowController : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = ProjectSceneNames.Gameplay;
    [SerializeField] private string bootstrapSceneName = ProjectSceneNames.Bootstrap;
    [SerializeField] private StringEventChannelSO roundEndedEvent;

    private InputAction restartAction;
    private InputAction backToBootstrapAction;
    private bool roundEnded;

    private void Awake()
    {
        restartAction = new InputAction(name: "Restart", type: InputActionType.Button);
        restartAction.AddBinding("<Keyboard>/r");
        restartAction.AddBinding("<Keyboard>/enter");
        restartAction.AddBinding("<Gamepad>/start");

        backToBootstrapAction = new InputAction(name: "BackToBootstrap", type: InputActionType.Button);
        backToBootstrapAction.AddBinding("<Keyboard>/escape");
        backToBootstrapAction.AddBinding("<Keyboard>/b");
        backToBootstrapAction.AddBinding("<Gamepad>/select");
    }

    private void OnEnable()
    {
        restartAction.Enable();
        backToBootstrapAction.Enable();
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        restartAction.Disable();
        backToBootstrapAction.Disable();
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        restartAction.Dispose();
        backToBootstrapAction.Dispose();
    }

    private void Update()
    {
        if (!roundEnded)
        {
            PrototypeRoundState roundState = FindAnyObjectByType<PrototypeRoundState>();
            if (roundState != null && roundState.IsRoundEnded)
            {
                roundEnded = true;
            }
        }

        if (!roundEnded)
        {
            return;
        }

        if (restartAction.WasPressedThisFrame())
        {
            SceneManager.LoadScene(gameplaySceneName);
            return;
        }

        if (backToBootstrapAction.WasPressedThisFrame())
        {
            SceneManager.LoadScene(bootstrapSceneName);
        }
    }

    public void Configure(StringEventChannelSO roundEndedEventChannel)
    {
        UnsubscribeFromEvents();
        roundEndedEvent = roundEndedEventChannel;

        if (isActiveAndEnabled)
        {
            SubscribeToEvents();
        }
    }

    private void HandleRoundEnded(string _)
    {
        roundEnded = true;
    }

    private void SubscribeToEvents()
    {
        UnsubscribeFromEvents();

        if (roundEndedEvent != null)
        {
            roundEndedEvent.OnEventRaised += HandleRoundEnded;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (roundEndedEvent != null)
        {
            roundEndedEvent.OnEventRaised -= HandleRoundEnded;
        }
    }
}
