using System;
using DG.Tweening;
using UnityEngine;

namespace CLJ.Runtime
{
    public class ExitGate : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _confettiVFX;
        [SerializeField] private GameObject _barrier;
        
        private float _elapsedTime;

        private Sequence _raiseSequence;

        public void RaiseBarrier()
        {
            _raiseSequence?.Kill();
            _raiseSequence = DOTween.Sequence();
            
            _raiseSequence
                .Append(_barrier.transform.DORotate(new Vector3(0, 0, 60f), 0.25f)
                    .OnComplete(() =>
                    {
                        _confettiVFX.Play();
                    }))
                .Append(_barrier.transform.DORotate(new Vector3(0, 0, 0), 0.25f).SetDelay(2f));
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Car car))
            {
                RaiseBarrier();
            }            
        }
    }
}