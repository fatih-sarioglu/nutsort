using NutSort.Data;
using UnityEngine;

namespace NutSort.Presentation
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private LevelCatalog _levels;
        [SerializeField] private BoardView _board;
        [SerializeField] private BoardInput _input;
        [SerializeField] private HudView _hud;
        [SerializeField] private MenuView _menu;
        [SerializeField] private CameraFramer _cameraFramer;

        private LevelController _currentLevel;
        private int _levelIndex;

        private void Start()
        {
            _hud.NextClicked += OnNextClicked;
            _hud.MenuClicked += ShowMenu;
            _menu.PlayClicked += OnPlayClicked;
            _menu.LevelSelected += StartLevel;
            _board.Built += _cameraFramer.Frame;

            _menu.BuildLevelButtons(_levels.Count);
            ShowMenu();
        }

        private void OnDestroy()
        {
            _hud.NextClicked -= OnNextClicked;
            _hud.MenuClicked -= ShowMenu;
            _menu.PlayClicked -= OnPlayClicked;
            _menu.LevelSelected -= StartLevel;
            _board.Built -= _cameraFramer.Frame;
            _currentLevel?.Dispose();
        }

        private void ShowMenu()
        {
            // board stays behind the menu but ignores input
            _currentLevel?.Dispose();
            _currentLevel = null;

            _hud.HideWin();
            _hud.SetVisible(false);
            _menu.Show();
        }

        private void StartLevel(int index)
        {
            _menu.Hide();
            _hud.SetVisible(true);
            LoadLevel(index);
        }

        private void OnPlayClicked() => StartLevel(0);

        private void LoadLevel(int index)
        {
            _currentLevel?.Dispose();

            // back to level 1 after the last one
            _levelIndex = index % _levels.Count;
            var level = _levels.Load(_levelIndex);
            _currentLevel = new LevelController(level, _board, _input, _hud);
        }

        private void OnNextClicked() => LoadLevel(_levelIndex + 1);
    }
}
