using UnityEngine;

namespace GamingProject
{
    public class AnimationSync : StateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var animsync = GameObject.Find("Animsync").GetComponent<Animator>();
            animator.SetFloat("Offset", animsync.GetCurrentAnimatorStateInfo(0).normalizedTime % 1);
        }
    }
}
