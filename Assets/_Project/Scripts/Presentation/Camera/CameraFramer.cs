using UnityEngine;

namespace NutSort.Presentation
{
    // fits the whole board on screen
    [RequireComponent(typeof(Camera))]
    public class CameraFramer : MonoBehaviour
    {
        [Tooltip("space around the board, leave some for the hud")]
        [SerializeField, Range(1f, 2f)] private float _padding = 1.25f;
        [SerializeField] private float _distance = 20f;

        private Camera _camera;
        private Bounds _target;
        private bool _hasTarget;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
        }

        public void Frame(Bounds target)
        {
            _target = target;
            _hasTarget = true;
            Apply();
        }

        private void LateUpdate()
        {
            if (_hasTarget && (Screen.width != _lastWidth || Screen.height != _lastHeight))
                Apply();
        }

        private void Apply()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            // camera is rotated so check every corner of the box
            var toCameraSpace = Quaternion.Inverse(transform.rotation);
            var extents = _target.extents;
            float halfWidth = 0f;
            float halfHeight = 0f;

            for (int i = 0; i < 8; i++)
            {
                var sign = new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f);
                var local = toCameraSpace * Vector3.Scale(extents, sign);
                halfWidth = Mathf.Max(halfWidth, Mathf.Abs(local.x));
                halfHeight = Mathf.Max(halfHeight, Mathf.Abs(local.y));
            }

            // ortho size is half of the height, width is size * aspect
            _camera.orthographicSize = Mathf.Max(halfHeight, halfWidth / _camera.aspect) * _padding;
            transform.position = _target.center - transform.forward * _distance;
        }
    }
}
