using System;
using UnityEngine;

namespace SandwichGame.Ingredients
{
    [Serializable]
    public class IngredientRuntimeState
    {
        public IngredientType ingredientType;
        public string currentStateId;
        public GameObject currentObject;
    }
}
