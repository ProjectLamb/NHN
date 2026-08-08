//using System.Collections.Generic;
//using System.Text;
//using SandwichGame.Actions;
//using SandwichGame.AI;
//using SandwichGame.Ingredients;
//using SandwichGame.Sandwich;
//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
//using DG.Tweening;


//namespace SandwichGame.UI
//{
//    public class CommandInputUI:MonoBehaviour
//    {
//        [Header("Score Animation")]
//        [SerializeField] private float categoryCountDuration = 0.8f;
//        [SerializeField] private float totalCountDuration = 1.1f;
//        [SerializeField] private float scoreStepInterval = 0.15f;

//        [SerializeField] private AudioSource scoreAudioSource;
//        [SerializeField] private AudioClip scoreConfirmSound;

//        private Sequence scoreSequence;

//        private int displayedComposition;
//        private int displayedOrder;
//        private int displayedStructure;
//        private int displayedTotalScore;

//        private const float RoundSeconds=2f; // 여기 50초 바꾸기
//        [SerializeField]private TMP_InputField input;[SerializeField]private TMP_Text statusText;[SerializeField]private TMP_Text menuText;[SerializeField]private Button executeButton;[SerializeField]private AICommandManager manager;[SerializeField]private ActionExecutor executor;
//        private readonly List<IngredientType> missionRecipe=new List<IngredientType>();
//        private SandwichManager sandwichManager;private TMP_Text timerText;private TMP_Text scoreText;private GameObject gameEndPanel;private GameObject loadingCanvas;private float remainingTime;private bool timerStarted;private bool roundEnded;

//        public void Configure(TMP_InputField i,TMP_Text s,Button b,AICommandManager m,ActionExecutor e){input=i;statusText=s;executeButton=b;manager=m;executor=e;}
//        private void OnEnable(){if(executor!=null)executor.StatusChanged+=SetStatus;if(manager!=null)manager.RequestStateChanged+=OnRequest;}
//        private void OnDisable(){if(executor!=null)executor.StatusChanged-=SetStatus;if(manager!=null)manager.RequestStateChanged-=OnRequest;}
//        private void Start(){sandwichManager=FindObjectOfType<SandwichManager>();loadingCanvas=GameObject.Find("CanvasLoading");ResolveRoundUI();ShowRandomMission();remainingTime=RoundSeconds;UpdateTimer();}
//        private void Update(){if(roundEnded)return;if(!timerStarted){if(loadingCanvas!=null&&loadingCanvas.activeInHierarchy)return;timerStarted=true;}remainingTime=Mathf.Max(0f,remainingTime-Time.deltaTime);UpdateTimer();if(remainingTime<=0f)EndRound();}
//        public void Submit(){if(!roundEnded&&manager!=null&&input!=null)manager.Interpret(input.text);}
//        public void RestartGame()=>SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//        public void ExitToMainMenu()=>SceneManager.LoadScene("GameStartMenu");
//        private void SetStatus(string s){if(statusText!=null)statusText.text=s;}
//        private void OnRequest(bool busy,string s){if(executeButton!=null)executeButton.interactable=!busy&&!roundEnded;SetStatus(s);}

