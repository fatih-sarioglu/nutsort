using System;
using System.Collections.Generic;
using DG.Tweening;
using NutSort.Core;
using UnityEngine;

namespace NutSort.Presentation
{
    // shows the board and animates the session events
    // animations from the same frame go into one sequence so they play in order
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private BoltView _boltPrefab;
        [SerializeField] private NutView _nutPrefab;
        [SerializeField] private ColorPalette _palette;
        [SerializeField] private AnimationConfig _animation;

        [Header("Layout")]
        [SerializeField] private float _boltSpacing = 2.6f;
        [SerializeField] private float _rowGap = 0.6f;
        [SerializeField] private int _maxBoltsPerRow = 5;

        private readonly List<BoltView> _bolts = new List<BoltView>();
        private readonly List<List<NutView>> _stacks = new List<List<NutView>>();

        private GameSession _session;
        private int _selectedBolt = -1;
        private int _liftedCount;
        private Move? _hint;

        private Sequence _timeline;
        private int _timelineFrame = -1;

        // for the camera
        public event Action<Bounds> Built;

        public void Bind(GameSession session)
        {
            Unbind();
            _session = session;
            _session.MoveApplied += OnMoveApplied;
            _session.MoveUndone += OnMoveUndone;
            _session.BoltCompleted += OnBoltCompleted;
            _session.BoltUncompleted += OnBoltUncompleted;
            _session.Restarted += Rebuild;
            Rebuild(session.Current);
        }

        public void Unbind()
        {
            KillAnimations();
            if (_session == null)
                return;

            _session.MoveApplied -= OnMoveApplied;
            _session.MoveUndone -= OnMoveUndone;
            _session.BoltCompleted -= OnBoltCompleted;
            _session.BoltUncompleted -= OnBoltUncompleted;
            _session.Restarted -= Rebuild;
            _session = null;
        }

        private void OnDestroy() => Unbind();

        public void Select(int bolt)
        {
            var timeline = Timeline();
            if (_selectedBolt >= 0)
                AppendDrop(timeline, _selectedBolt);

            _selectedBolt = bolt;
            _liftedCount = _session.Current.GetTopGroupSize(bolt);

            // same timing for all so they dont overlap
            float start = timeline.Duration();
            var stack = _stacks[bolt];
            for (int i = 0; i < _liftedCount; i++)
            {
                int slot = stack.Count - _liftedCount + i;
                var nut = stack[slot];
                var from = _bolts[bolt].GetNutPosition(slot);
                var to = _bolts[bolt].GetLiftPosition(i);
                timeline.Insert(start, nut.transform.DOMove(to, _animation.LiftDuration).SetEase(_animation.LiftEase));
                timeline.Insert(start, Spin(nut, from, to, _animation.LiftDuration));
            }
        }

        public void Deselect()
        {
            if (_selectedBolt < 0)
                return;

            AppendDrop(Timeline(), _selectedBolt);
            _selectedBolt = -1;
        }

        // wrong target, shake and drop back
        public void RejectSelection()
        {
            if (_selectedBolt < 0)
                return;

            var timeline = Timeline();
            float start = timeline.Duration();
            foreach (var nut in LiftedNuts(_selectedBolt))
            {
                timeline.Insert(start, nut.transform.DOShakePosition(
                    _animation.ShakeDuration, new Vector3(_animation.ShakeStrength, 0f, 0f), _animation.ShakeVibrato, 0f, false, false));
            }

            AppendDrop(timeline, _selectedBolt);
            _selectedBolt = -1;
        }

        public void ShowHint(Move move)
        {
            ClearHint();
            _hint = move;
            _bolts[move.From].SetHighlighted(true);
            _bolts[move.To].SetHighlighted(true);
        }

        public void ClearHint()
        {
            if (!_hint.HasValue)
                return;

            _bolts[_hint.Value.From].SetHighlighted(false);
            _bolts[_hint.Value.To].SetHighlighted(false);
            _hint = null;
        }

