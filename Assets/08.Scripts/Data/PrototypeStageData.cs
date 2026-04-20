using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrototypeStageData", menuName = "nc-sba-opo/Prototype Stage Data")]
public sealed class PrototypeStageData : ScriptableObject
{
    [SerializeField] private string stageId = "stage-01";
    [SerializeField] private Vector3 origin = new(0f, 0.5f, 5f);
    [SerializeField] private Vector2 cellSize = new(1f, 1f);
    [SerializeField] private Vector3 brickScale = Vector3.one;
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 1;
    [SerializeField] private List<int> cells = new() { 1, 1, 1, 1 };

    public string StageId => stageId;
    public Vector3 Origin => origin;
    public Vector2 CellSize => cellSize;
    public Vector3 BrickScale => brickScale;
    public int Columns => columns;
    public int Rows => rows;
    public IReadOnlyList<int> Cells => cells;

    public int GetCellValue(int row, int column)
    {
        if (row < 0 || row >= rows || column < 0 || column >= columns)
        {
            return 0;
        }

        int index = (row * columns) + column;
        if (index < 0 || index >= cells.Count)
        {
            return 0;
        }

        return cells[index];
    }
}
