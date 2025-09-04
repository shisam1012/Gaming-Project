using UnityEngine;

public class Stone : MonoBehaviour
{
    public void Initialize(GameObject[] stonePrefabs)
    {
        if (stonePrefabs.Length == 0) return;

        int stoneToUse = Random.Range(0, stonePrefabs.Length);
        GameObject stone = Instantiate(stonePrefabs[stoneToUse], transform.position, Quaternion.identity);
        stone.transform.parent = this.transform;
        stone.name = this.gameObject.name + "_stone";
    }
}
