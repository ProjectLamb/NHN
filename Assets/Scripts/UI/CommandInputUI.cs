using System.Collections.Generic;
using System.Text;
using SandwichGame.Actions;
using SandwichGame.AI;
using SandwichGame.Ingredients;
using SandwichGame.Sandwich;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SandwichGame.UI
{
    public class CommandInputUI:MonoBehaviour
    {
        private const float RoundSeconds=50f;
        [SerializeField]private TMP_InputField input;[SerializeField]private TMP_Text statusText;[SerializeField]private TMP_Text menuText;[SerializeField]private Button executeButton;[SerializeField]private AICommandManager manager;[SerializeField]private ActionExecutor executor;
        private readonly List<IngredientType> missionRecipe=new List<IngredientType>();
        private SandwichManager sandwichManager;private TMP_Text timerText;private TMP_Text scoreText;private GameObject gameEndPanel;private GameObject loadingCanvas;private float remainingTime;private bool timerStarted;private bool roundEnded;

        public void Configure(TMP_InputField i,TMP_Text s,Button b,AICommandManager m,ActionExecutor e){input=i;statusText=s;executeButton=b;manager=m;executor=e;}
        private void OnEnable(){if(executor!=null)executor.StatusChanged+=SetStatus;if(manager!=null)manager.RequestStateChanged+=OnRequest;}
        private void OnDisable(){if(executor!=null)executor.StatusChanged-=SetStatus;if(manager!=null)manager.RequestStateChanged-=OnRequest;}
        private void Start(){sandwichManager=FindObjectOfType<SandwichManager>();loadingCanvas=GameObject.Find("CanvasLoading");ResolveRoundUI();ShowRandomMission();remainingTime=RoundSeconds;UpdateTimer();}
        private void Update(){if(roundEnded)return;if(!timerStarted){if(loadingCanvas!=null&&loadingCanvas.activeInHierarchy)return;timerStarted=true;}remainingTime=Mathf.Max(0f,remainingTime-Time.deltaTime);UpdateTimer();if(remainingTime<=0f)EndRound();}
        public void Submit(){if(!roundEnded&&manager!=null&&input!=null)manager.Interpret(input.text);}
        public void RestartGame()=>SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        public void ExitToMainMenu()=>SceneManager.LoadScene("GameStartMenu");
        private void SetStatus(string s){if(statusText!=null)statusText.text=s;}
        private void OnRequest(bool busy,string s){if(executeButton!=null)executeButton.interactable=!busy&&!roundEnded;SetStatus(s);}

        private void ShowRandomMission()
        {
            TMP_Text target=ResolveMenuText();if(target==null)return;
            List<IngredientType> fillings=new List<IngredientType>{IngredientType.Ham,IngredientType.Tomato,IngredientType.Cheese,IngredientType.Mayonnaise,IngredientType.Cabbage};
            for(int i=fillings.Count-1;i>0;i--){int j=Random.Range(0,i+1);IngredientType value=fillings[i];fillings[i]=fillings[j];fillings[j]=value;}
            int fillingCount=Random.Range(2,fillings.Count+1);missionRecipe.Clear();missionRecipe.Add(IngredientType.Bread);for(int i=0;i<fillingCount;i++)missionRecipe.Add(fillings[i]);missionRecipe.Add(IngredientType.Bread);
            StringBuilder mission=new StringBuilder();for(int i=0;i<missionRecipe.Count;i++){if(i>0)mission.Append('\n');mission.Append(KoreanName(missionRecipe[i]));}target.text=mission.ToString();
        }
        private void ResolveRoundUI()
        {
            Canvas canvas=input!=null?input.GetComponentInParent<Canvas>():null;if(canvas==null)return;
            foreach(Transform child in canvas.GetComponentsInChildren<Transform>(true))
            {
                if(child.name=="Timer")timerText=child.GetComponent<TMP_Text>();
                else if(child.name=="GameEndPanel")gameEndPanel=child.gameObject;
                else if(child.name=="Score")scoreText=child.GetComponent<TMP_Text>();
                else if(child.name=="다시하기"){Button button=child.GetComponent<Button>();if(button!=null)button.onClick.AddListener(RestartGame);}
                else if(child.name=="게임종료"){Button button=child.GetComponent<Button>();if(button!=null)button.onClick.AddListener(ExitToMainMenu);}
            }
            if(gameEndPanel!=null)gameEndPanel.SetActive(false);
        }
        private TMP_Text ResolveMenuText()
        {
            if(menuText!=null)return menuText;Canvas canvas=input!=null?input.GetComponentInParent<Canvas>():null;
            if(canvas!=null)foreach(Transform child in canvas.GetComponentsInChildren<Transform>(true))if(child.name=="Menu"){menuText=child.GetComponentInChildren<TMP_Text>(true);break;}
            return menuText!=null?menuText:statusText;
        }
        private void UpdateTimer(){if(timerText!=null)timerText.text=$"남은 시간 : {Mathf.CeilToInt(remainingTime)}";}
        private void EndRound()
        {
            roundEnded=true;if(input!=null)input.interactable=false;if(executeButton!=null)executeButton.interactable=false;
            int score=CalculateScore(out int composition,out int order,out int structure);if(scoreText!=null)scoreText.text=$"당신의 점수 : {score}점\n재료 {composition}/50 · 순서 {order}/40 · 빵 구조 {structure}/10";if(gameEndPanel!=null)gameEndPanel.SetActive(true);
        }
        private int CalculateScore(out int composition,out int order,out int structure)
        {
            List<IngredientType> actual=new List<IngredientType>();if(sandwichManager!=null)foreach(SandwichLayerData layer in sandwichManager.Layers)actual.Add(layer.ingredientType);
            int denominator=Mathf.Max(missionRecipe.Count,actual.Count);int matched=0;foreach(IngredientType type in System.Enum.GetValues(typeof(IngredientType))){int expectedCount=missionRecipe.FindAll(x=>x==type).Count;int actualCount=actual.FindAll(x=>x==type).Count;matched+=Mathf.Min(expectedCount,actualCount);}
            composition=denominator==0?0:Mathf.RoundToInt(50f*matched/denominator);order=denominator==0?0:Mathf.RoundToInt(40f*LongestCommonSubsequence(missionRecipe,actual)/denominator);structure=actual.Count>=2&&actual[0]==IngredientType.Bread&&actual[actual.Count-1]==IngredientType.Bread?10:0;return composition+order+structure;
        }
        private static int LongestCommonSubsequence(List<IngredientType> expected,List<IngredientType> actual)
        {
            int[,] lengths=new int[expected.Count+1,actual.Count+1];for(int i=1;i<=expected.Count;i++)for(int j=1;j<=actual.Count;j++)lengths[i,j]=expected[i-1]==actual[j-1]?lengths[i-1,j-1]+1:Mathf.Max(lengths[i-1,j],lengths[i,j-1]);return lengths[expected.Count,actual.Count];
        }
        private static string KoreanName(IngredientType type){switch(type){case IngredientType.Bread:return "빵";case IngredientType.Ham:return "햄";case IngredientType.Tomato:return "토마토";case IngredientType.Cheese:return "치즈";case IngredientType.Mayonnaise:return "마요네즈";case IngredientType.Cabbage:return "양배추";default:return type.ToString();}}
    }
}
