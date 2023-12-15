using CLJ.Runtime.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLJ.Runtime.Views
{
    public class GameplayPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelLabel;
        [SerializeField] private Button restartGameButton;

        private GameplayModel _model;

        private void Start()
        {
            _model = new GameplayModel();
            restartGameButton.onClick.AddListener(() =>
            {
                _model.RestartLevel();
            });
            
            InitializeLevelLabel();
        }
        
        private void InitializeLevelLabel()
        {
            levelLabel.text = $"Lvl {_model.CurrentLevel}";
        }
    }
}