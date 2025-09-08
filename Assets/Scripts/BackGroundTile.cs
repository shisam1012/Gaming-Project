using UnityEngine;

public class BackGroundTile : MonoBehaviour
{
    public SpriteRenderer tileRenderer;  
    public Sprite[] tileSprites; //sand, empty, fluid

 
    public void SetTileSprite(int spriteIndex)
    {
        if (spriteIndex >= 0 && spriteIndex < tileSprites.Length)
            tileRenderer.sprite = tileSprites[spriteIndex];
    }

    
   /*public void FillWithWater()
    {
        SetTileSprite(the number of the sprite); 
    }*/
}
