//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.UI;

//public class TestFoodButtonSpawner : MonoBehaviour
//{
//    // =========================================================
//    // 프리팹 배치 기준
//    // =========================================================

//    public enum PlacementReference
//    {
//        Pivot,
//        Center
//    }

//    [Serializable]
//    public class FoodButtonData
//    {
//        [Tooltip("Inspector에서 구분하기 위한 이름")]
//        public string foodName;

//        [Tooltip("이 재료를 생성할 UI 버튼")]
//        public Button button;

//        [Tooltip("버튼을 눌렀을 때 생성할 재료 프리팹")]
//        public GameObject prefab;

//        [Tooltip("프리팹 배치 기준")]
//        public PlacementReference placementReference;
//    }

//    // =========================================================
//    // Inspector 설정
//    // =========================================================

//    [Header("First Placement")]
//    [Tooltip("첫 재료가 생성될 SandwichWillPlaceHere 오브젝트")]
//    [SerializeField]
//    private Transform sandwichWillPlaceHere;

//    [Tooltip("SandwichWillPlaceHere 큐브의 윗면을 생성 위치로 사용")]
//    [SerializeField]
//    private bool useCubeTopAsFirstPosition = true;

//    [Header("Food Buttons")]
//    [Tooltip("17개 재료 버튼 설정")]
//    [SerializeField]
//    private List<FoodButtonData> foodButtons =
//        new List<FoodButtonData>();

//    [Header("Optional")]
//    [Tooltip("생성된 재료를 모두 지우는 버튼")]
//    [SerializeField]
//    private Button clearButton;

//    [Tooltip("생성된 재료를 정리해 둘 부모 오브젝트. 없어도 됨")]
//    [SerializeField]
//    private Transform spawnedFoodParent;

//    // =========================================================
//    // 내부 상태
//    // =========================================================

//    private readonly List<GameObject> spawnedFoods =
//        new List<GameObject>();

//    private readonly List<Button> boundButtons =
//        new List<Button>();

//    private readonly List<UnityAction> boundActions =
//        new List<UnityAction>();

//    private Transform nextFoodPlaceHere;
//    private bool hasSpawnedFood;

//    // =========================================================
//    // Unity 생명주기
//    // =========================================================

//    private void Awake()
//    {
//        ConnectFoodButtons();

//        if (clearButton != null)
//            clearButton.onClick.AddListener(ClearAllFoods);
//    }

//    private void OnDestroy()
//    {
//        for (int i = 0; i < boundButtons.Count; i++)
//        {
//            if (boundButtons[i] != null)
//            {
//                boundButtons[i].onClick.RemoveListener(
//                    boundActions[i]
//                );
//            }
//        }

//        if (clearButton != null)
//            clearButton.onClick.RemoveListener(ClearAllFoods);
//    }

//    // =========================================================
//    // 버튼 자동 연결
//    // =========================================================

//    private void ConnectFoodButtons()
//    {
//        boundButtons.Clear();
//        boundActions.Clear();

//        for (int i = 0; i < foodButtons.Count; i++)
//        {
//            FoodButtonData data = foodButtons[i];

//            if (data == null || data.button == null)
//                continue;

//            int capturedIndex = i;

//            UnityAction action =
//                () => SpawnFood(capturedIndex);

//            data.button.onClick.AddListener(action);

//            boundButtons.Add(data.button);
//            boundActions.Add(action);
//        }
//    }

//    // =========================================================
//    // 재료 생성
//    // =========================================================

//    public void SpawnFood(int foodIndex)
//    {
//        if (foodIndex < 0 || foodIndex >= foodButtons.Count)
//        {
//            Debug.LogError(
//                $"잘못된 재료 번호입니다: {foodIndex}"
//            );

//            return;
//        }

//        FoodButtonData data = foodButtons[foodIndex];

