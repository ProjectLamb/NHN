//using System;
//using System.Collections;
//using System.Collections.Generic;
//using SandwichGame.AI;
//using SandwichGame.Ingredients;
//using SandwichGame.Sandwich;
//using UnityEngine;

//namespace SandwichGame.Actions
//{
//    public class ActionExecutor : MonoBehaviour
//    {
//        [SerializeField] private IngredientStateManager stateManager;
//        [SerializeField] private SandwichManager sandwichManager;
//        [SerializeField] private SandwichValidator validator;
//        [SerializeField] private bool continueAfterFailedAction = true;
//        [Header("Presentation")]
//        [SerializeField] private float cheeseBoxOpenSeconds = .45f;
//        [SerializeField] private float mayonnaiseAnimationSeconds = 1.2f;

//        public event Action<string> StatusChanged;
//        public event Action<string> HintChanged;

//        public void Configure(IngredientStateManager state, SandwichManager sandwich, SandwichValidator sandwichValidator)
//        { stateManager = state; sandwichManager = sandwich; validator = sandwichValidator; }

//        public void Execute(SandwichActionData[] actions)
//        {
//            if (actions == null || actions.Length == 0)
//            { Report("실행할 동작이 없습니다.", true); HintChanged?.Invoke("힌트: 재료와 행동을 함께 말해주세요. 예) 양배추를 잘라서 올려줘"); return; }
//            HintChanged?.Invoke(string.Empty);
//            for (int i = 0; i < actions.Length; i++)
//            {
//                SandwichActionData action = actions[i];
//                bool hasLaterPut = HasLaterPutForSameIngredient(actions, i, action);
//                bool ok = TryExecute(action, !hasLaterPut, out string message);
//                Report(message, !ok);
//                if (!ok) HintChanged?.Invoke(BuildHint(action));
//                if (!ok && !continueAfterFailedAction) break;
//            }
//        }

//        private bool TryExecute(SandwichActionData action, bool autoPutWhenReady, out string message)
//        {
//            message = "잘못된 action입니다.";
//            if (action == null || !Allowed.Contains(action.action)) return false;
//            if (action.action == "Finish") return FinishActionHandler.Execute(validator, out message);
//            if (!Enum.TryParse(action.targetIngredient, true, out IngredientType type)) { message = $"잘못된 재료: {action.targetIngredient}"; return false; }

//            bool ok;
//            switch (action.action)
//            {
//                case "Open": ok = OpenActionHandler.Execute(stateManager, type, out message); break;
//                case "TakeOff": ok = TakeOffActionHandler.Execute(stateManager, type, action.amount, out message); break;
//                case "Cut": ok = CutActionHandler.Execute(stateManager, type, action.amount, out message); break;
//                case "Put": ok = PutActionHandler.Execute(stateManager, sandwichManager, type, out message); break;
//                default: return false;
//            }

//            // Some natural-language commands prepare a complete ingredient but omit
//            // an explicit Put action. In that case, stack it immediately. If a Put for
//            // the same ingredient appears later in this response, defer to that action
//            // so the layer is not created twice.
//            if (ok && action.action != "Put" && autoPutWhenReady &&
//                stateManager.IsReadyToPut(type, out _) &&
//                stateManager.TryGetCurrentPrefab(type, out GameObject readyPrefab, out string readyStateId))
//            {
//                ok = sandwichManager.TryAddLayer(type, readyStateId, readyPrefab, out message);
//                if (ok)
//                    stateManager.ResetToInitialState(type);
//            }

//            if (ok) { PlayPresentation(action.action, type, action.amount); message = $"{action.action} {action.targetIngredient} 완료"; }
//            return ok;
//        }

//        private static bool HasLaterPutForSameIngredient(
//            SandwichActionData[] actions,
//            int currentIndex,
//            SandwichActionData current)
//        {
//            if (current == null || string.IsNullOrEmpty(current.targetIngredient))
//                return false;

//            for (int i = currentIndex + 1; i < actions.Length; i++)
//            {
//                SandwichActionData candidate = actions[i];
//                if (candidate != null &&
//                    candidate.action == "Put" &&
//                    string.Equals(candidate.targetIngredient, current.targetIngredient, StringComparison.OrdinalIgnoreCase))
//                    return true;
//            }

//            return false;
//        }

