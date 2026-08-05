using System;
using SandwichGame.Ingredients;
using UnityEngine;

namespace SandwichGame.Sandwich
{
    [Serializable] public class SandwichLayerData { public IngredientType ingredientType; public string stateId; public GameObject instance; }
    [Serializable] public class SandwichRecipeLayer { public IngredientType ingredientType; public string stateId; }
}
