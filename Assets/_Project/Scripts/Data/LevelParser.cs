using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NutSort.Core;

namespace NutSort.Data
{
    // bolts are bottom to top
    // { "id": 1, "capacity": 3, "minMoves": 4, "bolts": [["red", "blue", "red"], ["blue", "red", "blue"], []] }
    public static class LevelParser
    {
        public static LevelData Parse(string json)
        {
            var dto = JsonConvert.DeserializeObject<LevelDto>(json);
            if (dto == null)
                throw new Exception("Level JSON is empty");

            string name = $"Level {dto.Id}";
            if (dto.Capacity <= 0)
                throw new Exception($"{name}: capacity must be positive");
            if (dto.Bolts == null || dto.Bolts.Length < 2)
                throw new Exception($"{name}: needs at least two bolts");

            var bolts = new byte[dto.Bolts.Length][];
            var colorCounts = new Dictionary<byte, int>();

            for (int i = 0; i < dto.Bolts.Length; i++)
            {
                var keys = dto.Bolts[i] ?? new string[0];
                if (keys.Length > dto.Capacity)
                    throw new Exception($"{name}: bolt {i} has {keys.Length} nuts but capacity is {dto.Capacity}");

                var nuts = new byte[keys.Length];
                for (int j = 0; j < keys.Length; j++)
                {
                    if (!NutColors.TryGetId(keys[j], out var id))
                        throw new Exception($"{name}: unknown color '{keys[j]}' on bolt {i}");

                    nuts[j] = id;
                    colorCounts.TryGetValue(id, out int count);
                    colorCounts[id] = count + 1;
                }
                bolts[i] = nuts;
            }

            // each color must fill exactly one bolt
            foreach (var pair in colorCounts)
            {
                if (pair.Value != dto.Capacity)
                    throw new Exception($"{name}: color '{NutColors.GetKey(pair.Key)}' appears {pair.Value} times, expected {dto.Capacity}");
            }

            return new LevelData(dto.Id, dto.MinMoves, new BoardState(dto.Capacity, bolts));
        }

        private class LevelDto
        {
            [JsonProperty("id", Required = Required.Always)] public int Id { get; set; }
            [JsonProperty("capacity", Required = Required.Always)] public int Capacity { get; set; }
            [JsonProperty("minMoves")] public int MinMoves { get; set; }
            [JsonProperty("bolts", Required = Required.Always)] public string[][] Bolts { get; set; }
        }
    }
}
