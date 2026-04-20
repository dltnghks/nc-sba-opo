using UnityEngine;

public sealed class PrototypeRoundState : MonoBehaviour
{
    [SerializeField] private StringEventChannelSO roundEndedEvent;
    [SerializeField] private bool roundEnded;
    [SerializeField] private string lastResult;

    public bool IsRoundEnded => roundEnded;
    public string LastResult => lastResult;

    private void Awake()
    {
        if (FindObjectsByType<PrototypeRoundState>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (roundEnded)
        {
            return;
        }

        if (FindObjectsByType<PrototypeBrick>(FindObjectsSortMode.None).Length == 0)
        {
            EndRound("Clear");
        }
    }

    public void HandleBallLost(PrototypeBallController ball)
    {
        if (roundEnded)
        {
            return;
        }

        PrototypeSessionState sessionState = FindAnyObjectByType<PrototypeSessionState>();
        if (sessionState != null && sessionState.TryConsumeLife())
        {
            if (ball != null)
            {
                ball.ResetToPaddle();
            }

            PrototypeAudioManager audioManager = FindAnyObjectByType<PrototypeAudioManager>();
            if (audioManager != null)
            {
                audioManager.PlayLifeLost();
            }

            Debug.Log($"Life Lost. Remaining Lives: {sessionState.CurrentLives}");
            return;
        }

        EndRound("Fail");
    }

    private void EndRound(string result)
    {
        roundEnded = true;
        lastResult = result;
        PrototypeSessionState sessionState = FindAnyObjectByType<PrototypeSessionState>();
        string sessionSummary = sessionState != null
            ? $" | Score: {sessionState.Score} | Lives: {sessionState.CurrentLives}"
            : string.Empty;

        PrototypeAudioManager audioManager = FindAnyObjectByType<PrototypeAudioManager>();
        if (audioManager != null)
        {
            audioManager.PlayRoundEnd(result == "Clear");
        }

        Debug.Log($"Round Result: {result}{sessionSummary}");
        roundEndedEvent?.RaiseEvent(result);
    }

    public void Configure(StringEventChannelSO roundEndedEventChannel)
    {
        roundEndedEvent = roundEndedEventChannel;
    }
}
