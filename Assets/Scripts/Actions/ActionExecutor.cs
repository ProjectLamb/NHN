using System; using System.Collections.Generic; using SandwichGame.AI; using SandwichGame.Ingredients; using SandwichGame.Sandwich; using UnityEngine;
namespace SandwichGame.Actions
{
    public class ActionExecutor:MonoBehaviour
    {
        [SerializeField]private IngredientStateManager stateManager;[SerializeField]private SandwichManager sandwichManager;[SerializeField]private SandwichValidator validator;[SerializeField]private bool continueAfterFailedAction=true;
        public event Action<string> StatusChanged;
        public void Configure(IngredientStateManager s,SandwichManager m,SandwichValidator v){stateManager=s;sandwichManager=m;validator=v;}
        public void Execute(SandwichActionData[] actions){if(actions==null){Report("실행할 행동이 없습니다.",true);return;}foreach(SandwichActionData a in actions){bool ok=TryExecute(a,out string message);Report(message,!ok);if(!ok&&!continueAfterFailedAction)break;}}
        private bool TryExecute(SandwichActionData a,out string message)
        {message="잘못된 action입니다.";if(a==null||!Allowed.Contains(a.action))return false;if(a.action=="Finish")return FinishActionHandler.Execute(validator,out message);if(!Enum.TryParse(a.targetIngredient,true,out IngredientType t)){message=$"잘못된 재료: {a.targetIngredient}";return false;}bool ok;switch(a.action){case"Open":ok=OpenActionHandler.Execute(stateManager,t,out message);break;case"TakeOff":ok=TakeOffActionHandler.Execute(stateManager,t,a.amount,out message);break;case"Cut":ok=CutActionHandler.Execute(stateManager,t,a.amount,out message);break;case"Put":ok=PutActionHandler.Execute(stateManager,sandwichManager,t,out message);break;default:return false;}if(ok)message=$"{a.action} {a.targetIngredient} 완료";return ok;}
        private void Report(string m,bool error){if(error)Debug.LogError(m);else Debug.Log(m);StatusChanged?.Invoke(m);}
        private static readonly HashSet<string> Allowed=new HashSet<string>{"Open","TakeOff","Cut","Put","Finish"};
    }
}
