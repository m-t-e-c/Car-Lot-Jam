using UnityEngine;

namespace CLJ.Scripts
{
    public class Stickman : MonoBehaviour
    {
        [SerializeField] private StickmanAnimation _stickmanAnimation;
        [SerializeField] private Outline _outline;
        
        public void SetSelected()
        {
            _outline.enabled = !_outline.enabled;
            _stickmanAnimation.PlaySelectedAnimation();
        }
    }
}