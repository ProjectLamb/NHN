using SandwichGame.Ingredients;
using SandwichGame.Sandwich;
namespace SandwichGame.Actions
{
    public static class PutActionHandler
    {
        public static bool Execute(IngredientStateManager stateManager,SandwichManager sandwichManager,IngredientType type,out string error)
        {
            if(!stateManager.IsReadyToPut(type,out error))return false;
            if(!stateManager.TryGetCurrentPrefab(type,out UnityEngine.GameObject prefab,out string stateId)){error=$"{type} 현재 프리팹이 없습니다.";return false;}
            if(!sandwichManager.TryAddLayer(type,stateId,prefab,out error))return false;
            stateManager.ResetToInitialState(type);
            return true;
        }
    }
}