//        if (data.prefab == null)
//        {
//            Debug.LogError(
//                $"[{data.foodName}] 프리팹이 연결되지 않았습니다."
//            );

//            return;
//        }

//        Transform placementTarget = ResolvePlacementTarget();

//        if (placementTarget == null)
//            return;

//        Vector3 spawnPosition;

//        // 첫 번째 재료라면 큐브 윗면 사용 가능
//        if (!hasSpawnedFood &&
//            useCubeTopAsFirstPosition)
//        {
//            spawnPosition =
//                GetObjectTopPosition(sandwichWillPlaceHere);
//        }
//        else
//        {
//            spawnPosition = placementTarget.position;
//        }

//        GameObject spawnedFood = Instantiate(
//            data.prefab,
//            spawnPosition,
//            placementTarget.rotation,
//            spawnedFoodParent
//        );

//        spawnedFood.name = data.prefab.name;

//        // Center 기준 프리팹은 실제 렌더러 중심을
//        // 생성 위치에 맞춘다.
//        if (data.placementReference ==
//            PlacementReference.Center)
//        {
//            AlignObjectCenter(
//                spawnedFood,
//                spawnPosition
//            );
//        }

//        Transform foundNextPoint =
//            FindChildByName(
//                spawnedFood.transform,
//                "NextFoodPlaceHere"
//            );

//        if (foundNextPoint == null)
//        {
//            Debug.LogError(
//                $"[{spawnedFood.name}] 안에서 " +
//                "NextFoodPlaceHere를 찾지 못했습니다.\n" +
//                "다음 재료는 생성할 수 없습니다."
//            );
//        }

//        spawnedFoods.Add(spawnedFood);

//        nextFoodPlaceHere = foundNextPoint;
//        hasSpawnedFood = true;

//        Debug.Log(
//            $"재료 생성 완료: {data.foodName} / " +
//            $"기준: {data.placementReference}"
//        );
//    }

//    // =========================================================
//    // 현재 생성 위치 결정
//    // =========================================================

//    private Transform ResolvePlacementTarget()
//    {
//        // 첫 번째 재료
//        if (!hasSpawnedFood)
//        {
//            if (sandwichWillPlaceHere == null)
//            {
//                Debug.LogError(
//                    "SandwichWillPlaceHere가 연결되지 않았습니다."
//                );
//            }

//            return sandwichWillPlaceHere;
//        }

//        // 두 번째 이후 재료
//        if (nextFoodPlaceHere == null)
//        {
//            Debug.LogError(
//                "직전 재료에서 NextFoodPlaceHere를 " +
//                "찾지 못해 다음 재료를 생성할 수 없습니다."
//            );

//            return null;
//        }

//        return nextFoodPlaceHere;
//    }

//    // =========================================================
//    // Center 기준 위치 보정
//    // =========================================================

//    private void AlignObjectCenter(
//        GameObject targetObject,
//        Vector3 targetPosition)
//    {
//        if (!TryGetCombinedBounds(
//                targetObject,
//                out Bounds bounds))
//        {
//            Debug.LogWarning(
//                $"[{targetObject.name}]의 Renderer 또는 " +
//                "Collider를 찾지 못해 Pivot 기준으로 생성했습니다."
//            );

//            return;
//        }

//        Vector3 offset =
//            targetPosition - bounds.center;

//        targetObject.transform.position += offset;
//    }

//    private bool TryGetCombinedBounds(
//        GameObject targetObject,
//        out Bounds combinedBounds)
//    {
//        Renderer[] renderers =
//            targetObject.GetComponentsInChildren<Renderer>(true);

//        bool foundBounds = false;
//        combinedBounds = new Bounds();

//        foreach (Renderer currentRenderer in renderers)
//        {
//            if (!foundBounds)
//            {
//                combinedBounds = currentRenderer.bounds;
//                foundBounds = true;
//            }
//            else
//            {
//                combinedBounds.Encapsulate(
//                    currentRenderer.bounds
//                );
//            }
//        }

