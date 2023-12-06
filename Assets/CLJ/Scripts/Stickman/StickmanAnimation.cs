using UnityEngine;

namespace CLJ.Scripts
{
    public class StickmanAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        
        private static readonly int Selected = Animator.StringToHash("Selected");

        public void PlaySelectedAnimation()
        {
            _animator.SetTrigger(Selected);
        }
    }
}