        public void PlayWin(Action onFinished)
        {
            var timeline = Timeline();
            float start = timeline.Duration();

            for (int b = 0; b < _bolts.Count; b++)
            {
                float at = start + b * _animation.WinBoltStagger;
                timeline.Insert(at, _bolts[b].transform.DOPunchScale(
                    Vector3.one * _animation.WinPunch, _animation.WinPunchDuration, 6, 0.5f));

                foreach (var nut in _stacks[b])
                {
                    timeline.Insert(at, nut.transform.DOPunchPosition(
                        Vector3.up * _animation.WinNutHop, _animation.WinPunchDuration, 4, 0.5f));
                }
            }

            timeline.AppendInterval(_animation.WinPanelDelay);
            timeline.AppendCallback(() => onFinished?.Invoke());
        }

        private void OnMoveApplied(Move move, int nutCount)
        {
            ClearHint();
            AppendTransfer(Timeline(), move.From, move.To, nutCount);
        }

        // just play the move backwards
        private void OnMoveUndone(Move move, int nutCount)
        {
            ClearHint();
            Deselect();
            AppendTransfer(Timeline(), move.To, move.From, nutCount);
        }

        private void OnBoltCompleted(int bolt)
        {
            Timeline().Append(_bolts[bolt].CreateCapTween(true, _animation));
        }

        private void OnBoltUncompleted(int bolt)
        {
            Timeline().Append(_bolts[bolt].CreateCapTween(false, _animation));
        }

        // bottom nut lands first so they dont go through each other
        // if not everything fits the rest drops back down
        private void AppendTransfer(Sequence timeline, int from, int to, int count)
        {
            bool wasLifted = _selectedBolt == from;
            int leftOver = wasLifted ? _liftedCount - count : 0;
            _selectedBolt = -1;

            var source = _stacks[from];
            var target = _stacks[to];
            int sourceFirst = source.Count - count;
            var moving = source.GetRange(sourceFirst, count);
            source.RemoveRange(sourceFirst, count);

            int firstSlot = target.Count;
            target.AddRange(moving);

            float start = timeline.Duration();
            for (int j = 0; j < count; j++)
            {
                var nut = moving[j];
                var path = DOTween.Sequence();

                // left over nuts are below the moving ones
                var hover = _bolts[from].GetLiftPosition(leftOver + j);
                if (!wasLifted)
                {
                    var rest = _bolts[from].GetNutPosition(sourceFirst + j);
                    path.Append(nut.transform.DOMove(hover, _animation.LiftDuration).SetEase(_animation.LiftEase));
                    path.Join(Spin(nut, rest, hover, _animation.LiftDuration));
                }

                var above = _bolts[to].GetLiftPosition(j);
                var landing = _bolts[to].GetNutPosition(firstSlot + j);
                path.Append(nut.transform.DOJump(above, _animation.TravelArcHeight, 1, _animation.TravelDuration));
                path.AppendInterval(j * _animation.NutStagger);
                path.Append(nut.transform.DOMove(landing, _animation.DropDuration).SetEase(_animation.DropEase));
                path.Join(Spin(nut, above, landing, _animation.DropDuration));

                timeline.Insert(start, path);
            }

            // drop the ones that didnt fit
            for (int i = 0; i < leftOver; i++)
            {
                int slot = source.Count - leftOver + i;
                var nut = source[slot];
                var lifted = _bolts[from].GetLiftPosition(i);
                var rest = _bolts[from].GetNutPosition(slot);
                timeline.Insert(start, nut.transform.DOMove(rest, _animation.DropDuration).SetEase(_animation.DropEase));
                timeline.Insert(start, Spin(nut, lifted, rest, _animation.DropDuration));
            }
        }

