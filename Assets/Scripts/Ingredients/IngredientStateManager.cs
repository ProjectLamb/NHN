using System.Collections.Generic;
using SandwichGame.AI;
using SandwichGame.StateMachine;
using UnityEngine;

namespace SandwichGame.Ingredients
{
    public class IngredientStateManager : MonoBehaviour
    {
        [SerializeField] private StateTransitionDatabase transitionDatabase;
        [SerializeField] private IngredientPrefabDatabase prefabDatabase;
        [SerializeField] private IngredientView[] ingredientViews;
        [SerializeField] private List<IngredientRuntimeState> states = new List<IngredientRuntimeState>();
        private StateTransitionResolver resolver;

        private void Awake() { resolver = new StateTransitionResolver(transitionDatabase); EnsureStates(); BindAuthoredSceneObjects(); }
        private void Start() { foreach (IngredientRuntimeState state in states) if (state.currentObject == null) SpawnCurrent(state); }

        public void Configure(StateTransitionDatabase transitions, IngredientPrefabDatabase prefabs, IngredientView[] views)
        { transitionDatabase=transitions; prefabDatabase=prefabs; ingredientViews=views; }

        public void SetInitialSceneObject(IngredientType type, string stateId, GameObject sceneObject)
        {
            EnsureStates();
            IngredientRuntimeState state = GetState(type);
            state.currentStateId = stateId;
            state.currentObject = sceneObject;
        }

        public bool TryApplyTransition(IngredientType type,string action,string amount,out string error)
        {
            error=null; IngredientRuntimeState state=GetState(type);
            if(state==null){error=$"{type} 상태가 없습니다.";return false;}
            if(resolver==null)resolver=new StateTransitionResolver(transitionDatabase);
            if(!resolver.TryResolve(state.currentStateId,action,amount,out string next)){error=$"전이 없음: {state.currentStateId} + {action} + {(string.IsNullOrEmpty(amount)?"null":amount)}";return false;}
            if(prefabDatabase==null||!prefabDatabase.TryGetPrefab(next,out GameObject prefab)){error=$"'{next}' 프리팹이 연결되지 않았습니다.";return false;}
            Transform slot=FindSlot(type);
            Vector3 position=state.currentObject!=null?state.currentObject.transform.position:slot.position;
            Quaternion rotation=state.currentObject!=null?state.currentObject.transform.rotation:slot.rotation;
            Vector3 scale=state.currentObject!=null?state.currentObject.transform.localScale:prefab.transform.localScale;
            if(state.currentObject!=null)Destroy(state.currentObject);
            state.currentObject=Instantiate(prefab,position,rotation,slot);state.currentObject.transform.localScale=scale;state.currentStateId=next;return true;
        }

        public IngredientRuntimeState GetState(IngredientType type)=>states.Find(x=>x.ingredientType==type);
        public bool TryGetCurrentPrefab(IngredientType type,out GameObject prefab,out string stateId)
        { IngredientRuntimeState s=GetState(type); stateId=s?.currentStateId; prefab=null; return s!=null&&prefabDatabase!=null&&prefabDatabase.TryGetPrefab(stateId,out prefab); }
        public IngredientStateSnapshot CreateSnapshot()=>new IngredientStateSnapshot{bread=Id(IngredientType.Bread),ham=Id(IngredientType.Ham),tomato=Id(IngredientType.Tomato),cheese=Id(IngredientType.Cheese),mayonnaise=Id(IngredientType.Mayonnaise),cabbage=Id(IngredientType.Cabbage)};
        private string Id(IngredientType t)=>GetState(t)?.currentStateId??string.Empty;
        private Transform FindSlot(IngredientType t){if(ingredientViews!=null)foreach(IngredientView v in ingredientViews)if(v!=null&&v.IngredientType==t)return v.Slot;return transform;}
        private void SpawnCurrent(IngredientRuntimeState s){if(prefabDatabase!=null&&prefabDatabase.TryGetPrefab(s.currentStateId,out GameObject p)){Transform slot=FindSlot(s.ingredientType);s.currentObject=Instantiate(p,slot.position,slot.rotation,slot);}}
        private void EnsureStates(){Add(IngredientType.Bread,"BREAD_BAG_CLOSED_LOAF");Add(IngredientType.Ham,"HAM_PACK_CLOSED_STACK");Add(IngredientType.Tomato,"TOMATO_SLICE_STACK");Add(IngredientType.Cheese,"CHEESE_PACK_CLOSED_STACK");Add(IngredientType.Mayonnaise,"MAYO_BOTTLE_CLOSED");Add(IngredientType.Cabbage,"CABBAGE_PIECE");}
        private void BindAuthoredSceneObjects()
        {
            BindIfPresent(IngredientType.Cabbage,"CABBAGE_PIECE","양배추");
            BindIfPresent(IngredientType.Tomato,"TOMATO_SLICE_STACK","토마토");
        }
        private void BindIfPresent(IngredientType type,string stateId,string objectName)
        {
            GameObject sceneObject=GameObject.Find(objectName);if(sceneObject==null)return;
            IngredientRuntimeState state=GetState(type);state.currentStateId=stateId;state.currentObject=sceneObject;
        }
        private void Add(IngredientType t,string id){if(GetState(t)==null)states.Add(new IngredientRuntimeState{ingredientType=t,currentStateId=id});}
    }
}
