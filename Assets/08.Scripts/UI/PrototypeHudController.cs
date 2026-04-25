using TMPro;
using UnityEngine;

public sealed class PrototypeHudController : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO scoreChangedEvent;
    [SerializeField] private IntEventChannelSO livesChangedEvent;
    [SerializeField] private StringEventChannelSO roundEndedEvent;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text hintText;
    private bool resultShown;

    private void Start()
    {
        ApplyInitialVisibility();
        RefreshFromSessionState();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        if (resultShown)
        {
            return;
        }

        PrototypeRoundState roundState = FindAnyObjectByType<PrototypeRoundState>();
        if (roundState != null && roundState.IsRoundEnded)
        {
            HandleRoundEnded(roundState.LastResult);
        }
    }

    public void Configure(
        IntEventChannelSO scoreEventChannel,
        IntEventChannelSO livesEventChannel,
        StringEventChannelSO roundEndedEventChannel)
    {
        UnsubscribeFromEvents();
        scoreChangedEvent = scoreEventChannel;
        livesChangedEvent = livesEventChannel;
        roundEndedEvent = roundEndedEventChannel;

        if (isActiveAndEnabled)
        {
            SubscribeToEvents();
            RefreshFromSessionState();
        }
    }

    private void SubscribeToEvents()
    {
        UnsubscribeFromEvents();

        if (scoreChangedEvent != null)
        {
            scoreChangedEvent.OnEventRaised += HandleScoreChanged;
        }

        if (livesChangedEvent != null)
        {
            livesChangedEvent.OnEventRaised += HandleLivesChanged;
        }

        if (roundEndedEvent != null)
        {
            roundEndedEvent.OnEventRaised += HandleRoundEnded;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (scoreChangedEvent != null)
        {
            scoreChangedEvent.OnEventRaised -= HandleScoreChanged;
        }

        if (livesChangedEvent != null)
        {
            livesChangedEvent.OnEventRaised -= HandleLivesChanged;
        }

        if (roundEndedEvent != null)
        {
            roundEndedEvent.OnEventRaised -= HandleRoundEnded;
        }
    }

    private void HandleScoreChanged(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score {score:0000}";
        }
    }

    private void HandleLivesChanged(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives {Mathf.Max(lives, 0)}";
        }
    }

    private void HandleRoundEnded(string result)
    {
        resultShown = true;
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = result == "Clear" ? "Stage Clear" : "Round Failed";
        }

        if (hintText != null)
        {
            hintText.gameObject.SetActive(true);
            hintText.text = "R Restart   Esc Bootstrap";
        }
    }

    private void RefreshFromSessionState()
    {
        PrototypeSessionState sessionState = FindAnyObjectByType<PrototypeSessionState>();
        if (sessionState != null)
        {
            HandleScoreChanged(sessionState.Score);
            HandleLivesChanged(sessionState.CurrentLives);
        }

        PrototypeRoundState roundState = FindAnyObjectByType<PrototypeRoundState>();
        if (roundState != null && roundState.IsRoundEnded)
        {
            HandleRoundEnded(roundState.LastResult);
        }
    }

    private void ApplyInitialVisibility()
    {
        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }

        if (hintText != null)
        {
            hintText.gameObject.SetActive(false);
        }
    }
}
