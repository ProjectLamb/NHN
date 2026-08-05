using System;

namespace SandwichGame.AI
{
    [Serializable]
    public class SandwichActionResponse { public SandwichActionData[] actions; }

    [Serializable]
    public class AIErrorResponse { public string error; }
}