//        private string BuildHint(SandwichActionData action)
//        {
//            if (action == null || !Enum.TryParse(action.targetIngredient, true, out IngredientType type))
//                return "힌트: 빵, 양배추, 토마토, 햄, 치즈, 마요네즈 중 재료를 정확히 말해주세요.";
//            string state = stateManager.GetState(type)?.currentStateId ?? string.Empty;
//            switch (type)
//            {
//                case IngredientType.Bread: return state == "BREAD_BAG_OPENED_LOAF" ? "COMMAND FAILED\n빵을 원하는 크기로 썰어서 올려주세요." : "COMMAND FAILED\n빵은 비닐을 벗기고 원하는 크기로 썰어서 올려주세요.";
//                case IngredientType.Cabbage: return "COMMAND FAILED\n양배추는 원하는 크기로 잘라서 올려주세요.";
//                case IngredientType.Tomato: return "COMMAND FAILED\n토마토는 꺼내서 올려주세요.";
//                case IngredientType.Ham: return "COMMAND FAILED\n햄은 꺼내서 올려주세요.";
//                case IngredientType.Cheese:
//                    if (state == "CHEESE_PACK_OPENED_STACK") return "COMMAND FAILED\n열린 치즈 상자에서 치즈를 꺼내고 비닐을 벗겨서 올려주세요.";
//                    if (state.Contains("TAKEOFF_CHEESE_SLICE_WRAPPED")) return "COMMAND FAILED\n꺼낸 치즈의 비닐을 벗겨서 올려주세요.";
//                    return "COMMAND FAILED\n치즈 상자를 열고 치즈를 꺼낸 뒤 비닐을 벗겨서 올려주세요.";
//                case IngredientType.Mayonnaise: return "COMMAND FAILED\n마요네즈는 조금, 적당히 또는 많이 짜서 올려주세요.";
//                default: return "COMMAND FAILED\n재료를 손질한 다음 올려주세요.";
//            }
//        }

//        private void PlayPresentation(string action, IngredientType type, string amount)
//        {
//            if (type == IngredientType.Cheese && action == "Open" && stateManager.GetState(type)?.currentStateId == "CHEESE_PACK_OPENED_STACK") StartCoroutine(OpenCheeseBox());
//            if (type == IngredientType.Mayonnaise && action == "TakeOff") StartCoroutine(PlayMayonnaise(amount));
//        }

//        private IEnumerator OpenCheeseBox()
//        {
//            Transform top = FindTransform("Cheese_box_top"); if (top == null) yield break;
//            Quaternion from = top.localRotation; Quaternion to = Quaternion.Euler(top.localEulerAngles.x, top.localEulerAngles.y, -80f);
//            for (float time = 0; time < cheeseBoxOpenSeconds; time += Time.deltaTime) { top.localRotation = Quaternion.Slerp(from, to, time / cheeseBoxOpenSeconds); yield return null; }
//            top.localRotation = to;
//        }

//        private IEnumerator PlayMayonnaise(string amount)
//        {
//            GameObject bottle = FindGameObject("MayoBottleSoucing") ?? FindGameObject("MayoBottle");
//            if (bottle != null) { bottle.SetActive(true); Animator animator = bottle.GetComponentInChildren<Animator>(true); if (animator != null) animator.Play(0, 0, 0f); }
//            SkinnedMeshRenderer mayo = FindBlendShapeRenderer("Mayo_tong", "Mayo Use", out int index);
//            float target = amount == "L" ? 100f : amount == "S" ? 35f : 65f; float start = mayo != null ? mayo.GetBlendShapeWeight(index) : 0f;
//            for (float time = 0; time < mayonnaiseAnimationSeconds; time += Time.deltaTime) { if (mayo != null) mayo.SetBlendShapeWeight(index, Mathf.Lerp(start, target, time / mayonnaiseAnimationSeconds)); yield return null; }
//            if (mayo != null) mayo.SetBlendShapeWeight(index, target); if (bottle != null) bottle.SetActive(false);
//        }

//        private static SkinnedMeshRenderer FindBlendShapeRenderer(string rootName, string shapeName, out int index)
//        { index = -1; GameObject root = FindGameObject(rootName); if (root == null) return null; foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)) for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++) if (renderer.sharedMesh.GetBlendShapeName(i) == shapeName) { index = i; return renderer; } return null; }
//        private static GameObject FindGameObject(string name) { foreach (Transform transform in FindObjectsOfType<Transform>(true)) if (transform.name == name) return transform.gameObject; return null; }
//        private static Transform FindTransform(string name) => FindGameObject(name)?.transform;
//        private void Report(string message, bool error) { if (error) Debug.LogError(message); else Debug.Log(message); StatusChanged?.Invoke(message); }
//        private static readonly HashSet<string> Allowed = new HashSet<string> { "Open", "TakeOff", "Cut", "Put", "Finish" };
//    }
//}

