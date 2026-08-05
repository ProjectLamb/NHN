using System;

namespace SandwichGame.StateMachine
{
    [Serializable]
    public class StateTransitionData
    {
        public string sourceStateId;
        public string action;
        public string amount;
        public string resultStateId;

        public StateTransitionData(string source, string actionName, string actionAmount, string result)
        {
            sourceStateId = source;
            action = actionName;
            amount = actionAmount;
            resultStateId = result;
        }
    }
}
