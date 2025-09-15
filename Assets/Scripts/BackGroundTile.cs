using UnityEngine;
using static Stone;

[RequireComponent(typeof(SpriteRenderer))]
public class BackGroundTile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite[] tileSprites; 

    public enum BackgroundType
    {
        Sand,
        Empty,
        Fluid
    }
    [SerializeField] private BackgroundType type;
    public BackgroundType Type => type;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    public void SetTileSprite(int spriteIndex)
    {
        if (spriteIndex >= 0 && spriteIndex < tileSprites.Length)
            spriteRenderer.sprite = tileSprites[spriteIndex];
    }

    public void SetTileType(BackgroundType newType)
    {
        type = newType;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        switch (type)
        {
            case BackgroundType.Sand: spriteRenderer.color = new Color(0.6f, 0.4f, 0.2f); break;
            case BackgroundType.Empty: spriteRenderer.color = new Color(0.3f, 0.2f, 0.1f); break;
            case BackgroundType.Fluid: spriteRenderer.color = new Color (57f / 255f, 136f / 255f, 247f / 255f); break;
            //change to sprites instead of regular colors
        }
    }

    
}
