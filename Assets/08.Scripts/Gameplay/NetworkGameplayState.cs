using System.Diagnostics;
using Unity.Collections;
using Unity.Netcode;

public sealed class NetworkGameplayState : NetworkBehaviour
{
    private static NetworkGameplayState instance;

    private readonly NetworkVariable<int> score =
        new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<int> lives =
        new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<bool> roundEnded =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<FixedString32Bytes> roundResult =
        new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public static NetworkGameplayState Instance => instance;
    public static bool HasInstance => instance != null;

    public int Score => score.Value;
    public int Lives => lives.Value;
    public bool IsRoundEnded => roundEnded.Value;
    public string RoundResult => roundResult.Value.ToString();
    public bool HasStateAuthority => IsServer;

    public override void OnNetworkSpawn()
    {
        instance = this;
        name = "Network Gameplay State";
    }

    public override void OnNetworkDespawn()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void InitializeRound(int startingLives)
    {
        if (!IsServer)
        {
            return;
        }

        score.Value = 0;
        lives.Value = startingLives > 0 ? startingLives : 1;
        roundEnded.Value = false;
        roundResult.Value = default;
    }

    public void AddScore(int amount)
    {
        Debug.Print($"AddScore called with amount: {amount}");
        if (IsServer)
        {
            ApplyAddScore(amount);
            return;
        }

        AddScoreServerRpc(amount);
    }

    public bool TryConsumeLife()
    {
        if (IsServer)
        {
            return ApplyConsumeLife();
        }

        bool hadMoreThanOneLife = lives.Value > 1;
        ConsumeLifeServerRpc();
        return hadMoreThanOneLife;
    }

    public void EndRound(string result)
    {
        if (IsServer)
        {
            ApplyEndRound(result);
            return;
        }

        EndRoundServerRpc(new FixedString32Bytes(result ?? string.Empty));
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddScoreServerRpc(int amount)
    {
        ApplyAddScore(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ConsumeLifeServerRpc()
    {
        ApplyConsumeLife();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndRoundServerRpc(FixedString32Bytes result)
    {
        ApplyEndRound(result.ToString());
    }

    private void ApplyAddScore(int amount)
    {
        if (roundEnded.Value)
        {
            return;
        }

        score.Value += amount > 0 ? amount : 0;
    }

    private bool ApplyConsumeLife()
    {
        if (roundEnded.Value)
        {
            return lives.Value > 0;
        }

        lives.Value = lives.Value > 0 ? lives.Value - 1 : 0;
        if (lives.Value <= 0)
        {
            ApplyEndRound("Fail");
        }

        return lives.Value > 0;
    }

    private void ApplyEndRound(string result)
    {
        if (roundEnded.Value)
        {
            return;
        }

        roundEnded.Value = true;
        roundResult.Value = result;
    }
}
