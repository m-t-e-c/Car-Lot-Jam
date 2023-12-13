using UnityEngine;

namespace CLJ.Runtime
{
    public class StickmanAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        
        private static readonly int Selected = Animator.StringToHash("Selected");
        private static readonly int IsMoving = Animator.StringToHash("Moving");

        public void PlaySelectedAnimation()
        {
            _animator.SetTrigger(Selected);
        }
        
        public void ChangeMovingState(bool isMoving)
        {
            _animator.SetBool(IsMoving, isMoving);
        }
    }
}