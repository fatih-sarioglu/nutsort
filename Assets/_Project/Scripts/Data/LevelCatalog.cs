using UnityEngine;

namespace NutSort.Data
{
    // Level files are referenced as TextAssets so loading works the same on every platform
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "NutSort/Level Catalog")]
    public class LevelCatalog : ScriptableObject
    {
        [SerializeField] private TextAsset[] _levels;

        public int Count => _levels.Length;

        public LevelData Load(int index) => LevelParser.Parse(_levels[index].text);
    }
}
