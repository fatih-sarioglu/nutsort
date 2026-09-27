using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace NutSort.Presentation
{
    public class BoardInput : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _boltLayers = ~0;
        [SerializeField] private float _maxDistance = 100f;

        public event Action<int> BoltClicked;

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
                return;

            // ignore clicks on UI buttons
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var ray = _camera.ScreenPointToRay(pointer.position.ReadValue());
            if (Physics.Raycast(ray, out var hit, _maxDistance, _boltLayers) &&
                hit.collider.TryGetComponent<BoltView>(out var bolt))
            {
                BoltClicked?.Invoke(bolt.Index);
            }
        }
    }
}
