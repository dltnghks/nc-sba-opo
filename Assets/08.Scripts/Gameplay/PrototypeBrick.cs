using UnityEngine;

public sealed class PrototypeBrick : MonoBehaviour
{
    [SerializeField] private int hitPoints = 1;

    public bool ApplyHit()
    {
        hitPoints = Mathf.Max(hitPoints - 1, 0);
        if (hitPoints > 0)
        {
            return false;
        }

        Destroy(gameObject);
        return true;
    }
}
