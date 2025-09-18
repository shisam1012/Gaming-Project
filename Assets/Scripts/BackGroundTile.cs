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
        public BackgroundType Type => type;


        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
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
                case BackgroundType.Fluid:
                    //spriteRenderer.color = new Color(57f / 255f, 136f / 255f, 247f / 255f);
                    spriteRenderer.color = Color.white;
                    animator.enabled = true;
                    break;
                    //change to sprites instead of regular colors
            }
        }

    }
}
