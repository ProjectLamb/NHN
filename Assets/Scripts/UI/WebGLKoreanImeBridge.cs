using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace SandwichGame.UI
{
    /// <summary>
    /// Routes browser IME composition through an HTML input element on WebGL.
    /// Unity's native WebGL keyboard path does not reliably deliver Korean IME text.
    /// </summary>
    public sealed class WebGLKoreanImeBridge : MonoBehaviour, ISelectHandler, IPointerClickHandler
    {
        private TMP_InputField input;
        private string receiverName;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void SandwichImeCreate(string receiver);
        [DllImport("__Internal")] private static extern void SandwichImeFocus(string receiver, string value);
        [DllImport("__Internal")] private static extern void SandwichImeSetValue(string value);
        [DllImport("__Internal")] private static extern void SandwichImeDestroy(string receiver);
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachToInputFields();
#endif
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AttachToInputFields();
        }

        private static void AttachToInputFields()
        {
            TMP_InputField[] fields = FindObjectsByType<TMP_InputField>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (TMP_InputField field in fields)
            {
                if (field.GetComponent<WebGLKoreanImeBridge>() == null)
                {
                    field.gameObject.AddComponent<WebGLKoreanImeBridge>();
                }
            }
        }

        private void Awake()
        {
            input = GetComponent<TMP_InputField>();
            receiverName = gameObject.name;

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;
            SandwichImeCreate(receiverName);
#endif
        }

        public void OnSelect(BaseEventData eventData)
        {
            FocusBrowserInput();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            FocusBrowserInput();
        }

        private void FocusBrowserInput()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SandwichImeFocus(receiverName, input != null ? input.text : string.Empty);
#endif
        }

        // Called from WebGLKoreanIme.jslib via SendMessage.
        public void OnWebGLImeInput(string value)
        {
            if (input == null || !input.interactable)
                return;

            input.text = value ?? string.Empty;
            input.caretPosition = input.text.Length;
            input.selectionAnchorPosition = input.caretPosition;
            input.selectionFocusPosition = input.caretPosition;
        }

        // Called from WebGLKoreanIme.jslib when Enter is pressed outside composition.
        public void OnWebGLImeSubmit(string unused)
        {
            if (input == null || !input.interactable)
                return;

            input.onSubmit.Invoke(input.text);
            StartCoroutine(SyncAfterSubmit());
        }

        private IEnumerator SyncAfterSubmit()
        {
            yield return null;
#if UNITY_WEBGL && !UNITY_EDITOR
            SandwichImeSetValue(input != null ? input.text : string.Empty);
            FocusBrowserInput();
#endif
        }

        private void OnDestroy()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SandwichImeDestroy(receiverName);
#endif
        }
    }
}
