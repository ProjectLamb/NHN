using System.Collections.Generic;
using UnityEngine;

namespace SandwichGame.Sandwich
{
    public class SandwichValidator : MonoBehaviour
    {
        [SerializeField] private SandwichManager sandwichManager;
        [SerializeField] private List<SandwichRecipeLayer> expectedRecipe=new List<SandwichRecipeLayer>();
        public void Configure(SandwichManager manager)=>sandwichManager=manager;
        public bool Validate(out string message)
        {
            if(sandwichManager==null){message="SandwichManager가 연결되지 않았습니다.";return false;}
            if(expectedRecipe==null||expectedRecipe.Count==0){message="정답 레시피가 아직 설정되지 않았습니다.";return false;}
            IReadOnlyList<SandwichLayerData> actual=sandwichManager.Layers;
            if(actual.Count!=expectedRecipe.Count){message=$"완성 실패: 레이어 수 {actual.Count}/{expectedRecipe.Count}";return false;}
            for(int i=0;i<actual.Count;i++)if(actual[i].ingredientType!=expectedRecipe[i].ingredientType||actual[i].stateId!=expectedRecipe[i].stateId){message=$"완성 실패: {i+1}번째 레이어가 다릅니다.";return false;}
            message="샌드위치 완성 성공!";return true;
        }
    }
}
