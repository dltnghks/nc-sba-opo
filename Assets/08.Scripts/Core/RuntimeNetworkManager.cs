using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public sealed class RuntimeNetworkManager : MonoBehaviour
{
    private const ushort DefaultPort = 7777;
    private const string SavedRoomCodeKey = "bootstrap.saved_room_code";
    private const string SavedRoomAddressKey = "bootstrap.saved_room_address";

    private static RuntimeNetworkManager instance;

    [SerializeField] private string connectAddress = "127.0.0.1";
    [SerializeField] private ushort connectPort = DefaultPort;

    private NetworkManager networkManager;
    private UnityTransport unityTransport;
    private string statusMessage = "Network idle";
    private string currentRoomCode = string.Empty;

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
            SetIdleStatus();
            return;
        }

        networkManager.Shutdown();
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
        ApplyConnectionSettings();
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
    }

    private void UnregisterCallbacks()
    {
        networkManager.OnServerStarted -= HandleServerStarted;
        networkManager.OnClientConnectedCallback -= HandleClientConnected;
        networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
    }

    private void HandleServerStarted()
    {
        SetStatus(BuildStatusPrefix("Host listening"));
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (networkManager.IsHost)
        {
            SetStatus(BuildStatusPrefix($"Host connected client {clientId}"));
            return;
        }

        if (networkManager.LocalClientId == clientId)
        {
            SetStatus(BuildStatusPrefix("Client connected"));
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (networkManager.IsHost)
        {
            SetStatus(BuildStatusPrefix($"Client {clientId} disconnected"));
            return;
        }

        if (networkManager.LocalClientId == clientId || !networkManager.IsListening)
        {
            SetStatus(BuildStatusPrefix("Disconnected"));
        }
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
