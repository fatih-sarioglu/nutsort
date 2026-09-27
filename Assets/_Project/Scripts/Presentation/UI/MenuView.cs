using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NutSort.Presentation
{
    public class MenuView : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Transform _levelGrid;
        [SerializeField] private Button _levelButtonPrefab;

        public event Action PlayClicked;
        public event Action<int> LevelSelected;

        private void Awake() => _playButton.onClick.AddListener(() => PlayClicked?.Invoke());

        public void BuildLevelButtons(int levelCount)
        {
            for (int i = _levelGrid.childCount - 1; i >= 0; i--)
                Destroy(_levelGrid.GetChild(i).gameObject);

            for (int i = 0; i < levelCount; i++)
            {
                int index = i;
                var button = Instantiate(_levelButtonPrefab, _levelGrid);
                button.name = $"Level {index + 1}";
                button.GetComponentInChildren<TMP_Text>().text = (index + 1).ToString();
                button.onClick.AddListener(() => LevelSelected?.Invoke(index));
            }
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);
    }
}
