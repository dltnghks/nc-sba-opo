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

        if (Object.FindAnyObjectByType<PrototypeRoundState>() == null)
        {
            CreateRoundState();
        }

        PaddleMovement paddle = Object.FindAnyObjectByType<PaddleMovement>();
        if (paddle == null)
        {
            paddle = CreatePrototypePaddle();
        }

        if (Object.FindAnyObjectByType<PrototypeBallController>() == null)
        {
            CreatePrototypeBall(paddle);
        }
    }

    private static PaddleMovement CreatePrototypePaddle()
    {
        GameObject paddle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paddle.name = "Prototype Paddle";
        paddle.transform.position = new Vector3(0f, 0.5f, -6f);
        paddle.transform.localScale = new Vector3(2.5f, 1f, 1f);
        return paddle.AddComponent<PaddleMovement>();
    }

    private static void CreatePrototypeBall(PaddleMovement paddle)
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Prototype Ball";
        ball.transform.localScale = Vector3.one * 0.6f;

        PrototypeBallController ballController = ball.AddComponent<PrototypeBallController>();
        ballController.SetPaddle(paddle);
    }

    private static void CreateRoundState()
    {
        GameObject roundState = new("Prototype Round State");
        roundState.AddComponent<PrototypeRoundState>();
    }
}
