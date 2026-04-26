using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameplayRuntimeSetup
{
    private const string PrototypeStageResourcePath = "Stages/PrototypeStage01";
    private const string PrototypeBrickCatalogResourcePath = "Stages/PrototypeBrickCatalog";
    private const string ScoreChangedEventResourcePath = "Events/ScoreChanged";
    private const string LivesChangedEventResourcePath = "Events/LivesChanged";
    private const string RoundEndedEventResourcePath = "Events/RoundEnded";
    private const string BrickRootName = "@Bricks";

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

        RuntimeNetworkManager.Instance.SpawnGameplayStateIfNeeded();
        BuildStageLayout();

        if (Object.FindAnyObjectByType<PrototypeSessionState>() == null)
        {
            CreateSessionState();
        }

        if (Object.FindAnyObjectByType<PrototypeRoundState>() == null)
        {
            CreateRoundState();
        }

        ConfigureHudIfPresent();

        if (Object.FindAnyObjectByType<PrototypeResultFlowController>() == null)
        {
            CreateResultFlowController();
        }

        if (Object.FindAnyObjectByType<PrototypeAudioManager>() == null)
        {
            CreateAudioManager();
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
        PrototypeRoundState roundStateComponent = roundState.AddComponent<PrototypeRoundState>();
        roundStateComponent.Configure(Resources.Load<StringEventChannelSO>(RoundEndedEventResourcePath));
    }

    private static void CreateSessionState()
    {
        GameObject sessionState = new("Prototype Session State");
        PrototypeSessionState sessionStateComponent = sessionState.AddComponent<PrototypeSessionState>();
        sessionStateComponent.Configure(
            Resources.Load<IntEventChannelSO>(ScoreChangedEventResourcePath),
            Resources.Load<IntEventChannelSO>(LivesChangedEventResourcePath));
    }

    private static void ConfigureHudIfPresent()
    {
        PrototypeHudController hudController = Object.FindAnyObjectByType<PrototypeHudController>();
        if (hudController == null)
        {
            Debug.LogWarning("PrototypeHudController is not placed in the Gameplay scene.");
            return;
        }

        hudController.Configure(
            Resources.Load<IntEventChannelSO>(ScoreChangedEventResourcePath),
            Resources.Load<IntEventChannelSO>(LivesChangedEventResourcePath),
            Resources.Load<StringEventChannelSO>(RoundEndedEventResourcePath));
    }

    private static void CreateResultFlowController()
    {
        GameObject resultFlow = new("Prototype Result Flow");
        PrototypeResultFlowController resultFlowController = resultFlow.AddComponent<PrototypeResultFlowController>();
        resultFlowController.Configure(Resources.Load<StringEventChannelSO>(RoundEndedEventResourcePath));
    }

    private static void CreateAudioManager()
    {
        GameObject audioManager = new("Prototype Audio Manager");
        audioManager.AddComponent<PrototypeAudioManager>();
    }

    private static void BuildStageLayout()
    {
        PrototypeStageData stageData = Resources.Load<PrototypeStageData>(PrototypeStageResourcePath);
        if (stageData == null)
        {
            Debug.LogError($"Missing stage data at Resources/{PrototypeStageResourcePath}");
            return;
        }

        PrototypeBrickCatalog brickCatalog = Resources.Load<PrototypeBrickCatalog>(PrototypeBrickCatalogResourcePath);
        if (brickCatalog == null)
        {
            Debug.LogError($"Missing brick catalog at Resources/{PrototypeBrickCatalogResourcePath}");
            return;
        }

        Transform brickRoot = FindOrCreateBrickRoot();
        ClearExistingBricks(brickRoot);

        for (int row = 0; row < stageData.Rows; row++)
        {
            for (int column = 0; column < stageData.Columns; column++)
            {
                int brickId = stageData.GetCellValue(row, column);
                if (brickId <= 0)
                {
                    continue;
                }

                PrototypeBrickData brickData = brickCatalog.GetBrickById(brickId);
                if (brickData == null)
                {
                    Debug.LogWarning($"Missing brick definition for id {brickId} in stage {stageData.StageId}");
                    continue;
                }

                CreateBrick(brickRoot, stageData, row, column, brickData);
            }
        }
    }

    private static Transform FindOrCreateBrickRoot()
    {
        GameObject brickRoot = GameObject.Find(BrickRootName);
        if (brickRoot != null)
        {
            return brickRoot.transform;
        }

        return new GameObject(BrickRootName).transform;
    }

    private static void ClearExistingBricks(Transform brickRoot)
    {
        PrototypeBrick[] existingBricks = Object.FindObjectsByType<PrototypeBrick>(FindObjectsSortMode.None);
        foreach (PrototypeBrick brick in existingBricks)
        {
            if (brick != null)
            {
                Object.Destroy(brick.gameObject);
            }
        }

        for (int i = brickRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = brickRoot.GetChild(i);
            if (child != null)
            {
                Object.Destroy(child.gameObject);
            }
        }
    }

    private static void CreateBrick(Transform brickRoot, PrototypeStageData stageData, int row, int column, PrototypeBrickData brickData)
    {
        GameObject brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brick.name = "Brick";
        brick.transform.SetParent(brickRoot, false);
        brick.transform.localPosition = GetBrickPosition(stageData, row, column);
        brick.transform.localScale = stageData.BrickScale;

        PrototypeBrick prototypeBrick = brick.AddComponent<PrototypeBrick>();
        prototypeBrick.Configure(brickData);
    }

    private static Vector3 GetBrickPosition(PrototypeStageData stageData, int row, int column)
    {
        float width = (stageData.Columns - 1) * stageData.CellSize.x;
        float depth = (stageData.Rows - 1) * stageData.CellSize.y;
        float x = stageData.Origin.x - (width * 0.5f) + (column * stageData.CellSize.x);
        float z = stageData.Origin.z - (depth * 0.5f) + (row * stageData.CellSize.y);

        return new Vector3(x, stageData.Origin.y, z);
    }
}
