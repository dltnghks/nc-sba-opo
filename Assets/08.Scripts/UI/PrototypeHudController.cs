using UnityEngine;
using UnityEngine.UI;

public sealed class PrototypeHudController : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO scoreChangedEvent;
    [SerializeField] private IntEventChannelSO livesChangedEvent;
    [SerializeField] private StringEventChannelSO roundEndedEvent;

    private Text scoreText;
    private Text livesText;
    private Text statusText;
    private Text hintText;
    private bool hudBuilt;
    private bool resultShown;

    private void Start()
    {
        EnsureHud();
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
        EnsureHud();
        scoreText.text = $"Score {score:0000}";
    }

    private void HandleLivesChanged(int lives)
    {
        EnsureHud();
        livesText.text = $"Lives {Mathf.Max(lives, 0)}";
    }

    private void HandleRoundEnded(string result)
    {
        EnsureHud();
        resultShown = true;
        statusText.gameObject.SetActive(true);
        hintText.gameObject.SetActive(true);
        statusText.text = result == "Clear" ? "Stage Clear" : "Round Failed";
        hintText.text = "R Restart   Esc Bootstrap";
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

    private void EnsureHud()
    {
        if (hudBuilt)
        {
            return;
        }

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        CreateBackdrop(transform);

        scoreText = CreateText(
            "ScoreText",
            transform,
            font,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(24f, -24f),
            28,
            TextAnchor.UpperLeft);

        livesText = CreateText(
            "LivesText",
            transform,
            font,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-24f, -24f),
            28,
            TextAnchor.UpperRight);

        statusText = CreateText(
            "StatusText",
            transform,
            font,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 24f),
            42,
            TextAnchor.MiddleCenter);

        hintText = CreateText(
            "HintText",
            transform,
            font,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -24f),
            24,
            TextAnchor.MiddleCenter);

        statusText.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
        hudBuilt = true;
    }

    private static void CreateBackdrop(Transform parent)
    {
        GameObject backdrop = new("Backdrop");
        backdrop.transform.SetParent(parent, false);

        Image image = backdrop.AddComponent<Image>();
        image.color = new Color(0.05f, 0.07f, 0.11f, 0.18f);

        RectTransform rectTransform = backdrop.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static Text CreateText(
        string objectName,
        Transform parent,
        Font font,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        int fontSize,
        TextAnchor alignment)
    {
        GameObject textObject = new(objectName);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(520f, 80f);

        return text;
    }
}
