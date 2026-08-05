using SandwichGame.Sandwich;
namespace SandwichGame.Actions { public static class FinishActionHandler { public static bool Execute(SandwichValidator v,out string m){if(v==null){m="SandwichValidator가 없습니다.";return false;}return v.Validate(out m);} } }
