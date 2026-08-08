using System;
using System.Collections.Generic;
using SandwichGame.Ingredients;
using UnityEngine;

namespace SandwichGame.Sandwich
{
    public class SandwichManager : MonoBehaviour
    {
        [SerializeField] private Transform layerRoot;
        [SerializeField] private float verticalGap;
        [SerializeField] private List<SandwichLayerData> layers = new List<SandwichLayerData>();

        private readonly Dictionary<IngredientType, GameObject> stagedIngredients = new Dictionary<IngredientType, GameObject>();
        private readonly Dictionary<IngredientType, string> stagedStateIds = new Dictionary<IngredientType, string>();
        private Vector3 stackPoint;
        private bool stackInitialized;

        public IReadOnlyList<SandwichLayerData> Layers => layers;
        public void Configure(Transform root) => layerRoot = root;

        public bool ShowPreparedIngredient(IngredientType type, string stateId, GameObject prefab, out string error)
        {
            error = null;
            if (prefab == null) { error = $"{stateId} 프리팹이 없습니다."; return false; }
            if (!stackInitialized && !ResetStackPoint(out error)) return false;
            if (stagedIngredients.TryGetValue(type, out GameObject oldInstance) && oldInstance != null) Destroy(oldInstance);

            GameObject instance = CreateAtStackPoint(type, prefab);
            if (instance == null) { error = $"{prefab.name}의 Renderer 또는 Collider를 찾지 못했습니다."; return false; }
            stagedIngredients[type] = instance;
            stagedStateIds[type] = stateId;
            return true;
        }

        public bool TryAddLayer(IngredientType type, string stateId, GameObject prefab, out string error)
        {
            error = null;
            if (prefab == null) { error = $"{stateId} 프리팹이 없습니다."; return false; }
            if (!stackInitialized && !ResetStackPoint(out error)) return false;

            GameObject instance;
            if (stagedIngredients.TryGetValue(type, out GameObject staged) && staged != null &&
                stagedStateIds.TryGetValue(type, out string stagedId) && stagedId == stateId)
            {
                instance = staged;
                stagedIngredients.Remove(type);
                stagedStateIds.Remove(type);
            }
            else instance = CreateAtStackPoint(type, prefab);

            if (instance == null || !TryGetBounds(instance, out Bounds bounds))
            { if (instance != null) Destroy(instance); error = $"{prefab.name}의 Renderer 또는 Collider를 찾지 못했습니다."; return false; }

            stackPoint = new Vector3(bounds.center.x, bounds.max.y + verticalGap, bounds.center.z);
            layers.Add(new SandwichLayerData { ingredientType = type, stateId = stateId, instance = instance });
            return true;
        }

        private GameObject CreateAtStackPoint(IngredientType type, GameObject prefab)
        {
            Transform parent = layerRoot != null ? layerRoot : transform;
            GameObject instance = Instantiate(prefab, stackPoint, prefab.transform.rotation, parent);
            instance.name = prefab.name;
            if (TryGetBounds(instance, out Bounds bounds))
            {
                Vector3 offset;
                if (IsCenterBased(prefab.name, type))
                    offset = new Vector3(stackPoint.x - bounds.center.x, stackPoint.y - bounds.min.y, stackPoint.z - bounds.center.z);
                else
                    offset = new Vector3(0f, stackPoint.y - bounds.min.y, 0f);
                instance.transform.position += offset;
                return instance;
            }
            Destroy(instance);
            return null;
        }

        private bool ResetStackPoint(out string error)
        {
            error = null;
            Transform plate = FindPlateOnCoffeeTable();
            if (plate == null) { error = "Wooden Coffee Table 안의 Plate를 찾지 못했습니다."; return false; }
            stackPoint = TryGetBounds(plate.gameObject, out Bounds bounds)
                ? new Vector3(bounds.center.x, bounds.max.y, bounds.center.z) : plate.position;
            stackInitialized = true;
            return true;
        }

        private static bool IsCenterBased(string prefabName, IngredientType type)
            => prefabName == "Bread_Bag_Slice" || type == IngredientType.Cheese || type == IngredientType.Ham ||
               type == IngredientType.Mayonnaise || type == IngredientType.Tomato;

        private static bool TryGetBounds(GameObject target, out Bounds combined)
        {
            combined = new Bounds(); bool found = false;
            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>(true))
            { if (!renderer.enabled || !renderer.gameObject.activeInHierarchy) continue; if (!found) { combined = renderer.bounds; found = true; } else combined.Encapsulate(renderer.bounds); }
            if (found) return true;
            foreach (Collider collider in target.GetComponentsInChildren<Collider>(true))
            { if (!collider.enabled || !collider.gameObject.activeInHierarchy) continue; if (!found) { combined = collider.bounds; found = true; } else combined.Encapsulate(collider.bounds); }
            return found;
        }

        private static Transform FindPlateOnCoffeeTable()
        {
            foreach (Transform candidate in FindObjectsOfType<Transform>(true))
            {
                if (!string.Equals(candidate.name, "Plate", StringComparison.OrdinalIgnoreCase)) continue;
                for (Transform parent = candidate.parent; parent != null; parent = parent.parent)
                    if (string.Equals(parent.name, "Wooden Coffee Table", StringComparison.OrdinalIgnoreCase)) return candidate;
            }
            return null;
        }
    }
}
