using System;
using System.Collections;
using System.Collections.Generic;
using CLJ.Runtime.AStar;
using CLJ.Runtime.Level;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Stickman : MonoBehaviour
    {
        private static readonly int SelectedHash = Animator.StringToHash("Selected");
        private static readonly int IsMovingHash = Animator.StringToHash("Moving");
        private static readonly int EnterTheCarHash = Animator.StringToHash("EnterTheCar");

        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem angerEmojiVFX;
        [SerializeField] private ParticleSystem happyEmojiVFX;
        [SerializeField] private GridObjectColorSetter gridObjectColorSetter;
        [SerializeField] private Outline outline;
        [SerializeField] private float moveSpeed;

        private Pathfinder _pathfinder;
        private Vector2Int _gridPosition;
        private CellColor _cellColor;
        
        public bool IsMoving { get; private set; }

        public void Init(Pathfinder pathfinder, Vector2Int position, CellColor color)
        {
            _pathfinder = pathfinder;
            _gridPosition = position;
            _cellColor = color;
            gridObjectColorSetter.SetColor(_cellColor);
        }

        public void SetSelected()
        {
            animator.SetTrigger(SelectedHash);
            outline.enabled = !outline.enabled;
        }

        public void CancelSelection()
        {
            outline.enabled = false;
        }

        public void PlayHappyEmoji()
        {
            happyEmojiVFX.Play();
        }

        public void PlayAngerEmoji()
        {
            angerEmojiVFX.Play();
        }

        public void PlayEnterCarAnimation()
        {
            animator.SetTrigger(EnterTheCarHash);
        }

        public Vector2Int GetGridPosition()
        {
            return _gridPosition;
        }

        public CellColor GetColor()
        {
            return _cellColor;
        }
        
        public bool MoveTo(Vector2Int targetPosition, Action onMoveComplete = null, Action<Vector2Int> onPathFailed = null)
        {
            if (_gridPosition.Equals(targetPosition))
            {
                onMoveComplete?.Invoke();
                return true;
            }
            
            List<Vector2Int> path = _pathfinder.FindPath(_gridPosition, targetPosition, onPathFailed);
            if (path == null || path.Count == 0)
            {
                return false;
            }

            _gridPosition = targetPosition;
            StartCoroutine(FollowPath(path, onMoveComplete));
            return true;
        }

        private void ChangeMovingState(bool state)
        {
            IsMoving = state;
            animator.SetBool(IsMovingHash, state);
        }

        private IEnumerator FollowPath(List<Vector2Int> path, Action onMoveComplete)
        {
            ChangeMovingState(true);
            foreach (Vector2Int point in path)
            {
                yield return MoveToPoint(point);
            }
            ChangeMovingState(false);
            onMoveComplete?.Invoke();
        }

        private IEnumerator MoveToPoint(Vector2Int point)
        {
            Vector3 startPosition = transform.position;
            Vector3 endPosition = new Vector3(point.x, 0, point.y);
            float journeyLength = Vector3.Distance(startPosition, endPosition);
            float startTime = Time.time;

            while (transform.position != endPosition)
            {
                IsMoving = true;
                float distCovered = (Time.time - startTime) * moveSpeed;
                float fractionOfJourney = distCovered / journeyLength;
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(endPosition - startPosition), Time.deltaTime * 10f);
                transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
                yield return null;
            }
            
            transform.rotation = Quaternion.LookRotation(endPosition - startPosition);
            IsMoving = false;
        }
    }
}
