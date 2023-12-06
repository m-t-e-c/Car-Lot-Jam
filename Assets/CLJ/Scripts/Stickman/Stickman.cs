using UnityEngine;

namespace CLJ.Scripts
{
    public class Stickman : MonoBehaviour
    {
        [SerializeField] private StickmanAnimation _stickmanAnimation;
        
        public void SetSelected()
        {
            _stickmanAnimation.PlaySelectedAnimation();
        }
    }
}