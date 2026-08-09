using System.Collections.Generic;
namespace SandwichGame.StateMachine
{
    public static class DefaultStateTransitions
    {
        public static List<StateTransitionData> Build()
        {
            var t=new List<StateTransitionData>();
            Add(t,"BREAD_BAG_CLOSED_LOAF","Open",null,"BREAD_LOAF");
            Sizes(t,"BREAD_BAG_CLOSED_LOAF","Cut","_CUT_BREAD_BAG_CLOSED_LOAF");
            Sizes(t,"BREAD_LOAF","Cut","_CUT_BREAD_LOAF");
            Sizes(t,"CABBAGE_PIECE","Cut","_CUT_CABBAGE_PIECE"); Sizes(t,"TOMATO_SLICE_STACK","TakeOff","_TAKEOFF_TOMATO_SLICE"); Sizes(t,"HAM_PACK_CLOSED_STACK","TakeOff","_TAKEOFF_HAM_SLICE");
            Add(t,"CHEESE_PACK_CLOSED_STACK","Open",null,"CHEESE_PACK_OPENED_STACK"); Sizes(t,"CHEESE_PACK_OPENED_STACK","TakeOff","_TAKEOFF_CHEESE_SLICE_WRAPPED");
            Add(t,"S_TAKEOFF_CHEESE_SLICE_WRAPPED","Open",null,"S_OPENED_CHEESE_SLICE"); Add(t,"M_TAKEOFF_CHEESE_SLICE_WRAPPED","Open",null,"M_OPENED_CHEESE_SLICE"); Add(t,"L_TAKEOFF_CHEESE_SLICE_WRAPPED","Open",null,"L_OPENED_CHEESE_SLICE");
            Add(t,"MAYO_BOTTLE_CLOSED","Open",null,"MAYO_BOTTLE_OPENED");
            Sizes(t,"MAYO_BOTTLE_OPENED","TakeOff","_TAKEOFF_MAYO"); return t;
        }
        private static void Sizes(List<StateTransitionData> l,string s,string a,string suffix){foreach(string amount in Amounts)Add(l,s,a,amount,amount+suffix);}
        private static void Add(List<StateTransitionData> l,string s,string a,string m,string r)=>l.Add(new StateTransitionData(s,a,m,r));
        private static readonly string[] Amounts={"L","M","S"};
    }
}
