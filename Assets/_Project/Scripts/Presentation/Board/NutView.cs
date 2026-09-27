using UnityEngine;

namespace NutSort.Presentation
{
    public class NutView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        public void SetMaterial(Material material) => _renderer.sharedMaterial = material;
    }
}
