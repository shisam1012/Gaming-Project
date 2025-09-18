//using System;
using System.Collections;
using UnityEngine;
using static GamingProject.FlowerAnimation;

namespace GamingProject
{
    public class FlowerAnimation : MonoBehaviour
    {

        // Use this for initialization
        public enum FlowerType
        {
            Sprout,
            Red,
            Yellow,
            Purple
        }

        //[SerializeField] private FlowerType startingtype;
        //[SerializeField] private Sprite[] sprouts;
        [SerializeField] private Sprite[] flowers;

        private SpriteRenderer _spriteRenderer;
        private Animator animator;
        private Board _board;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            animator.enabled = false;
            SetFlowerSprite((int)FlowerType.Sprout);
        }

        public void SetBoard(Board board)
        {
            int flowertype = Random.Range(1, flowers.Length);
            board.onWin.AddListener(() => StartAnimation((FlowerType)flowertype));
            _board = board;
            
            
        }
        


        public void StartAnimation(FlowerType flowerType)
        {
            animator.enabled = true;
            //animator.SetTrigger("Grow");
            StartCoroutine(WaitForAnimationEnd(flowerType));
        }

        private IEnumerator WaitForAnimationEnd(FlowerType flowerType)
        {
            yield return new WaitForSeconds(3f);

            animator.enabled = false;
            SetFlowerSprite((int)flowerType);
            _board.onWin.RemoveAllListeners();
        }
        
        private void SetFlowerSprite(int spriteIndex)
        {
            if (spriteIndex >= 0 && spriteIndex < flowers.Length)
                _spriteRenderer.sprite = flowers[spriteIndex];
        }
    }
}