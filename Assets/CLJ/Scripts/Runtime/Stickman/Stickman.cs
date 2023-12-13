﻿using System;
using System.Collections;
using System.Collections.Generic;
using CLJ.Runtime.AStar;
using CLJ.Runtime.Level;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Stickman : MonoBehaviour
    {
        [SerializeField] private StickmanAnimation _stickmanAnimation;
        [SerializeField] private GridObjectColorSetter gridObjectColorSetter;
        [SerializeField] private Outline _outline;
        [SerializeField] private float _moveSpeed = 5f;

        private Pathfinder _pathfinder;
        private Vector2Int _gridPosition;

        private CellColor _cellColor;
        public bool isMoving;

        public void Init(Pathfinder pathfinder, Vector2Int position, CellColor color)
        {
            _pathfinder = pathfinder;
            _gridPosition = position;

            _cellColor = color;
            gridObjectColorSetter.SetColor(_cellColor);
        }

        public void SetSelected()
        {
            _stickmanAnimation.PlaySelectedAnimation();
            _outline.enabled = _outline.enabled == false;
        }
        
        public void CancelSelection()
        {
            _outline.enabled = false;
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
            isMoving = true;
            _stickmanAnimation.ChangeMovingState(true);

            foreach (var point in path)
            {
                Vector3 startPosition = transform.position;
                Vector3 endPosition = new Vector3(point.x, 0, point.y);
                float journeyLength = Vector3.Distance(startPosition, endPosition);
                float startTime = Time.time;

                while (transform.position != endPosition)
                {
                    float distCovered = (Time.time - startTime) * _moveSpeed;
                    float fractionOfJourney = distCovered / journeyLength;
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(endPosition - startPosition), Time.deltaTime * 10f);
                    transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
                    yield return null;
                }
            }

            isMoving = false;
            onMoveComplete?.Invoke();
            _stickmanAnimation.ChangeMovingState(false);
        }
    }
}