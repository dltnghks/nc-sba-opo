using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameplayRuntimeSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name != ProjectSceneNames.Gameplay)
        {
            return;
        }

        if (Object.FindAnyObjectByType<PaddleMovement>() != null)
        {
            return;
        }

        CreatePrototypePaddle();
    }

    private static void CreatePrototypePaddle()
    {
        GameObject paddle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paddle.name = "Prototype Paddle";
        paddle.transform.position = new Vector3(0f, 0.5f, -6f);
        paddle.transform.localScale = new Vector3(2.5f, 1f, 1f);
        paddle.AddComponent<PaddleMovement>();
    }
}
