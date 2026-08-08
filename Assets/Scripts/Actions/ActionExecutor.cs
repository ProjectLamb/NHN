using System;
using System.Collections;
using System.Collections.Generic;
using SandwichGame.AI;
using SandwichGame.Ingredients;
using SandwichGame.Sandwich;
using UnityEngine;

namespace SandwichGame.Actions
{
    public class ActionExecutor : MonoBehaviour
    {
        [SerializeField] private IngredientStateManager stateManager;
        [SerializeField] private SandwichManager sandwichManager;
        [SerializeField] private SandwichValidator validator;
        [SerializeField] private bool continueAfterFailedAction = true;
        [Header("Presentation")]
        [SerializeField] private float cheeseBoxOpenSeconds = .45f;
        [SerializeField] private float mayonnaiseAnimationSeconds = 1.2f;

        public event Action<string> StatusChanged;
        public event Action<string> HintChanged;

        public void Configure(IngredientStateManager state, SandwichManager sandwich, SandwichValidator sandwichValidator)
        { stateManager = state; sandwichManager = sandwich; validator = sandwichValidator; }

        public void Execute(SandwichActionData[] actions)
        {
            if (actions == null || actions.Length == 0)
            { Report("실행할 동작이 없습니다.", true); HintChanged?.Invoke("힌트: 재료와 행동을 함께 말해주세요. 예) 양배추를 잘라서 올려줘"); return; }
            HintChanged?.Invoke(string.Empty);
            foreach (SandwichActionData action in actions)
            {
                bool ok = TryExecute(action, out string message);
                Report(message, !ok);
                if (!ok) HintChanged?.Invoke(BuildHint(action));
                if (!ok && !continueAfterFailedAction) break;
            }
        }

        private bool TryExecute(SandwichActionData action, out string message)
        {
            message = "잘못된 action입니다.";
            if (action == null || !Allowed.Contains(action.action)) return false;
            if (action.action == "Finish") return FinishActionHandler.Execute(validator, out message);
            if (!Enum.TryParse(action.targetIngredient, true, out IngredientType type)) { message = $"잘못된 재료: {action.targetIngredient}"; return false; }

            bool ok;
            switch (action.action)
            {
                case "Open": ok = OpenActionHandler.Execute(stateManager, type, out message); break;
                case "TakeOff": ok = TakeOffActionHandler.Execute(stateManager, type, action.amount, out message); break;
                case "Cut": ok = CutActionHandler.Execute(stateManager, type, action.amount, out message); break;
                case "Put": ok = PutActionHandler.Execute(stateManager, sandwichManager, type, out message); break;
                default: return false;
            }

            if (ok && action.action != "Put" && stateManager.IsReadyToPut(type, out _) &&
                stateManager.TryGetCurrentPrefab(type, out GameObject prefab, out string stateId))
                ok = sandwichManager.ShowPreparedIngredient(type, stateId, prefab, out message);
            if (ok) { PlayPresentation(action.action, type, action.amount); message = $"{action.action} {action.targetIngredient} 완료"; }
            return ok;
        }

        private string BuildHint(SandwichActionData action)
        {
            if (action == null || !Enum.TryParse(action.targetIngredient, true, out IngredientType type))
                return "힌트: 빵, 양배추, 토마토, 햄, 치즈, 마요네즈 중 재료를 정확히 말해주세요.";
            string state = stateManager.GetState(type)?.currentStateId ?? string.Empty;
            switch (type)
            {
                case IngredientType.Bread: return state == "BREAD_BAG_OPENED_LOAF" ? "힌트: 빵을 원하는 크기로 썰어서 올려주세요." : "힌트: 빵은 비닐을 벗기고 원하는 크기로 썰어서 올려주세요.";
                case IngredientType.Cabbage: return "힌트: 양배추는 원하는 크기로 잘라서 올려주세요.";
                case IngredientType.Tomato: return "힌트: 토마토는 꺼내서 올려주세요.";
                case IngredientType.Ham: return "힌트: 햄은 꺼내서 올려주세요.";
                case IngredientType.Cheese:
                    if (state == "CHEESE_PACK_OPENED_STACK") return "힌트: 열린 치즈 상자에서 치즈를 꺼내고 비닐을 벗겨서 올려주세요.";
                    if (state.Contains("TAKEOFF_CHEESE_SLICE_WRAPPED")) return "힌트: 꺼낸 치즈의 비닐을 벗겨서 올려주세요.";
                    return "힌트: 치즈 상자를 열고 치즈를 꺼낸 뒤 비닐을 벗겨서 올려주세요.";
                case IngredientType.Mayonnaise: return "힌트: 마요네즈는 조금, 적당히 또는 많이 짜서 올려주세요.";
                default: return "힌트: 재료를 손질한 다음 올려주세요.";
            }
        }

        private void PlayPresentation(string action, IngredientType type, string amount)
        {
            if (type == IngredientType.Cheese && action == "Open" && stateManager.GetState(type)?.currentStateId == "CHEESE_PACK_OPENED_STACK") StartCoroutine(OpenCheeseBox());
            if (type == IngredientType.Mayonnaise && action == "TakeOff") StartCoroutine(PlayMayonnaise(amount));
        }

        private IEnumerator OpenCheeseBox()
        {
            Transform top = FindTransform("Cheese_box_top"); if (top == null) yield break;
            Quaternion from = top.localRotation; Quaternion to = Quaternion.Euler(top.localEulerAngles.x, top.localEulerAngles.y, -80f);
            for (float time = 0; time < cheeseBoxOpenSeconds; time += Time.deltaTime) { top.localRotation = Quaternion.Slerp(from, to, time / cheeseBoxOpenSeconds); yield return null; }
            top.localRotation = to;
        }

        private IEnumerator PlayMayonnaise(string amount)
        {
            GameObject bottle = FindGameObject("MayoBottleSoucing") ?? FindGameObject("MayoBottle");
            if (bottle != null) { bottle.SetActive(true); Animator animator = bottle.GetComponentInChildren<Animator>(true); if (animator != null) animator.Play(0, 0, 0f); }
            SkinnedMeshRenderer mayo = FindBlendShapeRenderer("Mayo_tong", "Mayo Use", out int index);
            float target = amount == "L" ? 100f : amount == "S" ? 35f : 65f; float start = mayo != null ? mayo.GetBlendShapeWeight(index) : 0f;
            for (float time = 0; time < mayonnaiseAnimationSeconds; time += Time.deltaTime) { if (mayo != null) mayo.SetBlendShapeWeight(index, Mathf.Lerp(start, target, time / mayonnaiseAnimationSeconds)); yield return null; }
            if (mayo != null) mayo.SetBlendShapeWeight(index, target); if (bottle != null) bottle.SetActive(false);
        }

        private static SkinnedMeshRenderer FindBlendShapeRenderer(string rootName, string shapeName, out int index)
        { index = -1; GameObject root = FindGameObject(rootName); if (root == null) return null; foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)) for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++) if (renderer.sharedMesh.GetBlendShapeName(i) == shapeName) { index = i; return renderer; } return null; }
        private static GameObject FindGameObject(string name) { foreach (Transform transform in FindObjectsOfType<Transform>(true)) if (transform.name == name) return transform.gameObject; return null; }
        private static Transform FindTransform(string name) => FindGameObject(name)?.transform;
        private void Report(string message, bool error) { if (error) Debug.LogError(message); else Debug.Log(message); StatusChanged?.Invoke(message); }
        private static readonly HashSet<string> Allowed = new HashSet<string> { "Open", "TakeOff", "Cut", "Put", "Finish" };
    }
}
