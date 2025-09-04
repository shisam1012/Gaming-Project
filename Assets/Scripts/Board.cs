using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject cellPrefab;
    private BackGroundTile [,] allTiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allTiles = new BackGroundTile[width,height];
        SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetUp()
    {
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2 (i, j);
                GameObject backgroundTile = Instantiate(cellPrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + i + "_" + j + " )";
            }

        }

    }

}
