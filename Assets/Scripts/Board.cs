using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject cellPrefab;
    private BackGroundTile [,] allTiles;
    public GameObject[,] allStones; 
    public GameObject[] stonePrefabs;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allTiles = new BackGroundTile[width,height];
        allStones = new GameObject[width,height];
        SetUp();
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

                int stoneToUse = Random.Range(0, stonePrefabs.Length);
                GameObject stone = Instantiate(stonePrefabs[stoneToUse], tempPosition, Quaternion.identity);
                stone.transform.parent = this.transform;
                stone.name = "( " + i + "_" + j + " )" + "_stone";
                allStones[i,j] = stone;
            }
        }
    }


}
