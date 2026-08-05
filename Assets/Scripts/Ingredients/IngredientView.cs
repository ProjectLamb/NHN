using UnityEngine;

namespace SandwichGame.Ingredients
{
    public class IngredientView : MonoBehaviour
    {
        [SerializeField] private IngredientType ingredientType;
        public IngredientType IngredientType => ingredientType;
        public Transform Slot => transform;
        public void Configure(IngredientType type) => ingredientType = type;
    }
}
