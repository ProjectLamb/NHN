using SandwichGame.Ingredients; using SandwichGame.Sandwich;
namespace SandwichGame.Actions { public static class PutActionHandler { public static bool Execute(IngredientStateManager sm,SandwichManager m,IngredientType t,out string e){if(!sm.TryGetCurrentPrefab(t,out UnityEngine.GameObject p,out string id)){e=$"{t} 현재 프리팹이 없습니다.";return false;}return m.TryAddLayer(t,id,p,out e);} } }