//        if (foundBounds)
//            return true;

//        // Renderer가 없다면 Collider 기준 사용
//        Collider[] colliders =
//            targetObject.GetComponentsInChildren<Collider>(true);

//        foreach (Collider currentCollider in colliders)
//        {
//            if (!foundBounds)
//            {
//                combinedBounds = currentCollider.bounds;
//                foundBounds = true;
//            }
//            else
//            {
//                combinedBounds.Encapsulate(
//                    currentCollider.bounds
//                );
//            }
//        }

//        return foundBounds;
//    }

//    // =========================================================
//    // 첫 생성 큐브의 윗면 위치
//    // =========================================================

//    private Vector3 GetObjectTopPosition(
//        Transform target)
//    {
//        if (target == null)
//            return Vector3.zero;

//        Collider targetCollider =
//            target.GetComponent<Collider>();

//        if (targetCollider != null)
//        {
//            Bounds bounds = targetCollider.bounds;

//            return new Vector3(
//                bounds.center.x,
//                bounds.max.y,
//                bounds.center.z
//            );
//        }

//        Renderer targetRenderer =
//            target.GetComponent<Renderer>();

//        if (targetRenderer != null)
//        {
//            Bounds bounds = targetRenderer.bounds;

//            return new Vector3(
//                bounds.center.x,
//                bounds.max.y,
//                bounds.center.z
//            );
//        }

//        // Collider와 Renderer가 없다면 Transform 위치 사용
//        return target.position;
//    }

//    // =========================================================
//    // 이름으로 하위 오브젝트 탐색
//    // =========================================================

//    private Transform FindChildByName(
//        Transform root,
//        string targetName)
//    {
//        Transform[] children =
//            root.GetComponentsInChildren<Transform>(true);

//        foreach (Transform child in children)
//        {
//            if (child.name == targetName)
//                return child;
//        }

//        return null;
//    }

//    // =========================================================
//    // 생성된 재료 전체 삭제
//    // =========================================================

//    public void ClearAllFoods()
//    {
//        foreach (GameObject food in spawnedFoods)
//        {
//            if (food != null)
//                Destroy(food);
//        }

//        spawnedFoods.Clear();

//        nextFoodPlaceHere = null;
//        hasSpawnedFood = false;

