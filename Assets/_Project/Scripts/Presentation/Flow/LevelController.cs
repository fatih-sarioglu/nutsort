using System;
using NutSort.Core;
using NutSort.Data;

namespace NutSort.Presentation
{
    // created on level start, disposed when the level ends
    public class LevelController : IDisposable
    {
        private readonly LevelData _level;
        private readonly GameSession _session;
        private readonly BoardView _board;
        private readonly BoardInput _input;
        private readonly HudView _hud;

        private int _selectedBolt = -1;

        public LevelController(LevelData level, BoardView board, BoardInput input, HudView hud)
        {
            _level = level;
            _session = new GameSession(level.Board);
            _board = board;
            _input = input;
            _hud = hud;

            _input.BoltClicked += OnBoltClicked;
            _hud.UndoClicked += OnUndoClicked;
            _hud.RestartClicked += OnRestartClicked;
            _hud.HintClicked += OnHintClicked;
            _session.MoveApplied += OnMoveApplied;
            _session.MoveUndone += OnMoveUndone;
            _session.Restarted += OnRestarted;
            _session.LevelWon += OnLevelWon;

            _board.Bind(_session);
            _hud.SetLevel(_level.Id);
            _hud.HideWin();
            RefreshHud();
        }

        public void Dispose()
        {
            _input.BoltClicked -= OnBoltClicked;
            _hud.UndoClicked -= OnUndoClicked;
            _hud.RestartClicked -= OnRestartClicked;
            _hud.HintClicked -= OnHintClicked;
            _session.MoveApplied -= OnMoveApplied;
            _session.MoveUndone -= OnMoveUndone;
            _session.Restarted -= OnRestarted;
            _session.LevelWon -= OnLevelWon;
            _board.Unbind();
        }

        private void OnBoltClicked(int bolt)
        {
            if (_session.IsWon)
                return;

            if (_selectedBolt < 0)
            {
                TrySelect(bolt);
                return;
            }

            if (_selectedBolt == bolt)
            {
                ClearSelection();
                return;
            }

            int from = _selectedBolt;
            _selectedBolt = -1;

            if (_session.TryMove(new Move(from, bolt)))
                return;

            // cant move there, select the clicked bolt instead or shake
            if (CanPickUp(bolt))
                TrySelect(bolt);
            else
                _board.RejectSelection();
        }

        private bool CanPickUp(int bolt)
        {
            var state = _session.Current;
            return !state.IsEmpty(bolt) && !state.IsCompleted(bolt);
        }

        private void TrySelect(int bolt)
        {
            if (!CanPickUp(bolt))
                return;

            _selectedBolt = bolt;
            _board.Select(bolt);
        }

        private void ClearSelection()
        {
            _selectedBolt = -1;
            _board.Deselect();
        }

        private void OnUndoClicked()
        {
            ClearSelection();
            _session.Undo();
        }

        private void OnRestartClicked()
        {
            ClearSelection();
            _session.Restart();
        }

        private void OnHintClicked()
        {
            if (_session.IsWon)
                return;

            ClearSelection();
            var result = BfsSolver.Solve(_session.Current);

            if (result.IsSolved && result.MinMoves > 0)
                _board.ShowHint(result.Path[0]);
            else if (result.Status == SolverStatus.Unsolvable)
                _hud.SetMessage("No way out from here. Try undo or restart.");
        }

        private void OnMoveApplied(Move move, int nutCount) => RefreshHud();

        private void OnMoveUndone(Move move, int nutCount) => RefreshHud();

        private void OnRestarted(BoardState state)
        {
            _hud.HideWin();
            RefreshHud();
        }

        private void OnLevelWon()
        {
            _selectedBolt = -1;
            int moves = _session.MoveCount;
            int stars = StarRating.Calculate(moves, _level.MinMoves);

            // wait for animations before showing the panel
            _board.PlayWin(() => _hud.ShowWin(stars, moves, _level.MinMoves));
        }

        private void RefreshHud()
        {
            _hud.SetMoves(_session.MoveCount);
            _hud.SetUndoInteractable(_session.CanUndo);
            _hud.SetMessage("");
        }
    }
}
