using UnityEngine;

[CreateAssetMenu(fileName = "MapShape", menuName = "Game/Map Shape")]
public class MapShape : ScriptableObject
{
    [Header("Grid Size (if Rows Text is used, these will be overwritten)")]
    public int width = 7;
    public int height = 10;

    [Header("Mask (true = playable cell, false = blocked/missing)")]
    [Tooltip("Length must be width*height. Row-major: y=0 is bottom row, y=height-1 is top row; each row is left->right.")]
    public bool[] mask;

    [Header("Rows Text (optional, paste 0/1 rows top→bottom)")]
    [TextArea(4, 20)]
    public string rowsText =
@"1111111
1111111
1111111
1111111
1111111
1111111
1111111
1111111
1111111
1111111";

    private void OnValidate()
    {
        TryApplyRowsText();
    }

    private void TryApplyRowsText()
    {
        if (string.IsNullOrWhiteSpace(rowsText)) return;

        string[] lines = rowsText.Replace("\r", "").Split('\n');
        int lineCount = 0;
        foreach (var line in lines) if (!string.IsNullOrWhiteSpace(line)) lineCount++;
        if (lineCount == 0) return;


        int h = 0;
        int w = 0;
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (w == 0) w = line.Length;
            h++;
        }

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (line.Length != w) { Debug.LogWarning("[MapShape] Row widths differ; aborting parse."); return; }
        }

        width = w;
        height = h;
        if (mask == null || mask.Length != width * height) mask = new bool[width * height];

        int y = 0;
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;

            for (int x = 0; x < width; x++)
            {
                char c = line[x];
                bool playable = (c == '1' || c == 'X' || c == '#');

                int topToBottomY = (height - 1) - y;
                mask[topToBottomY * width + x] = playable;
            }
            y++;
        }
    }

    public bool IsPlayable(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (mask == null || mask.Length != width * height) return true;
        return mask[y * width + x];
    }
}
