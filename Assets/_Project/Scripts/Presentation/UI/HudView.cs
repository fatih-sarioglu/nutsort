using System;
using DG.Tweening;
using NutSort.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NutSort.Presentation
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] private GameObject _gameplayRoot;

        [Header("Top bar")]
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _movesLabel;
        [SerializeField] private TMP_Text _messageLabel;

        [Header("Buttons")]
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _hintButton;
        [SerializeField] private Button _menuButton;

        [Header("Win panel")]
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private TMP_Text _winLabel;
        [SerializeField] private Button _nextButton;

        public event Action UndoClicked;
        public event Action RestartClicked;
        public event Action HintClicked;
        public event Action NextClicked;
        public event Action MenuClicked;

        private void Awake()
        {
            _undoButton.onClick.AddListener(() => UndoClicked?.Invoke());
            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            _hintButton.onClick.AddListener(() => HintClicked?.Invoke());
            _nextButton.onClick.AddListener(() => NextClicked?.Invoke());
            _menuButton.onClick.AddListener(() => MenuClicked?.Invoke());
        }

        public void SetVisible(bool visible) => _gameplayRoot.SetActive(visible);

        public void SetLevel(int levelId) => _levelLabel.text = $"Level {levelId}";

        public void SetMoves(int moves) => _movesLabel.text = $"Moves: {moves}";

        public void SetMessage(string message) => _messageLabel.text = message;

        public void SetUndoInteractable(bool interactable) => _undoButton.interactable = interactable;

        public void ShowWin(int stars, int moves, int minMoves)
        {
            _winLabel.text = $"Solved in {moves} moves (best: {minMoves})\n{stars} / {StarRating.MaxStars} stars";
            _winPanel.SetActive(true);

            var panel = _winPanel.transform;
            panel.DOKill();
            panel.localScale = Vector3.one * 0.8f;
            panel.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetLink(_winPanel);
        }

        public void HideWin()
        {
            _winPanel.transform.DOKill();
            _winPanel.SetActive(false);
        }
    }
}