//        Debug.Log("생성된 재료를 모두 삭제했습니다.");
//    }
//}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TestFoodButtonSpawner : MonoBehaviour
{
    // =========================================================
    // 재료 버튼 데이터
    // =========================================================

    [Serializable]
    public class FoodButtonData
    {
        [Tooltip("Inspector에서 구분하기 위한 재료 이름")]
        public string foodName;

        [Tooltip("이 재료를 생성할 UI 버튼")]
        public Button button;

        [Tooltip("버튼을 눌렀을 때 생성할 프리팹")]
        public GameObject prefab;
    }

    // =========================================================
    // Inspector 설정
    // =========================================================

    [Header("First Placement")]

    [Tooltip("첫 재료가 생성될 SandwichWillPlaceHere 큐브")]
    [SerializeField]
    private Transform sandwichWillPlaceHere;

    [Header("Food Buttons")]

    [Tooltip("17개 재료 버튼과 프리팹을 등록")]
    [SerializeField]
    private List<FoodButtonData> foodButtons =
        new List<FoodButtonData>();

    [Header("Optional")]

    [Tooltip("생성된 재료를 모두 삭제하는 버튼")]
    [SerializeField]
    private Button clearButton;

    [Tooltip("생성된 재료를 정리할 부모 오브젝트. 비워도 됨")]
    [SerializeField]
    private Transform spawnedFoodParent;

    [Tooltip("재료 사이에 추가할 높이 간격")]
    [SerializeField]
    private float verticalGap = 0f;

    // =========================================================
    // 내부 상태
    // =========================================================

    private readonly List<GameObject> spawnedFoods =
        new List<GameObject>();

    private readonly List<Button> boundButtons =
        new List<Button>();

    private readonly List<UnityAction> boundActions =
        new List<UnityAction>();

    // 다음 음식의 아랫면이 놓일 월드 좌표
    private Vector3 currentStackPoint;

    private bool stackPointInitialized;

    // =========================================================
    // Unity 생명주기
    // =========================================================

    private void Awake()
    {
        ConnectFoodButtons();

        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearAllFoods);
        }
    }

    private void Start()
    {
        ResetStackPoint();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < boundButtons.Count; i++)
        {
            if (boundButtons[i] != null &&
                boundActions[i] != null)
            {
                boundButtons[i].onClick.RemoveListener(
                    boundActions[i]
                );
            }
        }

        if (clearButton != null)
        {
            clearButton.onClick.RemoveListener(ClearAllFoods);
        }
    }

    // =========================================================
    // 버튼 자동 연결
    // =========================================================

    private void ConnectFoodButtons()
    {
        boundButtons.Clear();
        boundActions.Clear();

        for (int i = 0; i < foodButtons.Count; i++)
        {
            FoodButtonData data = foodButtons[i];

            if (data == null || data.button == null)
                continue;

            int capturedIndex = i;

            UnityAction action =
                () => SpawnFood(capturedIndex);

            data.button.onClick.AddListener(action);

            boundButtons.Add(data.button);
            boundActions.Add(action);
        }
    }

    // =========================================================
    // 재료 생성
    // =========================================================

    public void SpawnFood(int foodIndex)
    {
        if (foodIndex < 0 ||
            foodIndex >= foodButtons.Count)
        {
            Debug.LogError(
                $"잘못된 재료 번호입니다: {foodIndex}"
            );

            return;
        }

        FoodButtonData data = foodButtons[foodIndex];

        if (data == null || data.prefab == null)
        {
            Debug.LogError(
                $"Food Buttons의 {foodIndex}번 프리팹이 " +
                "연결되지 않았습니다."
            );

            return;
        }

        if (!stackPointInitialized)
        {
            if (!ResetStackPoint())
                return;
        }

        GameObject spawnedFood = Instantiate(
            data.prefab,
            Vector3.zero,
            data.prefab.transform.rotation,
            spawnedFoodParent
        );

        spawnedFood.name = data.prefab.name;

        // 생성된 실제 모델의 전체 Bounds 계산
        if (!TryGetCombinedBounds(
                spawnedFood,
                out Bounds foodBounds))
        {
            Debug.LogError(
                $"[{data.prefab.name}] 안에서 활성화된 " +
                "Renderer 또는 Collider를 찾지 못했습니다."
            );

            Destroy(spawnedFood);
            return;
        }

        // 현재 음식의 실제 아랫면 중앙
        Vector3 foodBottomCenter = new Vector3(
            foodBounds.center.x,
            foodBounds.min.y,
            foodBounds.center.z
        );

        // 음식의 아랫면 중앙을 현재 쌓기 위치에 맞춤
        Vector3 positionOffset =
            currentStackPoint - foodBottomCenter;

        spawnedFood.transform.position += positionOffset;

        // 이동한 뒤 Bounds를 다시 계산
        if (!TryGetCombinedBounds(
                spawnedFood,
                out foodBounds))
        {
            Debug.LogError(
                $"[{spawnedFood.name}] 이동 후 Bounds 계산에 " +
                "실패했습니다."
            );

            Destroy(spawnedFood);
            return;
        }

        // 배치된 음식의 실제 윗면 중앙을
        // 다음 음식이 생성될 위치로 지정
        currentStackPoint = new Vector3(
            foodBounds.center.x,
            foodBounds.max.y + verticalGap,
            foodBounds.center.z
        );

        spawnedFoods.Add(spawnedFood);

        Debug.Log(
            $"재료 생성 완료: {data.foodName}\n" +
            $"다음 생성 위치: {currentStackPoint}"
        );
    }

    // =========================================================
    // 첫 생성 위치 계산
    // =========================================================

    private bool ResetStackPoint()
    {
        if (sandwichWillPlaceHere == null)
        {
            stackPointInitialized = false;

            Debug.LogError(
                "Sandwich Will Place Here에 " +
                "SandwichWillPlaceHere 큐브를 연결해주세요."
            );

            return false;
        }

        // 큐브의 Collider가 있으면 Collider 윗면 사용
        Collider targetCollider =
            sandwichWillPlaceHere.GetComponent<Collider>();

        if (targetCollider != null &&
            targetCollider.enabled)
        {
            Bounds bounds = targetCollider.bounds;

            currentStackPoint = new Vector3(
                bounds.center.x,
                bounds.max.y + verticalGap,
                bounds.center.z
            );

            stackPointInitialized = true;
            return true;
        }

        // Collider가 없으면 Renderer 윗면 사용
        Renderer targetRenderer =
            sandwichWillPlaceHere.GetComponent<Renderer>();

        if (targetRenderer != null &&
            targetRenderer.enabled)
        {
            Bounds bounds = targetRenderer.bounds;

            currentStackPoint = new Vector3(
                bounds.center.x,
                bounds.max.y + verticalGap,
                bounds.center.z
            );

            stackPointInitialized = true;
            return true;
        }

        // 아무것도 없으면 Transform 위치 사용
        currentStackPoint =
            sandwichWillPlaceHere.position +
            Vector3.up * verticalGap;

        stackPointInitialized = true;

        Debug.LogWarning(
            "SandwichWillPlaceHere에 Collider와 Renderer가 없어 " +
            "Transform 위치를 첫 생성 위치로 사용합니다."
        );

        return true;
    }

    // =========================================================
    // 음식의 실제 전체 크기 계산
    // =========================================================

    private bool TryGetCombinedBounds(
        GameObject targetObject,
        out Bounds combinedBounds)
    {
        combinedBounds = new Bounds();

        bool foundBounds = false;

        Renderer[] renderers =
            targetObject.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer currentRenderer in renderers)
        {
            if (currentRenderer == null)
                continue;

            if (!currentRenderer.enabled)
                continue;

            if (!currentRenderer.gameObject.activeInHierarchy)
                continue;

            // 파티클 효과는 음식 크기 계산에서 제외
            if (currentRenderer is ParticleSystemRenderer)
                continue;

            if (!foundBounds)
            {
                combinedBounds = currentRenderer.bounds;
                foundBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(
                    currentRenderer.bounds
                );
            }
        }

        // 활성화된 Renderer를 찾았다면 해당 Bounds 사용
        if (foundBounds)
            return true;

        // Renderer가 없을 때 Collider로 계산
        Collider[] colliders =
            targetObject.GetComponentsInChildren<Collider>(true);

        foreach (Collider currentCollider in colliders)
        {
            if (currentCollider == null)
                continue;

            if (!currentCollider.enabled)
                continue;

            if (!currentCollider.gameObject.activeInHierarchy)
                continue;

            if (!foundBounds)
            {
                combinedBounds = currentCollider.bounds;
                foundBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(
                    currentCollider.bounds
                );
            }
        }

        return foundBounds;
    }

    // =========================================================
    // 생성된 음식 전체 삭제
    // =========================================================

    public void ClearAllFoods()
    {
        foreach (GameObject food in spawnedFoods)
        {
            if (food != null)
            {
                Destroy(food);
            }
        }

        spawnedFoods.Clear();

        stackPointInitialized = false;
        ResetStackPoint();

        Debug.Log(
            "생성된 재료를 모두 삭제하고 " +
            "쌓기 위치를 초기화했습니다."
        );
    }
}