        private void AppendDrop(Sequence timeline, int bolt)
        {
            float start = timeline.Duration();
            var stack = _stacks[bolt];
            int first = stack.Count - _liftedCount;

            for (int i = 0; i < _liftedCount; i++)
            {
                var nut = stack[first + i];
                var lifted = _bolts[bolt].GetLiftPosition(i);
                var rest = _bolts[bolt].GetNutPosition(first + i);
                timeline.Insert(start, nut.transform.DOMove(rest, _animation.DropDuration).SetEase(_animation.DropEase));
                timeline.Insert(start, Spin(nut, lifted, rest, _animation.DropDuration));
            }
        }

        private IEnumerable<NutView> LiftedNuts(int bolt)
        {
            var stack = _stacks[bolt];
            for (int i = stack.Count - _liftedCount; i < stack.Count; i++)
                yield return stack[i];
        }

        // screw effect, full turns only so the nut lines up with the thread at the end
        private Tween Spin(NutView nut, Vector3 from, Vector3 to, float duration)
        {
            float deltaY = to.y - from.y;
            float slots = Mathf.Abs(deltaY) / _bolts[0].SlotHeight;
            int turns = Mathf.Clamp(Mathf.RoundToInt(slots * _animation.SpinTurnsPerSlot), 1, _animation.MaxSpinTurns);
            float degrees = turns * 360f * Mathf.Sign(deltaY) * _animation.SpinDirection;
            return nut.transform.DOLocalRotate(new Vector3(0f, degrees, 0f), duration, RotateMode.LocalAxisAdd);
        }

        // new frame = finish the old sequence and start a new one
        private Sequence Timeline()
        {
            if (_timeline != null && _timeline.IsActive() && _timelineFrame == Time.frameCount)
                return _timeline;

            if (_timeline != null && _timeline.IsActive())
                _timeline.Complete(true);

            _timeline = DOTween.Sequence().SetLink(gameObject);
            _timelineFrame = Time.frameCount;
            return _timeline;
        }

        private void KillAnimations()
        {
            if (_timeline != null && _timeline.IsActive())
                _timeline.Kill();
            _timeline = null;
        }

        private void Rebuild(BoardState state)
        {
            KillAnimations();
            Clear();

            for (int b = 0; b < state.BoltCount; b++)
            {
                var bolt = Instantiate(_boltPrefab, transform.position, Quaternion.identity, transform);
                bolt.name = $"Bolt {b}";
                bolt.Setup(b, state.Capacity);
                bolt.transform.position = GetBoltPosition(b, state.BoltCount, bolt.Height);
                _bolts.Add(bolt);

                var stack = new List<NutView>(state.Capacity);
                for (int i = 0; i < state.GetCount(b); i++)
                {
                    byte color = state.GetColor(b, i);
                    var nut = Instantiate(_nutPrefab, bolt.GetNutPosition(i), Quaternion.identity, transform);
                    nut.SetMaterial(_palette.GetMaterial(color));
                    stack.Add(nut);
                }
                _stacks.Add(stack);

                bolt.SetCapped(state.IsCompleted(b));
            }

            Built?.Invoke(CalculateBounds());
        }

        private Bounds CalculateBounds()
        {
            var bounds = _bolts[0].WorldBounds;
            for (int i = 1; i < _bolts.Count; i++)
                bounds.Encapsulate(_bolts[i].WorldBounds);
            return bounds;
        }

        private void Clear()
        {
            _selectedBolt = -1;
            _liftedCount = 0;
            _hint = null;
            _bolts.Clear();
            _stacks.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }

        // 2 rows if there are too many bolts
        private Vector3 GetBoltPosition(int index, int count, float boltHeight)
        {
            int rows = count <= _maxBoltsPerRow ? 1 : 2;
            int perRow = Mathf.CeilToInt(count / (float)rows);
            int row = index / perRow;
            int column = index % perRow;
            int boltsInRow = row == rows - 1 ? count - perRow * (rows - 1) : perRow;

            float x = (column - (boltsInRow - 1) / 2f) * _boltSpacing;
            float y = (rows - 1 - row) * (boltHeight + _rowGap);
            return transform.position + new Vector3(x, y, 0f);
        }
    }
}
