using System.Collections.Generic;
using SandwichGame.Ingredients;
using UnityEngine;

namespace SandwichGame.Sandwich
{
    public class SandwichManager : MonoBehaviour
    {
        [SerializeField] private Transform layerRoot;
        [SerializeField] private Vector3 layerOffset=new Vector3(0,0.12f,0);
        [SerializeField] private List<SandwichLayerData> layers=new List<SandwichLayerData>();
        public IReadOnlyList<SandwichLayerData> Layers=>layers;
        public void Configure(Transform root)=>layerRoot=root;
        public bool TryAddLayer(IngredientType type,string stateId,GameObject prefab,out string error)
        {error=null;if(prefab==null){error=$"{stateId} 프리팹이 없습니다.";return false;}Transform root=layerRoot!=null?layerRoot:transform;GameObject o=Instantiate(prefab,root);o.transform.localPosition=layerOffset*layers.Count;o.transform.localRotation=Quaternion.identity;layers.Add(new SandwichLayerData{ingredientType=type,stateId=stateId,instance=o});return true;}
    }
}
