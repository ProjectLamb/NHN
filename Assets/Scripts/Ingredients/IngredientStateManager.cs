using System.Collections.Generic;
using SandwichGame.AI;
using SandwichGame.StateMachine;
using UnityEngine;
namespace SandwichGame.Ingredients
{
    public class IngredientStateManager:MonoBehaviour
    {
        [SerializeField]private StateTransitionDatabase transitionDatabase;[SerializeField]private IngredientPrefabDatabase prefabDatabase;[SerializeField]private IngredientView[] ingredientViews;[SerializeField]private List<IngredientRuntimeState> states=new List<IngredientRuntimeState>();private StateTransitionResolver resolver;
        private void Awake(){resolver=new StateTransitionResolver(transitionDatabase);EnsureStates();}
        public void Configure(StateTransitionDatabase transitions,IngredientPrefabDatabase prefabs,IngredientView[] views){transitionDatabase=transitions;prefabDatabase=prefabs;ingredientViews=views;}
        public void SetInitialSceneObject(IngredientType type,string stateId,GameObject sceneObject){EnsureStates();IngredientRuntimeState s=GetState(type);s.currentStateId=stateId;s.currentObject=sceneObject;}
        public bool TryApplyTransition(IngredientType type,string action,string amount,out string error)
        {
            error=null;IngredientRuntimeState state=GetState(type);if(state==null){error=$"{type} 상태가 없습니다.";return false;}if(resolver==null)resolver=new StateTransitionResolver(transitionDatabase);
            if(!resolver.TryResolve(state.currentStateId,action,amount,out string next)){error=$"사용할 수 없는 동작입니다: {state.currentStateId} + {action}";return false;}
            if(prefabDatabase==null||!prefabDatabase.TryGetPrefab(next,out GameObject prefab)){error=$"'{next}' 프리팹이 연결되지 않았습니다.";return false;}
            // 상태 전환은 데이터만 갱신한다. 실제 음식 프리팹은 Put 동작에서
            // SandwichManager가 Plate 위에 한 번만 생성한다.
            if(state.currentObject!=null)Destroy(state.currentObject);
            state.currentObject=null;state.currentStateId=next;return true;
        }
        public IngredientRuntimeState GetState(IngredientType type)=>states.Find(x=>x.ingredientType==type);
        public bool TryGetCurrentPrefab(IngredientType type,out GameObject prefab,out string stateId){IngredientRuntimeState s=GetState(type);stateId=s?.currentStateId;prefab=null;return s!=null&&prefabDatabase!=null&&prefabDatabase.TryGetPrefab(stateId,out prefab);}
        public bool IsReadyToPut(IngredientType type,out string error){string id=GetState(type)?.currentStateId??string.Empty;bool ready=id.Contains("_CUT_BREAD_LOAF")||id.Contains("_CUT_CABBAGE_PIECE")||id.Contains("_TAKEOFF_TOMATO_SLICE")||id.Contains("_TAKEOFF_HAM_SLICE")||id.Contains("_OPENED_CHEESE_SLICE")||id.Contains("_TAKEOFF_MAYO");error=ready?null:$"{type} 재료 준비가 아직 끝나지 않았습니다.";return ready;}
        public void ResetToInitialState(IngredientType type){IngredientRuntimeState state=GetState(type);if(state==null)return;state.currentStateId=InitialStateId(type);state.currentObject=null;}
        public IngredientStateSnapshot CreateSnapshot()=>new IngredientStateSnapshot{bread=Id(IngredientType.Bread),ham=Id(IngredientType.Ham),tomato=Id(IngredientType.Tomato),cheese=Id(IngredientType.Cheese),mayonnaise=Id(IngredientType.Mayonnaise),cabbage=Id(IngredientType.Cabbage)};
        private string Id(IngredientType t)=>GetState(t)?.currentStateId??string.Empty;private Transform FindSlot(IngredientType t){if(ingredientViews!=null)foreach(IngredientView v in ingredientViews)if(v!=null&&v.IngredientType==t)return v.Slot;return transform;}
        private void EnsureStates(){Add(IngredientType.Bread,"BREAD_BAG_CLOSED_LOAF");Add(IngredientType.Ham,"HAM_PACK_CLOSED_STACK");Add(IngredientType.Tomato,"TOMATO_SLICE_STACK");Add(IngredientType.Cheese,"CHEESE_PACK_CLOSED_STACK");Add(IngredientType.Mayonnaise,"MAYO_BOTTLE_CLOSED");Add(IngredientType.Cabbage,"CABBAGE_PIECE");}
        private static string InitialStateId(IngredientType type){switch(type){case IngredientType.Bread:return "BREAD_BAG_CLOSED_LOAF";case IngredientType.Ham:return "HAM_PACK_CLOSED_STACK";case IngredientType.Tomato:return "TOMATO_SLICE_STACK";case IngredientType.Cheese:return "CHEESE_PACK_CLOSED_STACK";case IngredientType.Mayonnaise:return "MAYO_BOTTLE_CLOSED";case IngredientType.Cabbage:return "CABBAGE_PIECE";default:return string.Empty;}}
        private void Add(IngredientType t,string id){if(GetState(t)==null)states.Add(new IngredientRuntimeState{ingredientType=t,currentStateId=id});}
    }
}
