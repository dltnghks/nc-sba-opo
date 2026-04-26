using UnityEngine;
using UnityEngine.Serialization;

public sealed class PrototypeRoundState : MonoBehaviour
{
    [SerializeField] private StringEventChannelSO roundEndedEvent;
    [FormerlySerializedAs("roundEnded")]
    [SerializeField] private bool cachedRoundEnded;
    [FormerlySerializedAs("lastResult")]
    [SerializeField] private string cachedLastResult;

    private NetworkGameplayState networkState;

    public bool IsRoundEnded => TryResolveNetworkState() ? networkState.IsRoundEnded : cachedRoundEnded;
    public string LastResult => TryResolveNetworkState() ? networkState.RoundResult : cachedLastResult;

    private void Awake()
    {
        if (FindObjectsByType<PrototypeRoundState>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (TryResolveNetworkState())
        {
            SyncFromNetworkState();
            if (!networkState.HasStateAuthority)
            {
                return;
            }
        }

        if (IsRoundEnded)
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
        if (IsRoundEnded)
        {
            return;
        }

        PrototypeSessionState sessionState = FindAnyObjectByType<PrototypeSessionState>();
        if (TryResolveNetworkState() && !networkState.HasStateAuthority)
        {
            sessionState?.TryConsumeLife();
            if (ball != null && sessionState != null && sessionState.CurrentLives > 1)
            {
                ball.ResetToPaddle();
            }

            return;
        }

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
        if (TryResolveNetworkState())
        {
            networkState.EndRound(result);
        }

        ApplyRoundResult(result, true);
    }

    private void ApplyRoundResult(string result, bool playAudio)
    {
        if (cachedRoundEnded)
        {
            return;
        }

        cachedRoundEnded = true;
        cachedLastResult = result;
        PrototypeSessionState sessionState = FindAnyObjectByType<PrototypeSessionState>();
        string sessionSummary = sessionState != null
            ? $" | Score: {sessionState.Score} | Lives: {sessionState.CurrentLives}"
            : string.Empty;

        if (playAudio)
        {
            PrototypeAudioManager audioManager = FindAnyObjectByType<PrototypeAudioManager>();
            if (audioManager != null)
            {
                audioManager.PlayRoundEnd(result == "Clear");
            }
        }

        Debug.Log($"Round Result: {result}{sessionSummary}");
        roundEndedEvent?.RaiseEvent(result);
    }

    public void Configure(StringEventChannelSO roundEndedEventChannel)
    {
        roundEndedEvent = roundEndedEventChannel;
        TryResolveNetworkState();
        SyncFromNetworkState();
    }

    private bool TryResolveNetworkState()
    {
        if (networkState != null)
        {
            return true;
        }

        networkState = NetworkGameplayState.Instance;
        return networkState != null;
    }

    private void SyncFromNetworkState()
    {
        if (networkState == null || !networkState.IsRoundEnded)
        {
            return;
        }

        string result = networkState.RoundResult;
        ApplyRoundResult(string.IsNullOrWhiteSpace(result) ? "Unknown" : result, false);
    }
}
