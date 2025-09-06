using UnityEngine;

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
    }

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
