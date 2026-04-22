using System.Text;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class LobbyController : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text roomText;
    [SerializeField] private TMP_Text playerListText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text hintText;

    private void Update()
    {
        RefreshTexts();
        HandleInputs();
    }

    private void RefreshTexts()
    {
        RuntimeNetworkManager networkManager = RuntimeNetworkManager.Instance;

        if (titleText != null)
        {
            titleText.text = "Room Lobby";
        }

        if (roomText != null)
        {
            string roomCode = string.IsNullOrWhiteSpace(networkManager.CurrentRoomCode)
                ? networkManager.SavedRoomCode
                : networkManager.CurrentRoomCode;
            roomText.text = string.IsNullOrWhiteSpace(roomCode)
                ? $"Direct session | {networkManager.ConnectAddress}:{networkManager.ConnectPort}"
                : $"Room {roomCode} | {networkManager.ConnectAddress}:{networkManager.ConnectPort}";
        }

        if (playerListText != null)
        {
            playerListText.text = BuildPlayerList(networkManager);
        }

        if (statusText != null)
        {
            statusText.text = networkManager.StatusMessage;
        }

        if (hintText != null)
        {
            hintText.text = "Esc Leave Room | Waiting for spawned players before ready/start work";
        }
    }

    private void HandleInputs()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame || keyboard.bKey.wasPressedThisFrame)
        {
            RuntimeNetworkManager.Instance.Shutdown();
            SceneManager.LoadScene(ProjectSceneNames.Bootstrap);
        }
    }

    private static string BuildPlayerList(RuntimeNetworkManager networkManager)
    {
        IReadOnlyList<NetworkLobbyPlayer> spawnedPlayers = NetworkLobbyPlayer.ActivePlayers;
        if (spawnedPlayers.Count > 0)
        {
            List<NetworkLobbyPlayer> orderedPlayers = new(spawnedPlayers);
            orderedPlayers.Sort((left, right) => left.PlayerClientId.CompareTo(right.PlayerClientId));

            StringBuilder spawnedBuilder = new("Players");
            for (int index = 0; index < orderedPlayers.Count; index++)
            {
                NetworkLobbyPlayer player = orderedPlayers[index];
                spawnedBuilder.Append('\n');
                spawnedBuilder.Append(index + 1);
                spawnedBuilder.Append(". ");
                spawnedBuilder.Append(string.IsNullOrWhiteSpace(player.PlayerLabel)
                    ? $"Client {player.PlayerClientId}"
                    : player.PlayerLabel);
                spawnedBuilder.Append(" [spawned]");

                if (player.PlayerClientId == networkManager.LocalClientId)
                {
                    spawnedBuilder.Append(" (You)");
                }
            }

            return spawnedBuilder.ToString();
        }

        if (networkManager.ConnectedPlayerIds.Count == 0)
        {
            return "Players\nWaiting for spawned player objects...";
        }

        StringBuilder builder = new("Players");
        for (int index = 0; index < networkManager.ConnectedPlayerIds.Count; index++)
        {
            ulong clientId = networkManager.ConnectedPlayerIds[index];
            builder.Append('\n');
            builder.Append(index + 1);
            builder.Append(". ");
            builder.Append(clientId == 0 ? "Host" : $"Client {clientId}");

            if (clientId == networkManager.LocalClientId)
            {
                builder.Append(" (You)");
            }
        }

        return builder.ToString();
    }
}
