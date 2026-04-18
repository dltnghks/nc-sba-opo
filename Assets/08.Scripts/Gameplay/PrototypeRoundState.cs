using UnityEngine;

public sealed class PrototypeRoundState : MonoBehaviour
{
    [SerializeField] private StringEventChannelSO roundEndedEvent;
    [SerializeField] private bool roundEnded;

    public bool IsRoundEnded => roundEnded;

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

            Debug.Log($"Life Lost. Remaining Lives: {sessionState.CurrentLives}");
            return;
        }

        EndRound("Fail");
    }

    private void EndRound(string result)
    {
        roundEnded = true;
        PrototypeSessionState sessionState = FindAnyObjectByType<PrototypeSessionState>();
        string sessionSummary = sessionState != null
            ? $" | Score: {sessionState.Score} | Lives: {sessionState.CurrentLives}"
            : string.Empty;
        Debug.Log($"Round Result: {result}{sessionSummary}");
        roundEndedEvent?.RaiseEvent(result);
    }

    public void Configure(StringEventChannelSO roundEndedEventChannel)
    {
        roundEndedEvent = roundEndedEventChannel;
    }
}
