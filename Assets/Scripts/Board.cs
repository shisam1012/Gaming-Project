using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject cellPrefab;
    private BackGroundTile [,] allTiles;
    public GameObject[] stonePrefabs;


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
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j);
                GameObject cell = Instantiate(cellPrefab, tempPosition, Quaternion.identity);
                cell.transform.parent = this.transform;
                cell.name = "( " + i + "_" + j + " )";

                BackGroundTile bgTile = cell.GetComponent<BackGroundTile>();
                Stone stoneTile = cell.GetComponent<Stone>();

                if (bgTile != null)
                {
                    bgTile.SetTileSprite(0); 
                    allTiles[i, j] = bgTile;
                }

                if (stoneTile != null)
                    stoneTile.Initialize(stonePrefabs);
            }
        }
    }


}
