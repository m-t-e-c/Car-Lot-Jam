using System;
using DG.Tweening;
using UnityEngine;

namespace CLJ.Runtime
{
    public class ExitGate : MonoBehaviour
    {
        
        [SerializeField] private ParticleSystem confettiVFX;
        [SerializeField] private GameObject barrier;
        
        private Sequence _raiseSequence;
        private float _elapsedTime;

        private void RaiseBarrier()
        {
            _raiseSequence?.Kill();
            _raiseSequence = DOTween.Sequence();
            
            _raiseSequence
                .Append(barrier.transform.DORotate(new Vector3(0, 0, 60f), 0.25f)
                    .OnComplete(() =>
                    {
                        confettiVFX.Play();
                    }))
                .Append(barrier.transform.DORotate(new Vector3(0, 0, 0), 0.25f).SetDelay(2f));
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Car car))
            {
                RaiseBarrier();
                GameEvents.OnCarPassedThroughTheGate?.Invoke();
            }            
        }
    }
}