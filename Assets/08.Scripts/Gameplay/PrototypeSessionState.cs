using UnityEngine;

public sealed class PrototypeSessionState : MonoBehaviour
{
    [SerializeField] private int startingLives = 3;
    [SerializeField] private IntEventChannelSO scoreChangedEvent;
    [SerializeField] private IntEventChannelSO livesChangedEvent;
    [SerializeField] private int currentLives;
    [SerializeField] private int score;

    public int CurrentLives => currentLives;
    public int Score => score;

    private void Awake()
    {
        if (FindObjectsByType<PrototypeSessionState>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        currentLives = Mathf.Max(startingLives, 1);
        livesChangedEvent?.RaiseEvent(currentLives);
        scoreChangedEvent?.RaiseEvent(score);
    }

    public void AddScore(int amount)
    {
        score += Mathf.Max(amount, 0);
        scoreChangedEvent?.RaiseEvent(score);
    }

    public bool TryConsumeLife()
    {
        currentLives = Mathf.Max(currentLives - 1, 0);
        livesChangedEvent?.RaiseEvent(currentLives);
        return currentLives > 0;
    }

    public void Configure(IntEventChannelSO scoreEventChannel, IntEventChannelSO livesEventChannel)
    {
        scoreChangedEvent = scoreEventChannel;
        livesChangedEvent = livesEventChannel;
    }
}