using System;
using System.Collections;
using System.Collections.Generic;
using SandwichGame.AI;
using SandwichGame.Ingredients;
using SandwichGame.Sandwich;
using TMPro;
using UnityEngine;

namespace SandwichGame.Actions
{
    public class ActionExecutor : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField]
        private IngredientStateManager stateManager;

        [SerializeField]
        private SandwichManager sandwichManager;

        [SerializeField]
        private SandwichValidator validator;

        [SerializeField]
        private bool continueAfterFailedAction = true;

        [Header("Presentation")]
        [SerializeField]
        private float cheeseBoxOpenSeconds = 0.45f;

        [SerializeField]
        private float mayonnaiseAnimationSeconds = 1.2f;

        [Header("Hint UI")]
        [Tooltip("Image와 Text(TMP)를 포함하는 Hint 부모 오브젝트")]
        [SerializeField]
        private GameObject hintRoot;

        [Tooltip("Hint 오브젝트 자식에 있는 TextMeshProUGUI")]
        [SerializeField]
        private TMP_Text hintText;

        [Tooltip("힌트가 화면에 표시되는 시간")]
        [SerializeField, Min(0f)]
        private float hintDisplaySeconds = 4f;

        public event Action<string> StatusChanged;
        public event Action<string> HintChanged;

        private Coroutine hintCoroutine;

        private void Awake()
        {
            ResolveHintUI();

            if (hintRoot != null)
            {
                hintRoot.SetActive(false);
            }
        }

        public void Configure(
            IngredientStateManager state,
            SandwichManager sandwich,
            SandwichValidator sandwichValidator)
        {
            stateManager = state;
            sandwichManager = sandwich;
            validator = sandwichValidator;
        }

        public void Execute(SandwichActionData[] actions)
        {
            HideHint();

            if (actions == null || actions.Length == 0)
            {
                Report("실행할 동작이 없습니다.", true);

                ShowHint(
                    "힌트: 재료와 행동을 함께 말해주세요.\n" +
                    "예) 양배추를 잘라서 올려줘"
                );

                return;
            }

            for (int i = 0; i < actions.Length; i++)
            {
                SandwichActionData action = actions[i];

                if (action != null &&
                    action.action == "Cut" &&
                    !HasLaterPutForSameIngredient(actions, i, action))
                {
                    string missingPutHint = BuildMissingPutHint(action);
                    Report("COMMAND FAILED", true);
                    ShowHint(missingPutHint);

                    if (!continueAfterFailedAction)
                    {
                        break;
                    }

                    continue;
                }

                bool ok = TryExecute(
                    action,
                    out string message
                );

                Report(message, !ok);

                if (!ok)
                {
                    ShowHint(BuildHint(action));
                }

                if (!ok && !continueAfterFailedAction)
                {
                    break;
                }
            }
        }

        private static bool HasLaterPutForSameIngredient(
            SandwichActionData[] actions,
            int currentIndex,
            SandwichActionData current)
        {
            if (current == null ||
                string.IsNullOrEmpty(current.targetIngredient))
            {
                return false;
            }

            for (int i = currentIndex + 1; i < actions.Length; i++)
            {
                SandwichActionData candidate = actions[i];

                if (candidate != null &&
                    candidate.action == "Put" &&
                    string.Equals(
                        candidate.targetIngredient,
                        current.targetIngredient,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildMissingPutHint(
            SandwichActionData action)
        {
            string ingredient =
                string.Equals(
                    action.targetIngredient,
                    "cabbage",
                    StringComparison.OrdinalIgnoreCase)
                    ? "양배추를"
                    : string.Equals(
                        action.targetIngredient,
                        "bread",
                        StringComparison.OrdinalIgnoreCase)
                        ? "빵을"
                        : "재료를";

            return
                "COMMAND FAILED\n" +
                ingredient + " 자른 뒤 올려달라고 함께 말해주세요.";
        }

        public void RejectCommand(string hint)
        {
            const string failure = "COMMAND FAILED";
            Report(failure, true);
            ShowHint(failure + "\n" + hint);
        }

        public void ResetAll()
        {
            sandwichManager?.ClearAll();
            stateManager?.ResetAll();
            HideHint();
            Report("모든 재료를 초기화했습니다.", false);
        }

        private bool TryExecute(
            SandwichActionData action,
            out string message)
        {
            message = "잘못된 action입니다.";

            if (action == null ||
                !Allowed.Contains(action.action))
            {
                return false;
            }

            if (action.action == "Finish")
            {
                return FinishActionHandler.Execute(
                    validator,
                    out message
                );
            }

            if (!Enum.TryParse(
                    action.targetIngredient,
                    true,
                    out IngredientType type))
            {
                message =
                    $"잘못된 재료: {action.targetIngredient}";

                return false;
            }

            bool ok;

            switch (action.action)
            {
                case "Open":
                    ok = OpenActionHandler.Execute(
                        stateManager,
                        type,
                        out message
                    );
                    break;

                case "TakeOff":
                    ok = TakeOffActionHandler.Execute(
                        stateManager,
                        type,
                        action.amount,
                        out message
                    );
                    break;

                case "Cut":
                    ok = CutActionHandler.Execute(
                        stateManager,
                        type,
                        action.amount,
                        out message
                    );
                    break;

                case "Put":
                    ok = PutActionHandler.Execute(
                        stateManager,
                        sandwichManager,
                        type,
                        out message
                    );
                    break;

                default:
                    return false;
            }

            /*
             * 손질이 완료됐지만 Put 명령이 생략된 경우
             * 자동으로 샌드위치에 재료를 올린다.
             *
             * 동일 응답 안에 Put 명령이 뒤에 있다면
             * 중복 생성을 막기 위해 자동 배치하지 않는다.
             */
            if (ok)
            {
                PlayPresentation(
                    action.action,
                    type,
                    action.amount
                );

                message =
                    $"{action.action} " +
                    $"{action.targetIngredient} 완료";
            }

            return ok;
        }

        private string BuildHint(
            SandwichActionData action)
        {
            if (action == null ||
                !Enum.TryParse(
                    action.targetIngredient,
                    true,
                    out IngredientType type))
            {
                return
                    "COMMAND FAILED\n" +
                    "빵, 양배추, 토마토, 햄, 치즈, " +
                    "마요네즈 중 재료를 정확히 말해주세요.";
            }

            string state =
                stateManager.GetState(type)?.currentStateId ??
                string.Empty;

            switch (type)
            {
                case IngredientType.Bread:
                    if (state == "BREAD_BAG_OPENED_LOAF")
                    {
                        return
                            "COMMAND FAILED\n" +
                            "빵을 원하는 크기로 썰어서 " +
                            "올려주세요.";
                    }

                    return
                        "COMMAND FAILED\n" +
                        "빵은 비닐을 벗기고 원하는 크기로 " +
                        "썰어서 올려주세요.";

                case IngredientType.Cabbage:
                    return
                        "COMMAND FAILED\n" +
                        "양배추는 원하는 크기로 잘라서 " +
                        "올려주세요.";

                case IngredientType.Tomato:
                    return
                        "COMMAND FAILED\n" +
                        "토마토는 꺼내서 올려주세요.";

                case IngredientType.Ham:
                    return
                        "COMMAND FAILED\n" +
                        "햄은 꺼내서 올려주세요.";

                case IngredientType.Cheese:
                    if (state ==
                        "CHEESE_PACK_OPENED_STACK")
                    {
                        return
                            "COMMAND FAILED\n" +
                            "열린 치즈 상자에서 치즈를 꺼내고 " +
                            "비닐을 벗겨서 올려주세요.";
                    }

                    if (state.Contains(
                            "TAKEOFF_CHEESE_SLICE_WRAPPED"))
                    {
                        return
                            "COMMAND FAILED\n" +
                            "꺼낸 치즈의 비닐을 벗겨서 " +
                            "올려주세요.";
                    }

                    return
                        "COMMAND FAILED\n" +
                        "치즈 상자를 열고 치즈를 꺼낸 뒤 " +
                        "비닐을 벗겨서 올려주세요.";

                case IngredientType.Mayonnaise:
                    if (state == "MAYO_BOTTLE_CLOSED")
                    {
                        return
                            "COMMAND FAILED\n" +
                            "마요네즈 뚜껑을 먼저 따고, " +
                            "조금, 적당히 또는 많이 짜주세요.";
                    }

                    return
                        "COMMAND FAILED\n" +
                        "마요네즈는 조금, 적당히 또는 많이 " +
                        "짜서 올려주세요.";

                default:
                    return
                        "COMMAND FAILED\n" +
                        "재료를 손질한 다음 올려주세요.";
            }
        }

        // =========================================================
        // Hint UI
        // =========================================================

        private void ResolveHintUI()
        {
            if (hintRoot != null && hintText == null)
            {
                hintText =
                    hintRoot.GetComponentInChildren<TMP_Text>(true);
            }

            if (hintRoot == null)
            {
                GameObject foundHint =
                    FindGameObject("Hint");

                if (foundHint != null)
                {
                    hintRoot = foundHint;

                    hintText =
                        hintRoot.GetComponentInChildren<TMP_Text>(
                            true
                        );
                }
            }

            if (hintRoot == null)
            {
                Debug.LogWarning(
                    "Hint 부모 오브젝트를 찾지 못했습니다."
                );
            }

            if (hintText == null)
            {
                Debug.LogWarning(
                    "Hint의 자식 TMP Text를 찾지 못했습니다."
                );
            }
        }

        private void ShowHint(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                HideHint();
                return;
            }

            if (hintCoroutine != null)
            {
                StopCoroutine(hintCoroutine);
                hintCoroutine = null;
            }

            if (hintText != null)
            {
                hintText.text = message;
            }

            if (hintRoot != null)
            {
                hintRoot.SetActive(true);
            }

            HintChanged?.Invoke(message);

            if (hintDisplaySeconds > 0f)
            {
                hintCoroutine =
                    StartCoroutine(HideHintAfterDelay());
            }
        }

        private IEnumerator HideHintAfterDelay()
        {
            yield return new WaitForSecondsRealtime(
                hintDisplaySeconds
            );

            hintCoroutine = null;

            if (hintRoot != null)
            {
                hintRoot.SetActive(false);
            }

            HintChanged?.Invoke(string.Empty);
        }

        public void HideHint()
        {
            if (hintCoroutine != null)
            {
                StopCoroutine(hintCoroutine);
                hintCoroutine = null;
            }

            if (hintRoot != null)
            {
                hintRoot.SetActive(false);
            }

            HintChanged?.Invoke(string.Empty);
        }

        // =========================================================
        // Presentation
        // =========================================================

        private void PlayPresentation(
            string action,
            IngredientType type,
            string amount)
        {
            if (type == IngredientType.Cheese &&
                action == "Open" &&
                stateManager.GetState(type)?.currentStateId ==
                "CHEESE_PACK_OPENED_STACK")
            {
                StartCoroutine(OpenCheeseBox());
            }

            if (type == IngredientType.Mayonnaise &&
                action == "TakeOff")
            {
                StartCoroutine(PlayMayonnaise(amount));
            }
        }

        private IEnumerator OpenCheeseBox()
        {
            Transform top = FindTransform("Cheese_box_top");

            if (top == null)
            {
                yield break;
            }

            Quaternion from = top.localRotation;

            Quaternion to = Quaternion.Euler(
                top.localEulerAngles.x,
                top.localEulerAngles.y,
                -80f
            );

            for (float time = 0f;
                 time < cheeseBoxOpenSeconds;
                 time += Time.deltaTime)
            {
                top.localRotation = Quaternion.Slerp(
                    from,
                    to,
                    time / cheeseBoxOpenSeconds
                );

                yield return null;
            }

            top.localRotation = to;
        }

        // =========================================================
        // 마요네즈 짜기 연출
        // =========================================================

        private IEnumerator PlayMayonnaise(string amount)
        {
            // 마요네즈 연출용 병
            GameObject sourcingBottle =
                FindGameObject("MayoBottleSoucing");

            // 평상시에 보이는 마요네즈 병
            GameObject normalBottle =
                FindGameObject("MayoBottle");

            // Main Camera 찾기
            Transform mainCameraTransform = null;

            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }
            else
            {
                mainCameraTransform =
                    FindTransform("Main Camera");
            }

            // Main Camera의 자식 중 R_Hand 찾기
            GameObject rightHand = null;

            if (mainCameraTransform != null)
            {
                Transform[] cameraChildren =
                    mainCameraTransform
                        .GetComponentsInChildren<Transform>(true);

                foreach (Transform child in cameraChildren)
                {
                    if (child.name == "R_HAND")
                    {
                        rightHand = child.gameObject;
                        break;
                    }
                }
            }

            // -----------------------------------------------------
            // 마요네즈 짜기 연출 시작
            // -----------------------------------------------------

            // 평상시 마요네즈 병 숨기기
            if (normalBottle != null)
            {
                normalBottle.SetActive(false);
            }
            else
            {
                Debug.LogWarning(
                    "MayoBottle 오브젝트를 찾지 못했습니다."
                );
            }

            // 오른손 숨기기
            if (rightHand != null)
            {
                rightHand.SetActive(false);
            }
            else
            {
                Debug.LogWarning(
                    "Main Camera의 자식에서 " +
                    "R_Hand를 찾지 못했습니다."
                );
            }

            // 연출용 마요네즈 병 켜기
            if (sourcingBottle != null)
            {
                sourcingBottle.SetActive(true);

                Animator animator =
                    sourcingBottle.GetComponentInChildren<Animator>(
                        true
                    );

                if (animator != null)
                {
                    animator.Play(0, 0, 0f);
                }
                else
                {
                    Debug.LogWarning(
                        "MayoBottleSoucing에서 " +
                        "Animator를 찾지 못했습니다."
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    "MayoBottleSoucing 오브젝트를 " +
                    "찾지 못했습니다."
                );
            }

            // Mayo_tong의 마요네즈 Blend Shape 찾기
            SkinnedMeshRenderer mayo =
                FindBlendShapeRenderer(
                    sourcingBottle,
                    "Mayo Use",
                    out int index
                );

            if (mayo != null)
            {
                mayo.enabled = true;
            }
            else
            {
                Debug.LogWarning(
                    "MayoBottleSoucing에서 Mayo Use Blend Shape를 " +
                    "찾지 못했습니다."
                );
            }

            float target =
                amount == "L"
                    ? 100f
                    : amount == "S"
                        ? 35f
                        : 65f;

            float start =
                mayo != null
                    ? mayo.GetBlendShapeWeight(index)
                    : 0f;

            // 마요네즈가 짜지는 애니메이션
            for (float time = 0f;
                 time < mayonnaiseAnimationSeconds;
                 time += Time.deltaTime)
            {
                if (mayo != null)
                {
                    mayo.SetBlendShapeWeight(
                        index,
                        Mathf.Lerp(
                            start,
                            target,
                            time /
                            mayonnaiseAnimationSeconds
                        )
                    );
                }

                yield return null;
            }

            if (mayo != null)
            {
                mayo.SetBlendShapeWeight(index, target);
            }

            // -----------------------------------------------------
            // 마요네즈 짜기 연출 종료
            // -----------------------------------------------------

            // 연출용 마요네즈 병 숨기기
            if (sourcingBottle != null)
            {
                sourcingBottle.SetActive(false);
            }

            // 평상시 마요네즈 병 다시 켜기
            if (normalBottle != null)
            {
                normalBottle.SetActive(true);
            }

            // 오른손 다시 켜기
            if (rightHand != null)
            {
                rightHand.SetActive(true);
            }
        }

        // =========================================================
        // Object Search
        // =========================================================

        private static SkinnedMeshRenderer
            FindBlendShapeRenderer(
                GameObject root,
                string shapeName,
                out int index)
        {
            index = -1;

            if (root == null)
            {
                return null;
            }

            foreach (SkinnedMeshRenderer renderer
                     in root.GetComponentsInChildren
                         <SkinnedMeshRenderer>(true))
            {
                if (renderer.sharedMesh == null)
                {
                    continue;
                }

                for (int i = 0;
                     i < renderer.sharedMesh.blendShapeCount;
                     i++)
                {
                    if (renderer.sharedMesh
                            .GetBlendShapeName(i) == shapeName)
                    {
                        index = i;
                        return renderer;
                    }
                }
            }

            return null;
        }

        private static GameObject FindGameObject(
            string objectName)
        {
            foreach (Transform current
                     in FindObjectsOfType<Transform>(true))
            {
                if (current.name == objectName)
                {
                    return current.gameObject;
                }
            }

            return null;
        }

        private static Transform FindTransform(
            string objectName)
        {
            return FindGameObject(objectName)?.transform;
        }

        private void Report(
            string message,
            bool error)
        {
            if (error)
            {
                Debug.LogError(message);
            }
            else
            {
                Debug.Log(message);
            }

            StatusChanged?.Invoke(message);
        }

        private static readonly HashSet<string> Allowed =
            new HashSet<string>
            {
                "Open",
                "TakeOff",
                "Cut",
                "Put",
                "Finish"
            };
    }
}
