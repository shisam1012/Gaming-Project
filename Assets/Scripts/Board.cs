using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject cellPrefab;      // רק הרקע
    public GameObject[] stonePrefabs;  // האבנים עצמן

    public GameObject[,] allStones;
    private BackGroundTile[,] allTiles;

    void Start()
    {
        allTiles = new BackGroundTile[width, height];
        allStones = new GameObject[width, height];
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
                if (bgTile != null)
                {
                    bgTile.SetTileSprite(0);
                    allTiles[i, j] = bgTile;
                }

                
                int stoneToUse = Random.Range(0, stonePrefabs.Length);
                GameObject stone = Instantiate(stonePrefabs[stoneToUse], tempPosition, Quaternion.identity);
                stone.transform.parent = this.transform;
                stone.name = "( " + i + "_" + j + " )_stone";
                allStones[i, j] = stone;
            }
        }
    }

    public void RemoveStones(List<Vector2Int> stonePositions)
    {
        foreach (var pos in stonePositions)
        {
            GameObject stoneObj = allStones[pos.x, pos.y];
            if (stoneObj != null)
            {
                Debug.Log("Removing stone at: " + pos.x + "," + pos.y);
                Destroy(stoneObj);
                allStones[pos.x, pos.y] = null;
            }
        }

        // add the fall and fill
    }
}