//        private void ShowRandomMission()
//        {
//            TMP_Text target=ResolveMenuText();if(target==null)return;
//            List<IngredientType> fillings=new List<IngredientType>{IngredientType.Ham,IngredientType.Tomato,IngredientType.Cheese,IngredientType.Mayonnaise,IngredientType.Cabbage};
//            for(int i=fillings.Count-1;i>0;i--){int j=Random.Range(0,i+1);IngredientType value=fillings[i];fillings[i]=fillings[j];fillings[j]=value;}
//            int fillingCount=Random.Range(2,fillings.Count+1);missionRecipe.Clear();missionRecipe.Add(IngredientType.Bread);for(int i=0;i<fillingCount;i++)missionRecipe.Add(fillings[i]);missionRecipe.Add(IngredientType.Bread);
//            StringBuilder mission=new StringBuilder();for(int i=0;i<missionRecipe.Count;i++){if(i>0)mission.Append('\n');mission.Append(KoreanName(missionRecipe[i]));}target.text=mission.ToString();
//        }
//        private void ResolveRoundUI()
//        {
//            Canvas canvas=input!=null?input.GetComponentInParent<Canvas>():null;if(canvas==null)return;
//            foreach(Transform child in canvas.GetComponentsInChildren<Transform>(true))
//            {
//                if(child.name=="Timer")timerText=child.GetComponent<TMP_Text>();
//                else if(child.name=="GameEndPanel")gameEndPanel=child.gameObject;
//                else if(child.name=="Score")scoreText=child.GetComponent<TMP_Text>();
//                else if(child.name=="다시하기"){Button button=child.GetComponent<Button>();if(button!=null)button.onClick.AddListener(RestartGame);}
//                else if(child.name=="게임종료"){Button button=child.GetComponent<Button>();if(button!=null)button.onClick.AddListener(ExitToMainMenu);}
//            }
//            if(gameEndPanel!=null)gameEndPanel.SetActive(false);
//        }
//        private TMP_Text ResolveMenuText()
//        {
//            if(menuText!=null)return menuText;Canvas canvas=input!=null?input.GetComponentInParent<Canvas>():null;
//            if(canvas!=null)foreach(Transform child in canvas.GetComponentsInChildren<Transform>(true))if(child.name=="Menu"){menuText=child.GetComponentInChildren<TMP_Text>(true);break;}
//            return menuText!=null?menuText:statusText;
//        }
//        private void UpdateTimer(){if(timerText!=null)timerText.text=$"남은 시간 : {Mathf.CeilToInt(remainingTime)}";}
//        private void EndRound()
//        {
//            roundEnded=true;if(input!=null)input.interactable=false;if(executeButton!=null)executeButton.interactable=false;
//            int score=CalculateScore(out int composition,out int order,out int structure);if(scoreText!=null)scoreText.text=$"재료 {composition}/50\n순서 {order}/40\n빵 구조 {structure}/10\n당신의 점수 : {score}점";if(gameEndPanel!=null)gameEndPanel.SetActive(true); //여기
//        }
//        private int CalculateScore(out int composition,out int order,out int structure)
//        {
//            List<IngredientType> actual=new List<IngredientType>();if(sandwichManager!=null)foreach(SandwichLayerData layer in sandwichManager.Layers)actual.Add(layer.ingredientType);
//            int denominator=Mathf.Max(missionRecipe.Count,actual.Count);int matched=0;foreach(IngredientType type in System.Enum.GetValues(typeof(IngredientType))){int expectedCount=missionRecipe.FindAll(x=>x==type).Count;int actualCount=actual.FindAll(x=>x==type).Count;matched+=Mathf.Min(expectedCount,actualCount);}
//            composition=denominator==0?0:Mathf.RoundToInt(50f*matched/denominator);order=denominator==0?0:Mathf.RoundToInt(40f*LongestCommonSubsequence(missionRecipe,actual)/denominator);structure=actual.Count>=2&&actual[0]==IngredientType.Bread&&actual[actual.Count-1]==IngredientType.Bread?10:0;return composition+order+structure;
//        }
//        private static int LongestCommonSubsequence(List<IngredientType> expected,List<IngredientType> actual)
//        {
//            int[,] lengths=new int[expected.Count+1,actual.Count+1];for(int i=1;i<=expected.Count;i++)for(int j=1;j<=actual.Count;j++)lengths[i,j]=expected[i-1]==actual[j-1]?lengths[i-1,j-1]+1:Mathf.Max(lengths[i-1,j],lengths[i,j-1]);return lengths[expected.Count,actual.Count];
//        }
//        private static string KoreanName(IngredientType type){switch(type){case IngredientType.Bread:return "빵";case IngredientType.Ham:return "햄";case IngredientType.Tomato:return "토마토";case IngredientType.Cheese:return "치즈";case IngredientType.Mayonnaise:return "마요네즈";case IngredientType.Cabbage:return "양배추";default:return type.ToString();}}
//    }
//}


