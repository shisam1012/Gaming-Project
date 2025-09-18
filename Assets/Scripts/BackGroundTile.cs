using UnityEngine;
using static GamingProject.Stone;

namespace GamingProject
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackGroundTile : MonoBehaviour
    {
        public enum BackgroundType
        {
            Sand,
            Empty,
            Fluid
        }
        
        [SerializeField] private BackgroundType type;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        public Sprite[] tileSprites;
        private Color color;
        public BackgroundType Type => type;


        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            animator.enabled = false;
            color = new(0.7059f, 0.6157f, 0.4941f);
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
                case BackgroundType.Sand: 
                    spriteRenderer.color = color;
                    SetTileSprite((int)BackgroundType.Sand);
                    break;
                case BackgroundType.Empty:
                    spriteRenderer.color = color;
                    SetTileSprite((int)BackgroundType.Empty);
                    break;
                case BackgroundType.Fluid:
                    spriteRenderer.color = Color.white;
                    SetTileSprite((int)BackgroundType.Empty); // background the same
                    animator.enabled = true;
                    break;
            }
        }

    }
}
