using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = ProjectSceneNames.Gameplay;
    [SerializeField] private string defaultClientAddress = "127.0.0.1";
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
                HandleHostPressed();
            }
            else if (keyboard.cKey.wasPressedThisFrame)
            {
                HandleClientPressed();
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

    private void HandleHostPressed()
    {
        RuntimeNetworkManager.Instance.SetAddress(defaultClientAddress);
        RuntimeNetworkManager.Instance.StartHost();
        RefreshStatus();
    }

    private void HandleClientPressed()
    {
        RuntimeNetworkManager.Instance.SetAddress(defaultClientAddress);
        RuntimeNetworkManager.Instance.StartClient();
        RefreshStatus();
    }

    private void HandleShutdownPressed()
    {
        RuntimeNetworkManager.Instance.Shutdown();
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        if (statusText != null)
        {
            statusText.text = RuntimeNetworkManager.Instance.StatusMessage;
        }
    }
}
