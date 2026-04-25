using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = ProjectSceneNames.Gameplay;
    [SerializeField] private string defaultClientAddress = "127.0.0.1";
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        RuntimeNetworkManager.Instance.SetAddress(defaultClientAddress);
        RefreshStatus();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
            {
                HandleSinglePlayerPressed();
            }
            else if (keyboard.hKey.wasPressedThisFrame)
            {
                HandleCreateRoomPressed();
            }
            else if (keyboard.jKey.wasPressedThisFrame || keyboard.cKey.wasPressedThisFrame)
            {
                HandleJoinRoomPressed();
            }
            else if (keyboard.xKey.wasPressedThisFrame)
            {
                HandleShutdownPressed();
            }
        }

        RefreshStatus();
    }

    private void HandleSinglePlayerPressed()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("BootstrapLoader requires a gameplay scene name.", this);
            return;
        }

        RuntimeNetworkManager.Instance.Shutdown();
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void HandleCreateRoomPressed()
    {
        RuntimeNetworkManager.Instance.SetAddress(defaultClientAddress);
        RuntimeNetworkManager.Instance.CreateRoom();
        RefreshStatus();
    }

    private void HandleJoinRoomPressed()
    {
        RuntimeNetworkManager.Instance.SetAddress(defaultClientAddress);
        RuntimeNetworkManager.Instance.JoinSavedRoom();
        RefreshStatus();
    }

    private void HandleShutdownPressed()
    {
        RuntimeNetworkManager.Instance.Shutdown();
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        if (subtitleText != null)
        {
            subtitleText.text = "M3 Bootstrap | Space/Enter Single Player | H Create Room | J Join Saved Room | X Shutdown";
        }

        if (hintText != null)
        {
            hintText.text = BuildHintMessage();
        }

        if (statusText != null)
        {
            statusText.text = RuntimeNetworkManager.Instance.StatusMessage;
        }
    }

    private string BuildHintMessage()
    {
        RuntimeNetworkManager networkManager = RuntimeNetworkManager.Instance;
        if (networkManager.HasSavedRoom)
        {
            return $"Saved room {networkManager.SavedRoomCode} targets {networkManager.SavedRoomAddress}:{RoomCodeUtility.GetPortForRoomCode(networkManager.SavedRoomCode)}";
        }

        return "Create a room on one instance with H, then join it from another instance with J on the same machine.";
    }
}
