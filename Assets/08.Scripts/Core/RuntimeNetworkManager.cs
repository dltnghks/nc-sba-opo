using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection;

public sealed class RuntimeNetworkManager : MonoBehaviour
{
    private const ushort DefaultPort = 7777;
    private const string SavedRoomCodeKey = "bootstrap.saved_room_code";
    private const string SavedRoomAddressKey = "bootstrap.saved_room_address";
    private const string LobbySnapshotMessageName = "lobby-snapshot";
    private const uint RuntimeLobbyPlayerPrefabHash = 0x4E43504F;

    private static readonly FieldInfo GlobalObjectIdHashField =
        typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo PrefabGlobalObjectIdHashField =
        typeof(NetworkObject).GetField("PrefabGlobalObjectIdHash", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly PropertyInfo IsSceneObjectProperty =
        typeof(NetworkObject).GetProperty("IsSceneObject", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static RuntimeNetworkManager instance;

    [SerializeField] private string connectAddress = "127.0.0.1";
    [SerializeField] private ushort connectPort = DefaultPort;

    private NetworkManager networkManager;
    private UnityTransport unityTransport;
    private string statusMessage = "Network idle";
    private string currentRoomCode = string.Empty;
    private readonly List<ulong> connectedPlayerIds = new();
    private GameObject runtimeLobbyPlayerPrefab;

    public static RuntimeNetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject root = new("Runtime Network Manager");
                instance = root.AddComponent<RuntimeNetworkManager>();
            }

            return instance;
        }
    }

    public string StatusMessage => statusMessage;
    public bool IsSessionActive => networkManager != null && networkManager.IsListening;
    public string ConnectAddress => connectAddress;
    public ushort ConnectPort => connectPort;
    public string CurrentRoomCode => currentRoomCode;
    public string SavedRoomCode => PlayerPrefs.GetString(SavedRoomCodeKey, string.Empty);
    public string SavedRoomAddress => PlayerPrefs.GetString(SavedRoomAddressKey, connectAddress);
    public bool HasSavedRoom => !string.IsNullOrWhiteSpace(SavedRoomCode);
    public IReadOnlyList<ulong> ConnectedPlayerIds => connectedPlayerIds;
    public ulong LocalClientId => networkManager != null ? networkManager.LocalClientId : 0;
    public bool IsHost => networkManager != null && networkManager.IsHost;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureNetworkObjects();
        RegisterCallbacks();
        SetStatus($"Network idle | {connectAddress}:{connectPort}");
    }

    private void OnDestroy()
    {
        if (instance == this && networkManager != null)
        {
            UnregisterCallbacks();
        }
    }

    public void StartHost()
    {
        EnsureNetworkObjects();
        if (networkManager.IsListening)
        {
            Shutdown();
        }

        ApplyConnectionSettings();
        bool started = networkManager.StartHost();
        SetStatus(started
            ? BuildStatusPrefix("Host started")
            : "Host start failed");
    }

    public void StartClient()
    {
        EnsureNetworkObjects();
        if (networkManager.IsListening)
        {
            Shutdown();
        }

        ApplyConnectionSettings();
        bool started = networkManager.StartClient();
        SetStatus(started
            ? BuildStatusPrefix("Client connecting")
            : "Client start failed");
    }

    public bool CreateRoom()
    {
        string generatedCode = RoomCodeUtility.GenerateRoomCode();
        currentRoomCode = generatedCode;
        connectPort = RoomCodeUtility.GetPortForRoomCode(generatedCode);
        SaveRoom(generatedCode, connectAddress);
        StartHost();
        return networkManager != null && networkManager.IsListening;
    }

    public bool JoinRoom(string roomCode, string address = null)
    {
        if (!RoomCodeUtility.TryNormalize(roomCode, out string normalizedCode))
        {
            SetStatus("Join failed | invalid room code");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            connectAddress = address.Trim();
        }

        currentRoomCode = normalizedCode;
        connectPort = RoomCodeUtility.GetPortForRoomCode(normalizedCode);
        SaveRoom(normalizedCode, connectAddress);
        StartClient();
        return networkManager != null && networkManager.IsListening;
    }

    public bool JoinSavedRoom()
    {
        string savedRoomCode = SavedRoomCode;
        if (string.IsNullOrWhiteSpace(savedRoomCode))
        {
            SetStatus("Join failed | no saved room");
            return false;
        }

        return JoinRoom(savedRoomCode, SavedRoomAddress);
    }

    public void Shutdown()
    {
        if (networkManager == null || !networkManager.IsListening)
        {
            connectedPlayerIds.Clear();
            SetIdleStatus();
            return;
        }

        networkManager.Shutdown();
        connectedPlayerIds.Clear();
        SetStatus(BuildStatusPrefix("Network stopped"));
    }

    public void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return;
        }

        connectAddress = address.Trim();
        ApplyConnectionSettings();
        SetIdleStatus();
    }

    private void EnsureNetworkObjects()
    {
        if (networkManager == null)
        {
            networkManager = GetComponent<NetworkManager>();
            if (networkManager == null)
            {
                networkManager = gameObject.AddComponent<NetworkManager>();
            }
        }

        if (unityTransport == null)
        {
            unityTransport = GetComponent<UnityTransport>();
            if (unityTransport == null)
            {
                unityTransport = gameObject.AddComponent<UnityTransport>();
            }
        }

        networkManager.NetworkConfig ??= new NetworkConfig();
        networkManager.NetworkConfig.NetworkTransport = unityTransport;
        networkManager.NetworkConfig.ForceSamePrefabs = false;
        EnsureRuntimeLobbyPlayerPrefab();
        ApplyConnectionSettings();
    }

    private void EnsureRuntimeLobbyPlayerPrefab()
    {
        if (runtimeLobbyPlayerPrefab == null)
        {
            runtimeLobbyPlayerPrefab = CreateRuntimeLobbyPlayerPrefab();
        }

        if (!networkManager.NetworkConfig.Prefabs.Contains(runtimeLobbyPlayerPrefab))
        {
            networkManager.AddNetworkPrefab(runtimeLobbyPlayerPrefab);
        }

        networkManager.NetworkConfig.PlayerPrefab = runtimeLobbyPlayerPrefab;
    }

    private GameObject CreateRuntimeLobbyPlayerPrefab()
    {
        GameObject root = new("Runtime Lobby Player Prefab");
        root.transform.SetParent(transform, false);
        root.hideFlags = HideFlags.HideInHierarchy;

        NetworkObject networkObject = root.AddComponent<NetworkObject>();
        root.AddComponent<NetworkLobbyPlayer>();

        GlobalObjectIdHashField?.SetValue(networkObject, RuntimeLobbyPlayerPrefabHash);
        PrefabGlobalObjectIdHashField?.SetValue(networkObject, RuntimeLobbyPlayerPrefabHash);
        IsSceneObjectProperty?.SetValue(networkObject, false);

        return root;
    }

    private void ApplyConnectionSettings()
    {
        if (unityTransport == null)
        {
            return;
        }

        unityTransport.SetConnectionData(connectAddress, connectPort);
    }

    private void RegisterCallbacks()
    {
        networkManager.OnServerStarted -= HandleServerStarted;
        networkManager.OnServerStarted += HandleServerStarted;
        networkManager.OnClientConnectedCallback -= HandleClientConnected;
        networkManager.OnClientConnectedCallback += HandleClientConnected;
        networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
        networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
        networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(LobbySnapshotMessageName);
        networkManager.CustomMessagingManager.RegisterNamedMessageHandler(LobbySnapshotMessageName, HandleLobbySnapshot);
    }

    private void UnregisterCallbacks()
    {
        networkManager.OnServerStarted -= HandleServerStarted;
        networkManager.OnClientConnectedCallback -= HandleClientConnected;
        networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
        networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(LobbySnapshotMessageName);
    }

    private void HandleServerStarted()
    {
        RefreshConnectedPlayerIdsFromHost();
        LoadSceneIfNeeded(ProjectSceneNames.Lobby);
        SetStatus(BuildStatusPrefix("Host listening"));
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (networkManager.IsHost)
        {
            RefreshConnectedPlayerIdsFromHost();
            BroadcastLobbySnapshot();
            SetStatus(BuildStatusPrefix($"Host connected client {clientId}"));
            return;
        }

        if (networkManager.LocalClientId == clientId)
        {
            connectedPlayerIds.Clear();
            connectedPlayerIds.Add(clientId);
            LoadSceneIfNeeded(ProjectSceneNames.Lobby);
            SetStatus(BuildStatusPrefix("Client connected"));
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (networkManager.IsHost)
        {
            RefreshConnectedPlayerIdsFromHost();
            BroadcastLobbySnapshot();
            SetStatus(BuildStatusPrefix($"Client {clientId} disconnected"));
            return;
        }

        if (networkManager.LocalClientId == clientId || !networkManager.IsListening)
        {
            connectedPlayerIds.Clear();
            SetStatus(BuildStatusPrefix("Disconnected"));
        }
    }

    private void RefreshConnectedPlayerIdsFromHost()
    {
        connectedPlayerIds.Clear();
        if (networkManager == null)
        {
            return;
        }

        foreach (ulong clientId in networkManager.ConnectedClientsIds)
        {
            connectedPlayerIds.Add(clientId);
        }
    }

    private void BroadcastLobbySnapshot()
    {
        if (networkManager == null || !networkManager.IsHost)
        {
            return;
        }

        using FastBufferWriter writer = new(sizeof(int) + (sizeof(ulong) * Mathf.Max(connectedPlayerIds.Count, 1)), Allocator.Temp);
        writer.WriteValueSafe(connectedPlayerIds.Count);
        for (int index = 0; index < connectedPlayerIds.Count; index++)
        {
            writer.WriteValueSafe(connectedPlayerIds[index]);
        }

        for (int index = 0; index < connectedPlayerIds.Count; index++)
        {
            ulong targetClientId = connectedPlayerIds[index];
            if (targetClientId == networkManager.LocalClientId)
            {
                continue;
            }

            networkManager.CustomMessagingManager.SendNamedMessage(LobbySnapshotMessageName, targetClientId, writer);
        }
    }

    private void HandleLobbySnapshot(ulong senderClientId, FastBufferReader reader)
    {
        connectedPlayerIds.Clear();
        reader.ReadValueSafe(out int playerCount);
        for (int index = 0; index < playerCount; index++)
        {
            reader.ReadValueSafe(out ulong clientId);
            connectedPlayerIds.Add(clientId);
        }
    }

    private static void LoadSceneIfNeeded(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == sceneName)
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void SaveRoom(string roomCode, string address)
    {
        PlayerPrefs.SetString(SavedRoomCodeKey, roomCode);
        PlayerPrefs.SetString(SavedRoomAddressKey, address);
        PlayerPrefs.Save();
    }

    private void SetIdleStatus()
    {
        SetStatus(BuildStatusPrefix("Network idle"));
    }

    private string BuildStatusPrefix(string label)
    {
        if (!string.IsNullOrWhiteSpace(currentRoomCode))
        {
            return $"{label} | room {currentRoomCode} | {connectAddress}:{connectPort}";
        }

        return $"{label} | {connectAddress}:{connectPort}";
    }

    private void SetStatus(string message)
    {
        statusMessage = message;
        Debug.Log($"[RuntimeNetworkManager] {message}", this);
    }
}
