using Unity.VisualScripting;
using UnityEngine;

public class BackGroundTile : MonoBehaviour
{
    public GameObject[] stones; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Initialize()
    {
        int stoneToUse = Random.Range(0, stones.Length);
        GameObject stone = Instantiate(stones[stoneToUse], transform.position, Quaternion.identity);
        stone.transform.parent = this.transform;
        stone.name = this.gameObject.name;

    }
}
