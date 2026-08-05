using System;

namespace SandwichGame.AI
{
    [Serializable]
    public class SandwichActionData
    {
        public string action;
        public string targetIngredient;
        public string amount;
        public string rawCommandPart;
    }
}
