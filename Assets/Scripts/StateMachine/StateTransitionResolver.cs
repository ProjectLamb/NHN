namespace SandwichGame.StateMachine
{
    public sealed class StateTransitionResolver
    {
        private readonly StateTransitionDatabase database;
        public StateTransitionResolver(StateTransitionDatabase value) => database = value;

        public bool TryResolve(string sourceStateId, string action, string amount, out string resultStateId)
        {
            resultStateId = null;
            if (database == null || database.transitions == null) return false;
            string normalizedAmount = string.IsNullOrWhiteSpace(amount) ? null : amount;
            foreach (StateTransitionData transition in database.transitions)
            {
                if (transition != null && transition.sourceStateId == sourceStateId && transition.action == action &&
                    (string.IsNullOrWhiteSpace(transition.amount) ? null : transition.amount) == normalizedAmount)
                {
                    resultStateId = transition.resultStateId;
                    return !string.IsNullOrWhiteSpace(resultStateId);
                }
            }
            return false;
        }
    }
}
