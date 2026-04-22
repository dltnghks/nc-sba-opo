using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public sealed class NetworkLobbyPlayer : NetworkBehaviour
{
    private static readonly List<NetworkLobbyPlayer> activePlayers = new();

    private readonly NetworkVariable<FixedString32Bytes> playerLabel =
        new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<bool> isReady =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public static IReadOnlyList<NetworkLobbyPlayer> ActivePlayers => activePlayers;

    public ulong PlayerClientId => OwnerClientId;
    public string PlayerLabel => playerLabel.Value.ToString();
    public bool IsReady => isReady.Value;

    public override void OnNetworkSpawn()
    {
        if (!activePlayers.Contains(this))
        {
            activePlayers.Add(this);
        }

        DontDestroyOnLoad(gameObject);
        name = $"Lobby Player {OwnerClientId}";

        if (IsServer && playerLabel.Value.Length == 0)
        {
            playerLabel.Value = BuildLabel(OwnerClientId);
        }
    }

    public override void OnNetworkDespawn()
    {
        activePlayers.Remove(this);
    }

    private void OnDestroy()
    {
        activePlayers.Remove(this);
    }

    [ServerRpc]
    public void SetReadyServerRpc(bool ready)
    {
        isReady.Value = ready;
    }

    public void SetReady(bool ready)
    {
        if (IsServer)
        {
            isReady.Value = ready;
            return;
        }

        if (IsOwner)
        {
            SetReadyServerRpc(ready);
        }
    }

    private static string BuildLabel(ulong clientId)
    {
        return clientId == 0 ? "Host" : $"Client {clientId}";
    }
}
