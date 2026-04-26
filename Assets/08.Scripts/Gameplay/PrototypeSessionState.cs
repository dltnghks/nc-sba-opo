using UnityEngine;
using UnityEngine.Serialization;

public sealed class PrototypeSessionState : MonoBehaviour
{
    [SerializeField] private int startingLives = 3;
    [SerializeField] private IntEventChannelSO scoreChangedEvent;
    [SerializeField] private IntEventChannelSO livesChangedEvent;
    [FormerlySerializedAs("currentLives")]
    [SerializeField] private int cachedLives;
    [FormerlySerializedAs("score")]
    [SerializeField] private int cachedScore;

    private NetworkGameplayState networkState;

    public int CurrentLives => TryResolveNetworkState() ? networkState.Lives : cachedLives;
    public int Score => TryResolveNetworkState() ? networkState.Score : cachedScore;

    private void Awake()
    {
        if (FindObjectsByType<PrototypeSessionState>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        cachedLives = Mathf.Max(startingLives, 1);
        livesChangedEvent?.RaiseEvent(cachedLives);
        scoreChangedEvent?.RaiseEvent(cachedScore);
    }

    private void Update()
    {
        if (!TryResolveNetworkState())
        {
            return;
        }

        SyncFromNetworkState();
    }

    public void AddScore(int amount)
    {
        if (TryResolveNetworkState())
        {
            networkState.AddScore(amount);
            SyncFromNetworkState();
            return;
        }

        cachedScore += Mathf.Max(amount, 0);
        scoreChangedEvent?.RaiseEvent(cachedScore);
    }

    public bool TryConsumeLife()
    {
        if (TryResolveNetworkState())
        {
            bool hasLivesRemaining = networkState.TryConsumeLife();
            SyncFromNetworkState();
            return hasLivesRemaining;
        }

        cachedLives = Mathf.Max(cachedLives - 1, 0);
        livesChangedEvent?.RaiseEvent(cachedLives);
        return cachedLives > 0;
    }

    public void Configure(IntEventChannelSO scoreEventChannel, IntEventChannelSO livesEventChannel)
    {
        scoreChangedEvent = scoreEventChannel;
        livesChangedEvent = livesEventChannel;
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
        if (networkState == null)
        {
            return false;
        }

        if (networkState.HasStateAuthority && networkState.Lives <= 0)
        {
            networkState.InitializeRound(Mathf.Max(startingLives, 1));
        }

        return true;
    }

    private void SyncFromNetworkState()
    {
        if (networkState == null)
        {
            return;
        }

        if (cachedScore != networkState.Score)
        {
            cachedScore = networkState.Score;
            scoreChangedEvent?.RaiseEvent(cachedScore);
        }

        if (cachedLives != networkState.Lives)
        {
            cachedLives = networkState.Lives;
            livesChangedEvent?.RaiseEvent(cachedLives);
        }
    }
}
