using System;
using System.Collections;
using System.Collections.Generic;
using CLJ.Runtime.AStar;
using CLJ.Runtime.Level;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Car : MonoBehaviour
    {
        [SerializeField] private GridObjectColorSetter _gridObjectColorSetter;
        [SerializeField] private Outline _outline;

        private Pathfinder _pathfinder;
        private CellColor _cellColor;
        private List<Vector2Int> _cellsAround;
        
        private Vector2Int _gridPosition;
        private Vector2Int _lastRoadPosition;

        public void Init(CellColor color, List<Vector2Int> cellsAround, Pathfinder pathfinder, Vector2Int gridPosition, Vector2Int lastRoadPosition)
        {
            _lastRoadPosition = new Vector2Int(-1, -1);
            _gridPosition = new Vector2Int(2,-1);
            _pathfinder = pathfinder;
            _cellsAround = cellsAround;
            _cellColor = color;
            _gridObjectColorSetter.SetColor(_cellColor);
        }
        
        public List<Vector2Int> GetAroundCell()
        {
            return _cellsAround;
        }

        public void Highlight()
        {
            _outline.enabled = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                MoveTo(_lastRoadPosition);
            }
        }

        public bool MoveTo(Vector2Int targetPosition, Action onMoveComplete = null)
        {
            List<Vector2Int> path = _pathfinder.FindPath(_gridPosition, targetPosition);

            if (path == null || path.Count == 0)
            {
                return false;
            }

            _gridPosition = targetPosition;
            StartCoroutine(FollowPath(path, onMoveComplete));
            return true;
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
    }
}