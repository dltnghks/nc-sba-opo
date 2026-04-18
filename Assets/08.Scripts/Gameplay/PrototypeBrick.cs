using UnityEngine;

public sealed class PrototypeBrick : MonoBehaviour
{
    [SerializeField] private PrototypeBrickData brickData;
    [SerializeField] private int hitPoints = 1;

    public void Configure(PrototypeBrickData configuredBrickData)
    {
        brickData = configuredBrickData;
        hitPoints = configuredBrickData != null ? configuredBrickData.HitPoints : 1;

        Renderer brickRenderer = GetComponent<Renderer>();
        if (brickRenderer != null && brickData != null)
        {
            brickRenderer.material.color = brickData.Color;
        }
    }

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
