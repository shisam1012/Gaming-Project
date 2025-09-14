using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Grid")]
    public int width = 7;
    public int height = 10;
    public MapShape shape;

    [Header("Rules")]
    public float timeLimitSeconds = 60f;
    public float pauseSeconds = 2f;

    [Header("Power Ups")]
    public bool allowPowerUps = true;
    public string[] powerUps;

    [Header("Flow Endpoints")]
    [Tooltip("Must be strictly inside the grid (not on border).")]
    public Vector2Int startInside;
    [Tooltip("Must be on the border of the grid.")]
    public Vector2Int destinationOnBorder;

    [TextArea] public string description;

    public int GetWidth() => shape ? shape.width : width;
    public int GetHeight() => shape ? shape.height : height;

    public bool IsPlayable(int x, int y)
    {
        if (shape == null) return true;
        return shape.IsPlayable(x, y);
    }
}
