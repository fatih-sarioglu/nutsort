using DG.Tweening;
using UnityEngine;

namespace NutSort.Presentation
{
    // bolt is head + segments + tip, one segment for each slot
    // fbx scale factor is 100, head pivot is at the top, the rest at the bottom
    [RequireComponent(typeof(BoxCollider))]
    public class BoltView : MonoBehaviour
    {
        [Header("Parts")]
        [SerializeField] private Transform _head;
        [SerializeField] private GameObject _segmentPrefab;
        [SerializeField] private Transform _tip;
        [SerializeField] private GameObject _cap;
        [SerializeField] private Renderer _highlightRenderer;

        [Header("Model dimensions")]
        [Tooltip("segment height, also the gap between nuts")]
        [SerializeField] private float _slotHeight = 0.9f;
        [SerializeField] private float _headHeight = 0.5f;
        [SerializeField] private float _tipHeight = 0.56f;
        [SerializeField] private float _width = 2.2f;

        [Header("Layout")]
        [SerializeField] private float _liftGap = 0.3f;
        [SerializeField] private float _capOffset = 0.1f;
        [SerializeField] private Color _highlightColor = new Color(1f, 0.85f, 0.2f);

        private int _capacity;
        private Color _normalColor;
        private Material _highlightMaterial;

        public int Index { get; private set; }
        public float SlotHeight => _slotHeight;

        // including space for lifted nuts
        public float Height { get; private set; }

        public Bounds WorldBounds =>
            new Bounds(transform.position + Vector3.up * (Height / 2f), new Vector3(_width, Height, _width));

        private float ThreadTop => _headHeight + _capacity * _slotHeight;
        private float TipTop => ThreadTop + _tipHeight;
        private Vector3 CapRest => new Vector3(0f, ThreadTop + _capOffset, 0f);

        private void Awake()
        {
            // copy of the material so each bolt can be highlighted
            _highlightMaterial = _highlightRenderer.material;
            _normalColor = _highlightMaterial.color;
        }

        private void OnDestroy()
        {
            if (_highlightMaterial != null)
                Destroy(_highlightMaterial);
        }

        public void Setup(int index, int capacity)
        {
            Index = index;
            _capacity = capacity;

            _head.localPosition = new Vector3(0f, _headHeight, 0f);
            for (int k = 0; k < capacity; k++)
            {
                var segment = Instantiate(_segmentPrefab, transform);
                segment.name = $"Segment {k}";
                segment.transform.localPosition = new Vector3(0f, _headHeight + k * _slotHeight, 0f);
                segment.transform.localRotation = Quaternion.identity;
            }
            _tip.localPosition = new Vector3(0f, ThreadTop, 0f);

            // collider is bigger than the model so its easier to click
            Height = TipTop + _liftGap + capacity * _slotHeight;
            var box = GetComponent<BoxCollider>();
            box.size = new Vector3(_width, Height, _width);
            box.center = new Vector3(0f, Height / 2f, 0f);

            SetCapped(false);
            SetHighlighted(false);
        }

        // slot 0 is the bottom
        public Vector3 GetNutPosition(int slot) =>
            transform.position + Vector3.up * (_headHeight + slot * _slotHeight);

        // above the tip when selected
        public Vector3 GetLiftPosition(int stackIndex) =>
            transform.position + Vector3.up * (TipTop + _liftGap + stackIndex * _slotHeight);

        public void SetCapped(bool capped)
        {
            _cap.transform.localPosition = CapRest;
            _cap.SetActive(capped);
        }

        // SetActive is inside the sequence, otherwise the cap shows up before the nuts land
        public Tween CreateCapTween(bool capped, AnimationConfig config)
        {
            var cap = _cap.transform;
            var rest = CapRest;
            var raised = rest + Vector3.up * config.CapDropHeight;
            var sequence = DOTween.Sequence();

            if (capped)
            {
                sequence.AppendCallback(() =>
                {
                    cap.localPosition = raised;
                    _cap.SetActive(true);
                });
                sequence.Append(cap.DOLocalMove(rest, config.CapDuration).SetEase(config.CapEase));
                sequence.Join(transform.DOPunchScale(Vector3.one * config.CapPunch, config.CapDuration, 6, 0.5f));
            }
            else
            {
                sequence.Append(cap.DOLocalMove(raised, config.CapDuration * 0.6f).SetEase(Ease.InBack));
                sequence.AppendCallback(() =>
                {
                    _cap.SetActive(false);
                    cap.localPosition = rest;
                });
            }

            return sequence;
        }

        public void SetHighlighted(bool highlighted) =>
            _highlightMaterial.color = highlighted ? _highlightColor : _normalColor;
    }
}
