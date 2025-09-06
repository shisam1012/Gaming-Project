using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    private Board board;

    public int column;
    public int row;

    private bool isDragging = false;
    private List<Vector2Int> draggedStones = new List<Vector2Int>();
    private List<Stone> draggedStoneObjects = new List<Stone>();

    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    [System.Obsolete]
    private void Start()
    {
        board = FindObjectOfType<Board>();
        column = Mathf.RoundToInt(transform.position.x);
        row = Mathf.RoundToInt(transform.position.y);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void Initialize(GameObject[] stonePrefabs)
    {
        if (stonePrefabs.Length == 0) return;

        
    }

    private void OnMouseDown()
    {
        isDragging = true;

       
        foreach (var s in draggedStoneObjects)
            s.ResetHighlight();

        draggedStones.Clear();
        draggedStoneObjects.Clear();

        AddStoneToPath(column, row);
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (x >= 0 && x < board.width && y >= 0 && y < board.height)
        {
            if (!draggedStones.Contains(new Vector2Int(x, y)))
            {
                AddStoneToPath(x, y);
            }
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        Debug.Log("Path:");
        foreach (var pos in draggedStones)
        {
            Debug.Log($"({pos.x}, {pos.y})");
        }

       
        foreach (var stone in draggedStoneObjects)
            stone.ResetHighlight();
    }

    private void AddStoneToPath(int x, int y)
    {
        draggedStones.Add(new Vector2Int(x, y));

        Stone stone = board.allStones[x, y].GetComponent<Stone>();
        if (stone != null)
        {
            stone.Highlight();
            draggedStoneObjects.Add(stone);
        }
    }

    public void Highlight()
    {
        // spriteRenderer.color = Color.yellow; 
        spriteRenderer.color = originalColor * 1.5f;
    }

    public void ResetHighlight()
    {
        spriteRenderer.color = originalColor;
    }
}




/*
public class Stone : MonoBehaviour
{
private Vector2 firstTouchPosition;
private Vector2 finalTouchPosition;
public float swipeAngle = 0;
public int column;
public int row;
public int targetX;
public int targetY;
private GameObject otherStone;
private Board board;
private Vector2 tempPosition;

[System.Obsolete]
private void Start()
{
    board = FindObjectOfType<Board>();
    targetX = (int)transform.position.x;
    targetY = (int)transform.position.y;
    row = targetY;
    column = targetX;   
}

private void Update()
{
    targetX = column;
    targetY = row;
    if (Mathf.Abs(targetX - transform.position.x) > .1)
    {
        //move towards the target
        tempPosition = new Vector2(targetX, transform.position.y);
        transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
    }
    else
    {
        //directly set th position
        tempPosition = new Vector2(targetX, transform.position.y);
        transform.position = tempPosition;
        board.allStones[column,row] = this.gameObject;
    }
    if (Mathf.Abs(targetY - transform.position.y) > .1)
    {
        //move towards the target
        tempPosition = new Vector2(transform.position.x, targetY);
        transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
    }
    else
    {
        //directly set th position
        tempPosition = new Vector2(transform.position.x, targetY);
        transform.position = tempPosition;
        board.allStones[column, row] = this.gameObject;
    }
}

public void Initialize(GameObject[] stonePrefabs)
{
    if (stonePrefabs.Length == 0) return;

    /*int stoneToUse = Random.Range(0, stonePrefabs.Length);
    GameObject stone = Instantiate(stonePrefabs[stoneToUse], transform.position, Quaternion.identity);
    stone.transform.parent = this.transform;
    stone.name = this.gameObject.name + "_stone";*/
/*}

private void OnMouseDown()
{
    firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //firstTouchPosition = Input.mousePosition;
    Debug.Log(firstTouchPosition);
    //CalculateAngle();
}
private void OnMouseUp()
{
    finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    CalculateAngle();
}

void CalculateAngle()
{
    swipeAngle = Mathf.Atan2(finalTouchPosition.y-firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x)*180/Mathf.PI;  
    Debug.Log(swipeAngle);
    MovePieces();
}

void MovePieces()
{
    if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width)
    {
        //right swipe
        otherStone = board.allStones[column+1, row];
        otherStone.GetComponent<Stone>().column --;
        column++;
    }
    else if(swipeAngle > 45 && swipeAngle <= 135 && row < board.height)
    {
        //up swipe
        otherStone = board.allStones[column, row+1];
        otherStone.GetComponent<Stone>().row--;
        row++;
    }
    else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
    {
        //left swipe
        otherStone = board.allStones[column - 1, row];
        otherStone.GetComponent<Stone>().column++;
        column--;
    }
    else if (swipeAngle < -45 && swipeAngle >= -135 && row >0)
    {
        //down swipe
        otherStone = board.allStones[column, row-1];
        otherStone.GetComponent<Stone>().row++;
        row--;
    }
}

}
*/