using System.Collections.Generic;
using System.Text;
using DG.Tweening;
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
    [System.Serializable]
    public class SandwichRecipeCollection { public SandwichRecipeDefinition[] recipes; }

    [System.Serializable]
    public class SandwichRecipeDefinition
    {
        public string recipeId;
        public string displayName;
        public string sourceImage;
        public string[] layers;
    }

    public class CommandInputUI : MonoBehaviour
    {
        // =========================================================
        // 점수 애니메이션 설정
        // =========================================================

        [Header("Score Animation")]
        [SerializeField] private float categoryCountDuration = 2.2f;
        [SerializeField] private float totalCountDuration = 3f;
        [SerializeField] private float scoreStepInterval = 0.3f;
        [SerializeField] private float scoreRollInterval = 0.07f;

        [Header("Score Sound")]
        [SerializeField] private AudioSource scoreAudioSource;

        // 숫자가 굴러가는 동안 반복 재생
        [SerializeField] private AudioClip scoreRollSound;

        // 재료, 순서, 빵 구조 점수 확정음
        [SerializeField] private AudioClip scoreConfirmSound;

        // 최종 점수 확정음
        [SerializeField] private AudioClip finalScoreSound;

        private Sequence scoreSequence;

        private int displayedComposition;
        private int displayedOrder;
        private int displayedStructure;
        private int displayedTotalScore;

        // =========================================================
        // 게임 기본 설정
        // =========================================================

        private const float RoundSeconds = 100f;

        [SerializeField] private TMP_InputField input;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text menuText;
        [SerializeField] private Button executeButton;
        [SerializeField] private AICommandManager manager;
        [SerializeField] private ActionExecutor executor;

        [Header("Recipe Menu")]
        [SerializeField] private TextAsset recipeJson;
        [SerializeField] private Sprite[] recipeImages;

        private readonly List<IngredientType> missionRecipe =
            new List<IngredientType>();

        private SandwichManager sandwichManager;
        private TMP_Text timerText;
        private TMP_Text scoreText;
        private TMP_Text hintText;
        private Text legacyHintText;
        private Coroutine hintClearCoroutine;

        private GameObject gameEndPanel;
        private GameObject loadingCanvas;

        private float remainingTime;
        private bool timerStarted;
        private bool roundEnded;

        private const int MaxCommandRequests = 6;
        private int commandRequestCount;

        // =========================================================
        // Unity 생명주기
        // =========================================================

        public void Configure(
            TMP_InputField i,
            TMP_Text s,
            Button b,
            AICommandManager m,
            ActionExecutor e)
        {
            input = i;
            statusText = s;
            executeButton = b;
            manager = m;
            executor = e;
        }

        public void ConfigureRecipes(TextAsset json, Sprite[] images)
        {
            recipeJson = json;
            recipeImages = images;
        }

        private void OnEnable()
        {
            if (input != null)
                input.onSubmit.AddListener(OnInputSubmitted);

            if (executor != null)
            {
                executor.StatusChanged += SetStatus;
                executor.HintChanged += SetHint;
            }

            if (manager != null)
                manager.RequestStateChanged += OnRequest;
        }

        private void OnDisable()
        {
            if (input != null)
                input.onSubmit.RemoveListener(OnInputSubmitted);

            if (hintClearCoroutine != null)
            {
                StopCoroutine(hintClearCoroutine);
                hintClearCoroutine = null;
            }
            if (executor != null)
            {
                executor.StatusChanged -= SetStatus;
                executor.HintChanged -= SetHint;
            }

            if (manager != null)
                manager.RequestStateChanged -= OnRequest;
        }

        private void OnDestroy()
        {
            scoreSequence?.Kill();
            StopScoreRollSound();
        }

        private void Start()
        {
            sandwichManager = FindObjectOfType<SandwichManager>();
            loadingCanvas = GameObject.Find("CanvasLoading");

            ResolveRoundUI();
            ShowRandomMission();

            remainingTime = RoundSeconds;
            UpdateTimer();
            UpdateCommandButton();
        }

        private void Update()
        {
            if (roundEnded)
                return;

            // 로딩 화면이 사라진 뒤부터 타이머 시작
            if (!timerStarted)
            {
                if (loadingCanvas != null &&
                    loadingCanvas.activeInHierarchy)
                {
                    return;
                }

                timerStarted = true;
            }

            remainingTime = Mathf.Max(
                0f,
                remainingTime - Time.deltaTime
            );

            UpdateTimer();

            if (remainingTime <= 0f)
                EndRound();
        }

        // =========================================================
        // 명령 입력
        // =========================================================

        public void Submit()
        {
            if (!roundEnded &&
                manager != null &&
                input != null &&
                !manager.IsBusy &&
                commandRequestCount < MaxCommandRequests &&
                !string.IsNullOrWhiteSpace(input.text))
            {
                string command = input.text;
                commandRequestCount++;
                UpdateCommandButton();
                manager.Interpret(command);
                input.text = string.Empty;
                input.ActivateInputField();
            }
        }

        private void OnInputSubmitted(string unused)
        {
            Submit();
        }

        private void UpdateCommandButton()
        {
            if (executeButton == null) return;
            TMP_Text tmpLabel = executeButton.GetComponentInChildren<TMP_Text>(true);
            if (tmpLabel != null) tmpLabel.text = $"명령 ({commandRequestCount}/{MaxCommandRequests})";
            else
            {
                Text legacyLabel = executeButton.GetComponentInChildren<Text>(true);
                if (legacyLabel != null) legacyLabel.text = $"명령 ({commandRequestCount}/{MaxCommandRequests})";
            }
            executeButton.interactable = commandRequestCount < MaxCommandRequests && !roundEnded && (manager == null || !manager.IsBusy);
        }

        public void RestartGame()
        {
            scoreSequence?.Kill();
            StopScoreRollSound();

            SceneManager.LoadScene(
                SceneManager.GetActiveScene().name
            );
        }

        public void ExitToMainMenu()
        {
            scoreSequence?.Kill();
            StopScoreRollSound();

            SceneManager.LoadScene("GameStartMenu");
        }

        public void ProvideToCustomer()
        {
            EndRound();
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void SetHint(string message)
        {
            if (hintClearCoroutine != null)
            {
                StopCoroutine(hintClearCoroutine);
                hintClearCoroutine = null;
            }
            if (hintText != null) hintText.text = message;
            else if (legacyHintText != null) legacyHintText.text = message;
            if (!string.IsNullOrEmpty(message)) hintClearCoroutine = StartCoroutine(ClearHintAfterDelay());
        }

        private System.Collections.IEnumerator ClearHintAfterDelay()
        {
            yield return new WaitForSecondsRealtime(1f);
            if (hintText != null) hintText.text = string.Empty;
            else if (legacyHintText != null) legacyHintText.text = string.Empty;
            hintClearCoroutine = null;
        }

        private void OnRequest(bool busy, string message)
        {
            if (executeButton != null)
            {
                executeButton.interactable =
                    !busy && !roundEnded && commandRequestCount < MaxCommandRequests;
            }

            SetStatus(message);
        }

        // =========================================================
        // 랜덤 미션 생성
        // =========================================================

        private void ShowRandomMission()
        {
            if (recipeJson == null)
            {
                SetStatus("SandwichRecipes.json이 연결되지 않았습니다.");
                return;
            }

            SandwichRecipeCollection collection = JsonUtility.FromJson<SandwichRecipeCollection>(recipeJson.text);
            if (collection == null || collection.recipes == null || collection.recipes.Length == 0)
            {
                SetStatus("레시피 JSON에 사용할 레시피가 없습니다.");
                return;
            }

            SandwichRecipeDefinition selected = collection.recipes[Random.Range(0, collection.recipes.Length)];
            missionRecipe.Clear();
            if (selected.layers != null)
                foreach (string layer in selected.layers)
                    if (System.Enum.TryParse(layer, true, out IngredientType type)) missionRecipe.Add(type);

            Image menuImage = ResolveMenuImage();
            Sprite selectedSprite = FindRecipeSprite(selected.sourceImage);
            if (menuImage != null && selectedSprite != null)
            {
                menuImage.sprite = selectedSprite;
                menuImage.preserveAspect = true;
            }

            TMP_Text target = ResolveMenuText();
            if (target != null && target != statusText)
            {
                StringBuilder mission = new StringBuilder(selected.displayName);
                foreach (IngredientType type in missionRecipe) mission.Append('\n').Append(KoreanName(type));
                target.text = mission.ToString();
            }
        }

        private Image ResolveMenuImage()
        {
            Canvas canvas = input != null ? input.GetComponentInParent<Canvas>() : null;
            if (canvas == null) return null;
            foreach (Transform child in canvas.GetComponentsInChildren<Transform>(true))
                if (child.name == "Menu") return child.GetComponent<Image>();
            return null;
        }

        private Sprite FindRecipeSprite(string sourceImage)
        {
            if (recipeImages == null || string.IsNullOrEmpty(sourceImage)) return null;
            string spriteName = sourceImage.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)
                ? sourceImage.Substring(0, sourceImage.Length - 4) : sourceImage;
            foreach (Sprite sprite in recipeImages) if (sprite != null && sprite.name == spriteName) return sprite;
            return null;
        }

        private void ShowLegacyRandomMission()
        {
            TMP_Text target = ResolveMenuText();

            if (target == null)
                return;

            List<IngredientType> fillings =
                new List<IngredientType>
                {
                    IngredientType.Ham,
                    IngredientType.Tomato,
                    IngredientType.Cheese,
                    IngredientType.Mayonnaise,
                    IngredientType.Cabbage
                };

            // 재료 순서 섞기
            for (int i = fillings.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);

                IngredientType value = fillings[i];
                fillings[i] = fillings[j];
                fillings[j] = value;
            }

            int fillingCount =
                Random.Range(2, fillings.Count + 1);

            missionRecipe.Clear();
            missionRecipe.Add(IngredientType.Bread);

            for (int i = 0; i < fillingCount; i++)
                missionRecipe.Add(fillings[i]);

            missionRecipe.Add(IngredientType.Bread);

            StringBuilder mission = new StringBuilder();

            for (int i = 0; i < missionRecipe.Count; i++)
            {
                if (i > 0)
                    mission.Append('\n');

                mission.Append(KoreanName(missionRecipe[i]));
            }

            target.text = mission.ToString();
        }

        // =========================================================
        // UI 자동 탐색
        // =========================================================

        private void ResolveRoundUI()
        {
            Canvas canvas =
                input != null
                    ? input.GetComponentInParent<Canvas>()
                    : null;

            if (canvas == null)
                return;

            Transform[] children =
                canvas.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.name == "Timer")
                {
                    timerText = child.GetComponent<TMP_Text>();
                }
                else if (child.name == "GameEndPanel")
                {
                    gameEndPanel = child.gameObject;
                }
                else if (child.name == "Score")
                {
                    scoreText = child.GetComponent<TMP_Text>();
                }
                else if (child.name == "Hint")
                {
                    hintText = child.GetComponent<TMP_Text>();
                    legacyHintText = child.GetComponent<Text>();
                    SetHint(string.Empty);
                }
                else if (child.name == "GetSandwichToCosBT")
                {
                    Button button = child.GetComponent<Button>();
                    if (button != null) button.onClick.AddListener(ProvideToCustomer);
                }
                else if (child.name == "다시하기")
                {
                    Button button = child.GetComponent<Button>();

                    if (button != null)
                        button.onClick.AddListener(RestartGame);
                }
                else if (child.name == "게임종료")
                {
                    Button button = child.GetComponent<Button>();

                    if (button != null)
                        button.onClick.AddListener(ExitToMainMenu);
                }
            }

            if (gameEndPanel != null)
                gameEndPanel.SetActive(false);
        }

        private TMP_Text ResolveMenuText()
        {
            if (menuText != null)
                return menuText;

            Canvas canvas =
                input != null
                    ? input.GetComponentInParent<Canvas>()
                    : null;

            if (canvas != null)
            {
                Transform[] children =
                    canvas.GetComponentsInChildren<Transform>(true);

                foreach (Transform child in children)
                {
                    if (child.name != "Menu")
                        continue;

                    menuText =
                        child.GetComponentInChildren<TMP_Text>(true);

                    break;
                }
            }

            return menuText != null ? menuText : statusText;
        }

        private void UpdateTimer()
        {
            if (timerText != null)
            {
                timerText.text =
                    $"남은 시간 : {Mathf.CeilToInt(remainingTime)}";
            }
        }

        // =========================================================
        // 라운드 종료
        // =========================================================

        private void EndRound()
        {
            if (roundEnded)
                return;

            roundEnded = true;

            if (input != null)
                input.interactable = false;

            if (executeButton != null)
                executeButton.interactable = false;

            int score = CalculateScore(
                out int composition,
                out int order,
                out int structure
            );

            if (gameEndPanel != null)
                gameEndPanel.SetActive(true);

            PlayScoreAnimation(
                composition,
                order,
                structure,
                score
            );
        }

        // =========================================================
        // 점수 연출
        // =========================================================

        private void PlayScoreAnimation(
            int composition,
            int order,
            int structure,
            int totalScore)
        {
            if (scoreText == null)
            {
                Debug.LogWarning(
                    "Score라는 이름의 TMP Text를 찾지 못했습니다."
                );

                return;
            }

            // 기존 연출 및 롤링음 정지
            scoreSequence?.Kill();
            scoreText.transform.DOKill();
            StopScoreRollSound();

            displayedComposition = 0;
            displayedOrder = 0;
            displayedStructure = 0;
            displayedTotalScore = 0;

            UpdateScoreText();

            scoreSequence = DOTween.Sequence();

            // 결과창이 열린 뒤 잠시 대기
            scoreSequence.AppendInterval(0.5f);

            // -----------------------------------------------------
            // 1. 재료 점수
            // -----------------------------------------------------

            scoreSequence.AppendCallback(StartScoreRollSound);

            scoreSequence.Append(
                RollScore(
                    50,
                    composition,
                    categoryCountDuration,
                    value => displayedComposition = value
                )
            );

            // 롤링 종료와 동시에 소리 정지
            scoreSequence.AppendCallback(StopScoreRollSound);
            scoreSequence.AppendCallback(PlayScoreConfirmEffect);

            scoreSequence.Append(
                scoreText.transform.DOPunchScale(
                    Vector3.one * 0.08f,
                    0.25f,
                    6,
                    0.5f
                )
            );

            scoreSequence.AppendInterval(scoreStepInterval);

            // -----------------------------------------------------
            // 2. 순서 점수
            // -----------------------------------------------------

            scoreSequence.AppendCallback(StartScoreRollSound);

            scoreSequence.Append(
                RollScore(
                    40,
                    order,
                    categoryCountDuration,
                    value => displayedOrder = value
                )
            );

            // 롤링 종료와 동시에 소리 정지
            scoreSequence.AppendCallback(StopScoreRollSound);
            scoreSequence.AppendCallback(PlayScoreConfirmEffect);

            scoreSequence.Append(
                scoreText.transform.DOPunchScale(
                    Vector3.one * 0.08f,
                    0.25f,
                    6,
                    0.5f
                )
            );

            scoreSequence.AppendInterval(scoreStepInterval);

            // -----------------------------------------------------
            // 3. 빵 구조 점수
            // -----------------------------------------------------

            scoreSequence.AppendCallback(StartScoreRollSound);

            scoreSequence.Append(
                RollScore(
                    10,
                    structure,
                    categoryCountDuration,
                    value => displayedStructure = value
                )
            );

            // 롤링 종료와 동시에 소리 정지
            scoreSequence.AppendCallback(StopScoreRollSound);
            scoreSequence.AppendCallback(PlayScoreConfirmEffect);

            scoreSequence.Append(
                scoreText.transform.DOPunchScale(
                    Vector3.one * 0.08f,
                    0.25f,
                    6,
                    0.5f
                )
            );

            scoreSequence.AppendInterval(scoreStepInterval + 0.3f);

            // -----------------------------------------------------
            // 4. 최종 점수
            // -----------------------------------------------------

            scoreSequence.AppendCallback(StartScoreRollSound);

            scoreSequence.Append(
                RollScore(
                    100,
                    totalScore,
                    totalCountDuration,
                    value => displayedTotalScore = value
                )
            );

            // 롤링 종료와 동시에 소리 정지
            scoreSequence.AppendCallback(StopScoreRollSound);
            scoreSequence.AppendCallback(PlayFinalScoreEffect);

            scoreSequence.Append(
                scoreText.transform.DOPunchScale(
                    Vector3.one * 0.2f,
                    0.45f,
                    10,
                    0.7f
                )
            );

            // Time.timeScale이 0이어도 실행
            scoreSequence.SetUpdate(true);
        }

        /// <summary>
        /// 일정 시간 동안 랜덤 숫자를 보여주다가
        /// 마지막 순간에 실제 점수로 확정한다.
        /// </summary>
        private Tween RollScore(
            int maximumScore,
            int finalScore,
            float duration,
            System.Action<int> setValue)
        {
            float previousElapsed = -scoreRollInterval;
            int previousRandomValue = -1;

            return DOVirtual.Float(
                    0f,
                    duration,
                    duration,
                    elapsed =>
                    {
                        if (elapsed - previousElapsed <
                            scoreRollInterval)
                        {
                            return;
                        }

                        previousElapsed = elapsed;

                        int randomValue;

                        do
                        {
                            randomValue = Random.Range(
                                0,
                                maximumScore + 1
                            );
                        }
                        while (
                            maximumScore > 0 &&
                            randomValue == previousRandomValue
                        );

                        previousRandomValue = randomValue;

                        setValue(randomValue);
                        UpdateScoreText();
                    }
                )
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    // 마지막 순간에 실제 점수로 확정
                    setValue(finalScore);
                    UpdateScoreText();
                });
        }

        private void UpdateScoreText()
        {
            if (scoreText == null)
                return;

            scoreText.text =
                $"재료 {displayedComposition}/50\n" +
                $"순서 {displayedOrder}/40\n" +
                $"빵 구조 {displayedStructure}/10\n" +
                $"당신의 점수 : {displayedTotalScore}점";
        }

        // =========================================================
        // 점수 효과음
        // =========================================================

        /// <summary>
        /// 현재 점수 항목의 숫자 롤링 시작과 동시에
        /// 뚜루루루 음원을 처음부터 반복 재생한다.
        /// </summary>
        private void StartScoreRollSound()
        {
            if (scoreAudioSource == null ||
                scoreRollSound == null)
            {
                return;
            }

            scoreAudioSource.Stop();
            scoreAudioSource.playOnAwake = false;
            scoreAudioSource.clip = scoreRollSound;
            scoreAudioSource.loop = true;
            scoreAudioSource.Play();
        }

        /// <summary>
        /// 현재 점수 항목의 숫자 롤링이 끝나는 순간
        /// 뚜루루루 음원을 즉시 정지한다.
        /// </summary>
        private void StopScoreRollSound()
        {
            if (scoreAudioSource == null)
                return;

            scoreAudioSource.Stop();
            scoreAudioSource.loop = false;
            scoreAudioSource.clip = null;
        }

        /// <summary>
        /// 재료, 순서, 빵 구조 점수가 확정될 때 재생한다.
        /// </summary>
        private void PlayScoreConfirmEffect()
        {
            if (scoreAudioSource != null &&
                scoreConfirmSound != null)
            {
                scoreAudioSource.PlayOneShot(
                    scoreConfirmSound
                );
            }
        }

        /// <summary>
        /// 최종 점수가 확정될 때 재생한다.
        /// finalScoreSound가 없으면 일반 확정음을 사용한다.
        /// </summary>
        private void PlayFinalScoreEffect()
        {
            if (scoreAudioSource == null)
                return;

            AudioClip soundToPlay =
                finalScoreSound != null
                    ? finalScoreSound
                    : scoreConfirmSound;

            if (soundToPlay != null)
            {
                scoreAudioSource.PlayOneShot(
                    soundToPlay
                );
            }
        }

        // =========================================================
        // 실제 점수 계산
        // =========================================================

        private int CalculateScore(
            out int composition,
            out int order,
            out int structure)
        {
            List<IngredientType> actual =
                new List<IngredientType>();

            if (sandwichManager != null)
            {
                foreach (SandwichLayerData layer
                         in sandwichManager.Layers)
                {
                    actual.Add(layer.ingredientType);
                }
            }

            int denominator =
                Mathf.Max(missionRecipe.Count, actual.Count);

            int matched = 0;

            foreach (IngredientType type
                     in System.Enum.GetValues(
                         typeof(IngredientType)))
            {
                int expectedCount =
                    missionRecipe.FindAll(x => x == type).Count;

                int actualCount =
                    actual.FindAll(x => x == type).Count;

                matched += Mathf.Min(
                    expectedCount,
                    actualCount
                );
            }

            // 재료 구성 점수: 50점
            composition =
                denominator == 0
                    ? 0
                    : Mathf.RoundToInt(
                        50f * matched / denominator
                    );

            // 재료 순서 점수: 40점
            order =
                denominator == 0
                    ? 0
                    : Mathf.RoundToInt(
                        40f *
                        LongestCommonSubsequence(
                            missionRecipe,
                            actual
                        ) /
                        denominator
                    );

            // 위아래 빵 구조 점수: 10점
            structure =
                actual.Count >= 2 &&
                actual[0] == IngredientType.Bread &&
                actual[actual.Count - 1] ==
                IngredientType.Bread
                    ? 10
                    : 0;

            return composition + order + structure;
        }

        private static int LongestCommonSubsequence(
            List<IngredientType> expected,
            List<IngredientType> actual)
        {
            int[,] lengths =
                new int[
                    expected.Count + 1,
                    actual.Count + 1
                ];

            for (int i = 1; i <= expected.Count; i++)
            {
                for (int j = 1; j <= actual.Count; j++)
                {
                    if (expected[i - 1] == actual[j - 1])
                    {
                        lengths[i, j] =
                            lengths[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        lengths[i, j] = Mathf.Max(
                            lengths[i - 1, j],
                            lengths[i, j - 1]
                        );
                    }
                }
            }

            return lengths[
                expected.Count,
                actual.Count
            ];
        }

        private static string KoreanName(
            IngredientType type)
        {
            switch (type)
            {
                case IngredientType.Bread:
                    return "빵";

                case IngredientType.Ham:
                    return "햄";

                case IngredientType.Tomato:
                    return "토마토";

                case IngredientType.Cheese:
                    return "치즈";

                case IngredientType.Mayonnaise:
                    return "마요네즈";

                case IngredientType.Cabbage:
                    return "양배추";

                default:
                    return type.ToString();
            }
        }
    }
}
