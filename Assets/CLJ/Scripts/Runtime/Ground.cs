using System;
using CLJ.Runtime.AStar;
using DG.Tweening;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Ground : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private LayerMask _occupyLayer;

        private Node _node;
        private Material _material;
        private Color _startColor;

        private Sequence _highlightSequence;

        private void Start()
        {
            _material = new Material(_renderer.sharedMaterial);
            _renderer.sharedMaterial = _material;
            _startColor = _renderer.sharedMaterial.color;
        }

        private void FixedUpdate()
        {
            if (_node == null) return;
            _node.IsOccupied =
                Physics.CheckBox(transform.position, Vector3.one * 0.5f, Quaternion.identity, _occupyLayer);
        }

        public void SetNode(Node node)
        {
            _node = node;
        }

        public void Highlight(bool canMovable)
        {
            _highlightSequence?.Kill();
            _highlightSequence = DOTween.Sequence();

            if (canMovable)
            {
                _highlightSequence
                    .Append(_renderer.sharedMaterial.DOColor(new Color32(0,255,0,255), 0.3f))
                    .OnComplete(() => _renderer.sharedMaterial.DOColor(_startColor, 0.3f));
            }
            else
            {
                _highlightSequence
                    .Append(_renderer.sharedMaterial.DOColor(new Color32(255,0,0,255), 0.3f))
                    .OnComplete(() => _renderer.sharedMaterial.DOColor(_startColor, 0.3f));
            }
        }

        public Vector2Int GetCoordinates()
        {
            return _node.Position;
        }

        private void OnDrawGizmos()
        {
            if (_node == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}