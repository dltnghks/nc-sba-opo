using UnityEngine;

[CreateAssetMenu(fileName = "PrototypeBrickData", menuName = "nc-sba-opo/Prototype Brick Data")]
public sealed class PrototypeBrickData : ScriptableObject
{
    [SerializeField] private int id = 1;
    [SerializeField] private string displayName = "Basic Brick";
    [SerializeField] private int hitPoints = 1;
    [SerializeField] private int score = 100;
    [SerializeField] private Color color = Color.white;

    public int Id => id;
    public string DisplayName => displayName;
    public int HitPoints => Mathf.Max(hitPoints, 1);
    public int Score => Mathf.Max(score, 0);
    public Color Color => color;
}
