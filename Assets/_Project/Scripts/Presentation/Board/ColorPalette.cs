using System;
using System.Collections.Generic;
using NutSort.Data;
using UnityEngine;

namespace NutSort.Presentation
{
    // one shared material per color
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "NutSort/Color Palette")]
    public class ColorPalette : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string Key;
            public Color Color;
        }

        [SerializeField] private Material _baseMaterial;
        [SerializeField] private Entry[] _entries;

        private readonly Dictionary<byte, Material> _cache = new Dictionary<byte, Material>();

        // cache stays between plays in the editor otherwise
        private void OnEnable() => _cache.Clear();

        public Material GetMaterial(byte colorId)
        {
            if (_cache.TryGetValue(colorId, out var cached))
                return cached;

            string key = NutColors.GetKey(colorId);
            var material = new Material(_baseMaterial) { name = $"Nut_{key}", color = FindColor(key) };
            _cache.Add(colorId, material);
            return material;
        }

        private Color FindColor(string key)
        {
            foreach (var entry in _entries)
            {
                if (entry.Key == key)
                    return entry.Color;
            }

            Debug.LogError($"No color for '{key}' in the palette");
            return Color.magenta;
        }
    }
}
