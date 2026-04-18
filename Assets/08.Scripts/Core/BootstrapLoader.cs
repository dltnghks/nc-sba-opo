using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = ProjectSceneNames.Gameplay;

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("BootstrapLoader requires a gameplay scene name.", this);
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }
}
