using System.Collections;
using System.Collections.Generic;
using CLJ.Runtime.AStar;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Stickman : MonoBehaviour
    {
        public float moveSpeed = 5f;
        private Pathfinder _pathfinder;
        private bool _isMoving;

        private Vector2Int _gridPosition;

        public void Init(Pathfinder pathfinder, Vector2Int position)
        {
            _pathfinder = pathfinder;
            _gridPosition = position;

        }

        public void MoveTo(Vector2Int targetPosition)
        {
            List<Vector2Int> path = _pathfinder.FindPath(_gridPosition, targetPosition);

            if (path == null || path.Count == 0)
                return;
            
            _gridPosition = targetPosition;
            StartCoroutine(FollowPath(path));
        }

        private IEnumerator FollowPath(List<Vector2Int> path)
        {
            _isMoving = true;

            foreach (var point in path)
            {
                Vector3 startPosition = transform.position;
                Vector3 endPosition = new Vector3(point.x, 0, point.y);
                float journeyLength = Vector3.Distance(startPosition, endPosition);
                float startTime = Time.time;

                while (transform.position != endPosition)
                {
                    _isMoving = true;
                    float distCovered = (Time.time - startTime) * moveSpeed;
                    float fractionOfJourney = distCovered / journeyLength;
                    transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
                    yield return null;
                }
            }

            _isMoving = false;
        }
    }
}