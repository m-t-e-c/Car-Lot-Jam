using System;
using CLJ.Runtime.AStar;
using DG.Tweening;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Ground : MonoBehaviour
    {
        [SerializeField] private Renderer myRenderer;
        [SerializeField] private LayerMask occupyLayer;

        private Node _node;
        private Material _material;
        private Color _startColor;

        private Sequence _highlightSequence;
        public bool isOccupied;

        private void Start()
        {
            _material = new Material(myRenderer.sharedMaterial);
            myRenderer.sharedMaterial = _material;
            _startColor = myRenderer.sharedMaterial.color;
        }

        private void FixedUpdate()
        {
            if (_node == null) return;
            isOccupied = Physics.CheckBox(transform.position, Vector3.one * 0.5f, Quaternion.identity, occupyLayer);
            _node.IsOccupied = isOccupied;
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
                    .Append(myRenderer.sharedMaterial.DOColor(new Color32(0,255,0,255), 0.3f))
                    .OnComplete(() => myRenderer.sharedMaterial.DOColor(_startColor, 0.3f));
            }
            else
            {
                _highlightSequence
                    .Append(myRenderer.sharedMaterial.DOColor(new Color32(255,0,0,255), 0.3f))
                    .OnComplete(() => myRenderer.sharedMaterial.DOColor(_startColor, 0.3f));
            }
        }

        public Vector2Int GetCoordinates()
        {
            return _node.Coordinate;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}