using System.Collections.Generic;
using UnityEngine;

namespace SandwichGame.StateMachine
{
    [CreateAssetMenu(menuName = "Sandwich/State Transition Database")]
    public class StateTransitionDatabase : ScriptableObject
    {
        public List<StateTransitionData> transitions = new List<StateTransitionData>();
    }
}
