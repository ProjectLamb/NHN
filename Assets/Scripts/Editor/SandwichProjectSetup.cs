#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using SandwichGame.Actions;
using SandwichGame.AI;
using SandwichGame.Ingredients;
using SandwichGame.Sandwich;
using SandwichGame.StateMachine;
using SandwichGame.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SandwichGame.Editor
{
    [InitializeOnLoad]
    internal static class SandwichFirstImportSetup
    {
        static SandwichFirstImportSetup()
        {
            EditorApplication.delayCall += TryApply;
        }

        private static void TryApply()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += TryApply;
                return;
            }
            bool databaseMissing = AssetDatabase.LoadAssetAtPath<StateTransitionDatabase>("Assets/ScriptableObjects/StateTransitionDatabase.asset") == null;
            IngredientPrefabDatabase prefabDatabase = AssetDatabase.LoadAssetAtPath<IngredientPrefabDatabase>("Assets/ScriptableObjects/IngredientPrefabDatabase.asset");
            bool generatedCubesRemain = AssetDatabase.IsValidFolder("Assets/Prefabs/Generated");
            bool realIngredientMigrationNeeded = prefabDatabase == null || prefabDatabase.setupVersion < 3;
            if (!databaseMissing && !generatedCubesRemain && !realIngredientMigrationNeeded) return;
            string originalScene = SceneManager.GetActiveScene().path;
            try
            {
                EditorSceneManager.SaveOpenScenes();
                SandwichProjectSetup.ApplyAll();
                if (!string.IsNullOrEmpty(originalScene) && originalScene != "Assets/Scenes/GameScene.unity")
                    EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
                Debug.Log("Initial sandwich setup was applied automatically. Use Sandwich > Apply Complete Setup To GameScene to refresh it later.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    public static class SandwichProjectSetup
    {
        private const string DataFolder="Assets/ScriptableObjects";
        private const string GeneratedPrefabFolder="Assets/Prefabs/Generated";
        private const string GeneratedMaterialFolder="Assets/Design/GeneratedMaterials";

        [MenuItem("Sandwich/Apply Complete Setup To GameScene")]
        public static void ApplyAll()
        {
            EnsureFolder(DataFolder);
            RemoveGeneratedPlaceholders();
            StateTransitionDatabase transitions=LoadOrCreate<StateTransitionDatabase>($"{DataFolder}/StateTransitionDatabase.asset");
            transitions.transitions=DefaultStateTransitions.Build();EditorUtility.SetDirty(transitions);
            IngredientPrefabDatabase prefabs=LoadOrCreate<IngredientPrefabDatabase>($"{DataFolder}/IngredientPrefabDatabase.asset");
            RefreshRealAssetMappings(prefabs,transitions.transitions);prefabs.setupVersion=3;EditorUtility.SetDirty(prefabs);AssetDatabase.SaveAssets();
            Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity",OpenSceneMode.Single);
            SetupScene(scene,transitions,prefabs);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log($"Sandwich setup complete: {transitions.transitions.Count} transitions, {prefabs.entries.Count} prefab states.");
        }

        public static void ApplyAllBatch(){ApplyAll();}

        private static void SetupScene(Scene scene,StateTransitionDatabase transitions,IngredientPrefabDatabase prefabs)
        {
            TMP_InputField input=FindSceneObject<TMP_InputField>(scene);
            Button button=FindSceneObject<Button>(scene);
            if(input==null||button==null)throw new InvalidOperationException("GameScene의 TMP_InputField 또는 Button을 찾지 못했습니다.");
            GameObject root=GameObject.Find("SandwichGameSystem")??new GameObject("SandwichGameSystem");
            IngredientStateManager stateManager=GetOrAdd<IngredientStateManager>(root);SandwichManager sandwichManager=GetOrAdd<SandwichManager>(root);SandwichValidator validator=GetOrAdd<SandwichValidator>(root);ActionExecutor executor=GetOrAdd<ActionExecutor>(root);AICommandManager ai=GetOrAdd<AICommandManager>(root);CommandInputUI ui=GetOrAdd<CommandInputUI>(root);

            Transform slots=GetOrCreateChild(root.transform,"IngredientSlots");IngredientView[] views=new IngredientView[6];
            for(int i=0;i<views.Length;i++){IngredientType type=(IngredientType)i;Transform slot=GetOrCreateChild(slots,type+"Slot");slot.position=new Vector3(-5f+i*2f,1.25f,0f);IngredientView view=GetOrAdd<IngredientView>(slot.gameObject);view.Configure(type);views[i]=view;EditorUtility.SetDirty(view);}
            Transform layerRoot=GetOrCreateChild(root.transform,"SandwichLayerRoot");GameObject plate=FindNamedSceneObject(scene,"Plate");if(plate!=null)layerRoot.position=plate.transform.position;
            TMP_Text status=GetOrCreateStatus(input);
            stateManager.Configure(transitions,prefabs,views);sandwichManager.Configure(layerRoot);validator.Configure(sandwichManager);executor.Configure(stateManager,sandwichManager,validator);ai.Configure(stateManager,executor);ui.Configure(input,status,button,ai,executor);
            TextAsset recipeJson=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Design/UI/MENU/SandwichRecipes.json");
            Sprite[] recipeImages=new Sprite[5];for(int i=0;i<recipeImages.Length;i++)recipeImages[i]=AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Design/UI/MENU/Recipe{i+1}.png");
            ui.ConfigureRecipes(recipeJson,recipeImages);
            EditorUtility.SetDirty(stateManager);EditorUtility.SetDirty(sandwichManager);EditorUtility.SetDirty(validator);EditorUtility.SetDirty(executor);EditorUtility.SetDirty(ai);EditorUtility.SetDirty(ui);
            bool already=false;for(int i=0;i<button.onClick.GetPersistentEventCount();i++)if(button.onClick.GetPersistentTarget(i)==ui)already=true;if(!already)UnityEventTools.AddPersistentListener(button.onClick,ui.Submit);
            Text legacyLabel=button.GetComponentInChildren<Text>(true);if(legacyLabel!=null){legacyLabel.text="명령 (0/6)";EditorUtility.SetDirty(legacyLabel);} input.placeholder.GetComponent<TMP_Text>().text="샌드위치 명령을 입력하세요";
            Selection.activeGameObject=root;
        }

        private static TMP_Text GetOrCreateStatus(TMP_InputField input)
        {
            Transform canvas=input.GetComponentInParent<Canvas>().transform;Transform existing=canvas.Find("SandwichStatusText");
            TextMeshProUGUI text=existing!=null?existing.GetComponent<TextMeshProUGUI>():null;
            if(text==null){GameObject go=new GameObject("SandwichStatusText",typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));go.transform.SetParent(canvas,false);text=go.GetComponent<TextMeshProUGUI>();RectTransform r=text.rectTransform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(0,-385);r.sizeDelta=new Vector2(1150,55);text.alignment=TextAlignmentOptions.Center;text.fontSize=25;text.color=new Color(.25f,1f,.42f);}
            text.text="게임을 시작하면 오늘의 샌드위치 조합이 제시됩니다.";EditorUtility.SetDirty(text);return text;
        }

        private static void RefreshRealAssetMappings(IngredientPrefabDatabase database,List<StateTransitionData> transitions)
        {
            Dictionary<string,GameObject> old=new Dictionary<string,GameObject>();foreach(IngredientPrefabEntry e in database.entries)if(e!=null&&!string.IsNullOrEmpty(e.stateId)&&e.prefab!=null&&!AssetDatabase.GetAssetPath(e.prefab).StartsWith(GeneratedPrefabFolder,StringComparison.Ordinal))old[e.stateId]=e.prefab;
            SortedSet<string> ids=new SortedSet<string>();foreach(StateTransitionData t in transitions){ids.Add(t.sourceStateId);ids.Add(t.resultStateId);}ids.Add("TOMATO_SLICE_STACK");ids.Add("CABBAGE_PIECE");database.entries.Clear();
            GameObject cabbageWhole=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/양배추/양배추_한통.fbx");
            GameObject cabbageHalf=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/양배추/양배추_반통.fbx");
            GameObject cabbagePiece=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/양배추/양배추_조각.fbx");
            GameObject cabbageShredded=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/양배추/양배추_채.fbx");
            GameObject tomato=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/토마토/TOMATO/260805_tomato.fbx");
            GameObject wrappedBreadHalf=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bread_Bag_BASEHalf.prefab");
            GameObject wrappedBreadPiece=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bread_Bag_Slice.prefab");
            GameObject breadWhole=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bread_BASE.prefab");
            GameObject breadHalf=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bread_BASEHalf.prefab");
            GameObject breadPiece=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bread_Piece.prefab");
            foreach(string id in ids){GameObject prefab=null;if(id=="CABBAGE_PIECE")prefab=cabbageWhole;else if(id=="L_CUT_CABBAGE_PIECE")prefab=cabbageHalf;else if(id=="M_CUT_CABBAGE_PIECE")prefab=cabbagePiece;else if(id=="S_CUT_CABBAGE_PIECE")prefab=cabbageShredded;else if(id=="L_CUT_BREAD_BAG_CLOSED_LOAF")prefab=wrappedBreadHalf;else if(id=="M_CUT_BREAD_BAG_CLOSED_LOAF"||id=="S_CUT_BREAD_BAG_CLOSED_LOAF")prefab=wrappedBreadPiece;else if(id=="BREAD_LOAF")prefab=breadWhole;else if(id=="L_CUT_BREAD_LOAF")prefab=breadHalf;else if(id=="M_CUT_BREAD_LOAF"||id=="S_CUT_BREAD_LOAF")prefab=breadPiece;else if(id.Contains("TOMATO"))prefab=tomato;else old.TryGetValue(id,out prefab);database.entries.Add(new IngredientPrefabEntry{stateId=id,prefab=prefab});}
        }

        private static void RemoveGeneratedPlaceholders(){if(AssetDatabase.IsValidFolder(GeneratedPrefabFolder))AssetDatabase.DeleteAsset(GeneratedPrefabFolder);if(AssetDatabase.IsValidFolder(GeneratedMaterialFolder))AssetDatabase.DeleteAsset(GeneratedMaterialFolder);}

        private static T FindSceneObject<T>(Scene scene)where T:Component{foreach(T item in UnityEngine.Object.FindObjectsOfType<T>(true))if(item.gameObject.scene==scene)return item;return null;}
        private static GameObject FindNamedSceneObject(Scene scene,string objectName){foreach(GameObject item in UnityEngine.Object.FindObjectsOfType<GameObject>(true))if(item.scene==scene&&item.name==objectName)return item;return null;}
        private static T GetOrAdd<T>(GameObject go)where T:Component=>go.GetComponent<T>()??go.AddComponent<T>();
        private static Transform GetOrCreateChild(Transform parent,string name){Transform child=parent.Find(name);if(child!=null)return child;GameObject go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
        private static T LoadOrCreate<T>(string path)where T:ScriptableObject{T asset=AssetDatabase.LoadAssetAtPath<T>(path);if(asset!=null)return asset;asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);return asset;}
        private static void EnsureFolder(string path){string[] parts=path.Split('/');string current=parts[0];for(int i=1;i<parts.Length;i++){string next=current+"/"+parts[i];if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(current,parts[i]);current=next;}}
    }
}
#endif
