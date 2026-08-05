using SandwichGame.Ingredients;
namespace SandwichGame.Actions { public static class TakeOffActionHandler { public static bool Execute(IngredientStateManager m,IngredientType t,string a,out string e)=>m.TryApplyTransition(t,"TakeOff",a,out e); } }
