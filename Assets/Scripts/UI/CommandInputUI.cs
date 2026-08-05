using SandwichGame.Actions;using SandwichGame.AI;using TMPro;using UnityEngine;using UnityEngine.UI;
namespace SandwichGame.UI
{
    public class CommandInputUI:MonoBehaviour
    {
        [SerializeField]private TMP_InputField input;[SerializeField]private TMP_Text statusText;[SerializeField]private Button executeButton;[SerializeField]private AICommandManager manager;[SerializeField]private ActionExecutor executor;
        public void Configure(TMP_InputField i,TMP_Text s,Button b,AICommandManager m,ActionExecutor e){input=i;statusText=s;executeButton=b;manager=m;executor=e;}
        private void OnEnable(){if(executor!=null)executor.StatusChanged+=SetStatus;if(manager!=null)manager.RequestStateChanged+=OnRequest;}
        private void OnDisable(){if(executor!=null)executor.StatusChanged-=SetStatus;if(manager!=null)manager.RequestStateChanged-=OnRequest;}
        public void Submit(){if(manager!=null&&input!=null)manager.Interpret(input.text);}
        private void SetStatus(string s){if(statusText!=null)statusText.text=s;}private void OnRequest(bool busy,string s){if(executeButton!=null)executeButton.interactable=!busy;SetStatus(s);}
    }
}
