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

        private Vector3 stackPoint;
        private bool stackInitialized;

        public IReadOnlyList<SandwichLayerData> Layers => layers;
        public void Configure(Transform root) => layerRoot = root;

        public void ClearAll()
        {
            foreach (SandwichLayerData layer in layers)
                if (layer != null && layer.instance != null)
                    Destroy(layer.instance);

            layers.Clear();
            stackInitialized = false;
            stackPoint = Vector3.zero;
        }

        public bool TryAddLayer(IngredientType type, string stateId, GameObject prefab, out string error)
        {
            error = null;
            if (prefab == null) { error = $"{stateId} 프리팹이 없습니다."; return false; }
            if (!stackInitialized && !ResetStackPoint(out error)) return false;

            GameObject instance = CreateAtStackPoint(prefab);

            if (instance == null || !TryGetBounds(instance, out Bounds bounds))
            { if (instance != null) Destroy(instance); error = $"{prefab.name}의 Renderer 또는 Collider를 찾지 못했습니다."; return false; }

            // Match FoodMakingTest: after placement, use the actual top-center of
            // the combined renderer bounds as the next layer's bottom position.
            stackPoint = new Vector3(
                bounds.center.x,
                bounds.max.y + verticalGap,
                bounds.center.z);
            layers.Add(new SandwichLayerData { ingredientType = type, stateId = stateId, instance = instance });
            return true;
        }

        private GameObject CreateAtStackPoint(GameObject prefab)
        {
            Transform parent = layerRoot != null ? layerRoot : transform;
            // Match FoodMakingTest: instantiate first, then align the food's actual
            // bottom-center to the current stack point using its combined bounds.
            GameObject instance = Instantiate(prefab, Vector3.zero, prefab.transform.rotation, parent);
            instance.name = prefab.name;
            if (PositionAtStackPoint(instance))
            {
                return instance;
            }
            Destroy(instance);
            return null;
        }

        private bool PositionAtStackPoint(GameObject instance)
        {
            if (!TryGetBounds(instance, out Bounds bounds))
                return false;

            Vector3 bottomCenter = new Vector3(
                bounds.center.x,
                bounds.min.y,
                bounds.center.z);

            instance.transform.position += stackPoint - bottomCenter;
            return true;
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

        private static bool TryGetBounds(GameObject target, out Bounds combined)
        {
            combined = new Bounds(); bool found = false;
            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy || renderer is ParticleSystemRenderer) continue;
                if (!found) { combined = renderer.bounds; found = true; } else combined.Encapsulate(renderer.bounds);
            }
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
