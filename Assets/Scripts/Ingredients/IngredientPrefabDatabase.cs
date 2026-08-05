using System;
using System.Collections.Generic;
using UnityEngine;

namespace SandwichGame.Ingredients
{
    [Serializable]
    public class IngredientPrefabEntry { public string stateId; public GameObject prefab; }

    [CreateAssetMenu(menuName = "Sandwich/Ingredient Prefab Database")]
    public class IngredientPrefabDatabase : ScriptableObject
    {
        [HideInInspector] public int setupVersion;
        public List<IngredientPrefabEntry> entries = new List<IngredientPrefabEntry>();
        public bool TryGetPrefab(string stateId, out GameObject prefab)
        {
            prefab = null;
            if (entries == null) return false;
            foreach (IngredientPrefabEntry entry in entries)
                if (entry != null && entry.stateId == stateId) { prefab = entry.prefab; return prefab != null; }
            return false;
        }
    }
}
