using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrototypeBrickCatalog", menuName = "nc-sba-opo/Prototype Brick Catalog")]
public sealed class PrototypeBrickCatalog : ScriptableObject
{
    [SerializeField] private List<PrototypeBrickData> bricks = new();

    public PrototypeBrickData GetBrickById(int id)
    {
        foreach (PrototypeBrickData brick in bricks)
        {
            if (brick != null && brick.Id == id)
            {
                return brick;
            }
        }

        return null;
    }
}
