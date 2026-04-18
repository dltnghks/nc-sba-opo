using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class PrototypeFailZone : MonoBehaviour
{
    private void Reset()
    {
        if (TryGetComponent(out Collider hitCollider))
        {
            hitCollider.isTrigger = true;
        }
    }

    public void TriggerFail()
    {
        PrototypeRoundState roundState = FindAnyObjectByType<PrototypeRoundState>();
        if (roundState != null)
        {
            roundState.FailRound();
        }
    }
}
