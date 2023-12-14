using System;
using System.Collections;
using System.Collections.Generic;
using CLJ.Runtime.AStar;
using CLJ.Runtime.Level;
using DG.Tweening;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Car : MonoBehaviour
    {
        [SerializeField] private Transform _doorEnterTransform;
        [SerializeField] private Transform _seatTransform;
        [SerializeField] private CarAnimations _carAnimations;
        [SerializeField] private GridObjectColorSetter _gridObjectColorSetter;
        [SerializeField] private Outline _outline;

        private CellColor _cellColor;
        
        private Vector2Int _gridPosition;
        private CellDirection _direction;
        private List<Vector2Int> _aroundCells;
        private Pathfinder _pathfinder;
        
        public void Init(CellColor color, Pathfinder pathfinder, Vector2Int gridPosition, CellDirection direction, List<Vector2Int> aroundCells)
        {
            _aroundCells = aroundCells;
            _direction = direction;
            _gridPosition = gridPosition;
            _pathfinder = pathfinder;
            _cellColor = color;
            _gridObjectColorSetter.SetColor(_cellColor);
        }
        
        public CellColor GetColor()
        {
            return _cellColor;
        }
        
        public void SetStickman(Stickman stickman)
        {
            _carAnimations.PlayDoorOpenAnimation();
            stickman.transform.DOMove(_doorEnterTransform.position, 2f).OnComplete(() =>
            {
                _carAnimations.PlayDoorCloseAnimation();
                stickman.transform.parent = _seatTransform;
            });
            stickman.GetComponent<StickmanAnimation>().PlayEnterCarAnimation();
            _outline.enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Move();
            }
            
            if (Input.GetKeyDown(KeyCode.A))
            {
                MoveTo(_pathfinder.GetLastNode().Coordinate);
            }
        }

        public void Move()
        {
            Vector2Int key = Vector2Int.zero;
            Vector2Int target = Vector2Int.zero;
            if (_direction == CellDirection.Right)
            {
                var pathXMax = _pathfinder.GetPathWidth();
                key = new Vector2Int(pathXMax, _gridPosition.y + 1);
                target = _pathfinder.GetNode(key).Position;
            }
            else if (_direction == CellDirection.Left)
            {
                key = new Vector2Int(0, _gridPosition.y + 1);
                target = _pathfinder.GetNode(key).Position;

            }
            else if (_direction == CellDirection.Up)
            {
                key = new Vector2Int(_gridPosition.x + 1,0);
                target = _pathfinder.GetNode(key).Position;
            }
            else if (_direction == CellDirection.Down)
            {
                var pathYMax =_pathfinder.GetPathHeight();
                key = new Vector2Int(_gridPosition.x + 1, pathYMax);
                target = _pathfinder.GetNode(key).Position;
            }
          
            transform.DOMove(new Vector3(target.x,0,target.y), 1f)
                .OnComplete(() =>
                {
                    _gridPosition = key;
                });
        }
        
        public void MoveTo(Vector2Int targetPosition, Action onMoveComplete = null)
        {
            List<Vector2Int> path = _pathfinder.FindPath(_gridPosition, targetPosition);

            if (path == null || path.Count == 0)
            {
                return;
            }

            _gridPosition = targetPosition;
            StartCoroutine(FollowPath(path, onMoveComplete));
        }
        
        private IEnumerator FollowPath(List<Vector2Int> path, Action onMoveComplete = null)
        {
            foreach (var point in path)
            {
                Vector3 startPosition = transform.position;
                Vector3 endPosition = new Vector3(point.x, 0, point.y);
                float journeyLength = Vector3.Distance(startPosition, endPosition);
                float startTime = Time.time;

                while (transform.position != endPosition)
                {
                    float distCovered = (Time.time - startTime) * 5;
                    float fractionOfJourney = distCovered / journeyLength;
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(endPosition - startPosition), Time.deltaTime * 10f);
                    transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
                    yield return null;
                }
            }

            onMoveComplete?.Invoke();
        }

        public List<Vector2Int> GetAroundCells()
        {
            return _aroundCells;
        }

        public void Highlight()
        {
            _outline.enabled = true;
        }
    }
}