using CLJ.Runtime.Models;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLJ.Runtime.Presenters
{
    public class GameOverPresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverLabel;
        [SerializeField] private Button nextLevelButton;

        private GameOverModel _model;
        
        private void Start()
        {
            _model = new GameOverModel();
            nextLevelButton.onClick.AddListener(() =>
            {
                _model.LoadNextLevel();
            });

            InitializeLevelLabel();
            PlayFadeInAnimation();
        }
        
        private void InitializeLevelLabel()
        {
            gameOverLabel.text = $"Level {_model.CurrentLevelIndex} completed!";
        }

        private void PlayFadeInAnimation()
        {
            gameOverPanel.alpha = 0;
            gameOverPanel.interactable = false;
            gameOverPanel.DOFade(1, 0.5f).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                gameOverPanel.interactable = true;
            });
        }
    }
}