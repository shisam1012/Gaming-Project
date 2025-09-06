using UnityEngine;

public class Stone : MonoBehaviour
{
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    public float swipeAngle = 0;

    private void Start()
    {
        
    }

    private void Update()
    {
        
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
    }

}
