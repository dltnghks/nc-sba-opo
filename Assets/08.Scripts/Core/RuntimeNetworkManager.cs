using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public sealed class RuntimeNetworkManager : MonoBehaviour
{
    private const ushort DefaultPort = 7777;

    private static RuntimeNetworkManager instance;

    [SerializeField] private string connectAddress = "127.0.0.1";
    [SerializeField] private ushort connectPort = DefaultPort;

    private NetworkManager networkManager;
    private UnityTransport unityTransport;
    private string statusMessage = "Network idle";

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
            ? $"Host started | {connectAddress}:{connectPort}"
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
            ? $"Client connecting to {connectAddress}:{connectPort}"
            : "Client start failed");
    }

    public void Shutdown()
    {
        if (networkManager == null || !networkManager.IsListening)
        {
            SetStatus($"Network idle | {connectAddress}:{connectPort}");
            return;
        }

        networkManager.Shutdown();
        SetStatus($"Network stopped | {connectAddress}:{connectPort}");
    }

    public void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return;
        }

        connectAddress = address.Trim();
        ApplyConnectionSettings();
        SetStatus($"Network idle | {connectAddress}:{connectPort}");
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
        SetStatus($"Host listening | {connectAddress}:{connectPort}");
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (networkManager.IsHost)
        {
            SetStatus($"Host connected client {clientId} | {connectAddress}:{connectPort}");
            return;
        }

        if (networkManager.LocalClientId == clientId)
        {
            SetStatus($"Client connected to {connectAddress}:{connectPort}");
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (networkManager.IsHost)
        {
            SetStatus($"Client {clientId} disconnected | {connectAddress}:{connectPort}");
            return;
        }

        if (networkManager.LocalClientId == clientId || !networkManager.IsListening)
        {
            SetStatus($"Disconnected | {connectAddress}:{connectPort}");
        }
    }

    private void SetStatus(string message)
    {
        statusMessage = message;
        Debug.Log($"[RuntimeNetworkManager] {message}", this);
    }
}
