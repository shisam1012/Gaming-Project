using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    public GameObject cellPrefab;      // רק הרקע
    [SerializeField] private Stone[] stonePrefabsRef;  // האבנים עצמן

    public Stone[,] allStones;
    private BackGroundTile[,] allTiles;

    public int Width => width;
    public int Height => height;

    void Start()
    {
        allTiles = new BackGroundTile[width, height];
        allStones = new Stone[width, height];
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

                
                int stoneToUse = Random.Range(0, stonePrefabsRef.Length);
                Stone stone = Instantiate(stonePrefabsRef[stoneToUse], tempPosition, Quaternion.identity);
                stone.Init(this);
                stone.transform.parent = transform;
                stone.name = "( " + i + "_" + j + " )_stone";
                allStones[i, j] = stone;
            }
        }
    }

    public void RemoveStones(List<Vector2Int> stonePositions)
    {
        foreach (var pos in stonePositions)
        {
            Stone stoneObj = allStones[pos.x, pos.y];
            if (stoneObj != null)
            {
                Debug.Log("Removing stone at: " + pos.x + "," + pos.y);
                Destroy(stoneObj.gameObject);
                allStones[pos.x, pos.y] = null;
            }
        }

        // add the fall and fill
    }
}
