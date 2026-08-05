using SandwichGame.Ingredients;
namespace SandwichGame.Actions { public static class CutActionHandler { public static bool Execute(IngredientStateManager m,IngredientType t,string a,out string e)=>m.TryApplyTransition(t,"Cut",a,out e); } }
