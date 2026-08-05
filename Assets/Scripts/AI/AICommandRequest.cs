using System;

namespace SandwichGame.AI
{
    [Serializable]
    public class IngredientStateSnapshot
    {
        public string bread, ham, tomato, cheese, mayonnaise, cabbage;
    }

    [Serializable]
    public class AICommandRequest
    {
        public string command;
        public IngredientStateSnapshot currentStates;
    }
}
