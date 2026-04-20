using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = ProjectSceneNames.Gameplay;

    private InputAction startAction;

    private void Awake()
    {
        startAction = new InputAction(name: "StartGame", type: InputActionType.Button);
        startAction.AddBinding("<Keyboard>/space");
        startAction.AddBinding("<Keyboard>/enter");
        startAction.AddBinding("<Mouse>/leftButton");
        startAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void OnEnable()
    {
        startAction.Enable();
    }

    private void OnDisable()
    {
        startAction.Disable();
    }

    private void OnDestroy()
    {
        startAction.Dispose();
    }

    private void Start()
    {
        BuildStartScreen();
    }

    private void Update()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("BootstrapLoader requires a gameplay scene name.", this);
            return;
        }

        if (startAction.WasPressedThisFrame())
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    private void BuildStartScreen()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        CreateText(
            "TitleText",
            font,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 48f),
            new Vector2(720f, 80f),
            44,
            TextAnchor.MiddleCenter,
            "NC SBA OPO");

        CreateText(
            "SubtitleText",
            font,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -12f),
            new Vector2(760f, 48f),
            22,
            TextAnchor.MiddleCenter,
            "Single-player core milestone build");

        CreateText(
            "StartHintText",
            font,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 64f),
            new Vector2(760f, 48f),
            24,
            TextAnchor.MiddleCenter,
            "Press Space, Enter, Click, or A to start");
    }

    private void CreateText(
        string objectName,
        Font font,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize,
        TextAnchor alignment,
        string content)
    {
        GameObject textObject = new(objectName);
        textObject.transform.SetParent(transform, false);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = content;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
    }
}
