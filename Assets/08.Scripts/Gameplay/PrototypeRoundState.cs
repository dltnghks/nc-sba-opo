using UnityEngine;

public sealed class PrototypeRoundState : MonoBehaviour
{
    [SerializeField] private bool roundEnded;

    public bool IsRoundEnded => roundEnded;

    private void Awake()
    {
        if (FindObjectsByType<PrototypeRoundState>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (roundEnded)
        {
            return;
        }

        if (FindObjectsByType<PrototypeBrick>(FindObjectsSortMode.None).Length == 0)
        {
            EndRound("Clear");
        }
    }

    public void FailRound()
    {
        if (roundEnded)
        {
            return;
        }

        EndRound("Fail");
    }

    private void EndRound(string result)
    {
        roundEnded = true;
        Debug.Log($"Round Result: {result}");
    }
}
