using System;

namespace NutSort.Data
{
    // json uses names, game uses the index as color id
    public static class NutColors
    {
        private static readonly string[] Keys =
        {
            "red", "blue", "green", "yellow", "purple", "orange", "cyan", "pink"
        };

        public static bool TryGetId(string key, out byte id)
        {
            int index = Array.IndexOf(Keys, key);
            id = (byte)Math.Max(index, 0);
            return index >= 0;
        }

        public static string GetKey(byte id) => Keys[id];
    }
}
