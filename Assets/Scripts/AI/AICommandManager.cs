using System;using System.Collections;using System.Text;using SandwichGame.Actions;using SandwichGame.Ingredients;using UnityEngine;using UnityEngine.Networking;
namespace SandwichGame.AI
{
    public class AICommandManager:MonoBehaviour
    {
        [SerializeField]private string functionUrl="https://asia-northeast3-YOUR_PROJECT.cloudfunctions.net/interpretSandwichCommand";[SerializeField]private bool useMockResponse=true;[SerializeField]private int requestTimeoutSeconds=15;[SerializeField]private IngredientStateManager stateManager;[SerializeField]private ActionExecutor executor;
        public event Action<bool,string> RequestStateChanged;public bool IsBusy{get;private set;}
        public void Configure(IngredientStateManager s,ActionExecutor e){stateManager=s;executor=e;}
        public void Interpret(string command){if(IsBusy)return;command=command?.Trim();if(string.IsNullOrEmpty(command)||command.Length>200){RequestStateChanged?.Invoke(false,"명령은 1~200자로 입력하세요.");return;}StartCoroutine(Run(command));}
        private IEnumerator Run(string command){IsBusy=true;RequestStateChanged?.Invoke(true,"명령 해석 중...");if(useMockResponse){yield return null;Handle(BuildMockResponse(command));Done();yield break;}if(stateManager==null){Fail("상태 관리자가 없습니다.");yield break;}byte[] body=Encoding.UTF8.GetBytes(JsonUtility.ToJson(new AICommandRequest{command=command,currentStates=stateManager.CreateSnapshot()}));using(UnityWebRequest r=new UnityWebRequest(functionUrl,UnityWebRequest.kHttpVerbPOST)){r.uploadHandler=new UploadHandlerRaw(body);r.downloadHandler=new DownloadHandlerBuffer();r.SetRequestHeader("Content-Type","application/json");r.timeout=requestTimeoutSeconds;yield return r.SendWebRequest();if(r.result!=UnityWebRequest.Result.Success){Fail(r.responseCode==0?"서버 연결 또는 시간 초과 오류":$"서버 오류 HTTP {r.responseCode}");yield break;}Handle(r.downloadHandler.text);}Done();}
        private void Handle(string json){try{SandwichActionResponse r=JsonUtility.FromJson<SandwichActionResponse>(json);if(r?.actions==null)throw new FormatException();executor.Execute(r.actions);}catch(Exception e){Debug.LogError(e);RequestStateChanged?.Invoke(false,"응답 JSON 형식 오류");}}
        private void Done(){IsBusy=false;RequestStateChanged?.Invoke(false,"명령 처리 완료");}private void Fail(string m){IsBusy=false;RequestStateChanged?.Invoke(false,m);}
        private static string BuildMockResponse(string command)
        {
            if((command.Contains("양배추")||command.ToLowerInvariant().Contains("cabbage"))&&(command.Contains("자르")||command.Contains("잘라")||command.Contains("썰")))
            {
                string amount=(command.Contains("작게")||command.Contains("조금")||command.Contains("얇게"))?"S":(command.Contains("크게")||command.Contains("많이")||command.Contains("두껍게"))?"L":"M";
                return "{\"actions\":[{\"action\":\"Cut\",\"targetIngredient\":\"cabbage\",\"amount\":\""+amount+"\",\"rawCommandPart\":\""+EscapeJson(command)+"\"}]}";
            }
            return "{\"actions\":[{\"action\":\"Open\",\"targetIngredient\":\"bread\",\"amount\":null,\"rawCommandPart\":\"빵 봉지를 열고\"},{\"action\":\"TakeOff\",\"targetIngredient\":\"bread\",\"amount\":\"M\",\"rawCommandPart\":\"빵을 꺼내서\"},{\"action\":\"Cut\",\"targetIngredient\":\"bread\",\"amount\":\"S\",\"rawCommandPart\":\"얇게 잘라\"}]}";
        }
        private static string EscapeJson(string value)=>value.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r"," ").Replace("\n"," ");
    }
}
