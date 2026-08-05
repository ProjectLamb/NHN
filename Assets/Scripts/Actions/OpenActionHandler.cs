using SandwichGame.Ingredients;
namespace SandwichGame.Actions { public static class OpenActionHandler { public static bool Execute(IngredientStateManager m,IngredientType t,out string e)=>m.TryApplyTransition(t,"Open",null,out e); } }
