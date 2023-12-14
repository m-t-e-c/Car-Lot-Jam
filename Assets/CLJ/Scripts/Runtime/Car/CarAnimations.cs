using UnityEngine;

namespace CLJ.Runtime
{
    public class CarAnimations : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        
        private static readonly int DoorOpen = Animator.StringToHash("OpenDoor");
        private static readonly int Stumble = Animator.StringToHash("Stumble");
        private static readonly int Accelerate = Animator.StringToHash("Accelerate");

        public void PlayDoorOpenAnimation()
        {
            _animator.SetBool(DoorOpen,true);
        }
        
        public void PlayDoorCloseAnimation()
        {
            _animator.SetBool(DoorOpen,false);
        }

        public void PlayStumbleAnimation()
        {
            _animator.SetTrigger(Stumble);
        }

        public void PlayAccelerateAnimation()
        {
            _animator.SetTrigger(Accelerate);
        }
    }
}