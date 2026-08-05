using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Text))]
public class LegacyTextBlink : MonoBehaviour
{
    [SerializeField, Min(0.05f)]
    [Tooltip("텍스트가 켜짐/꺼짐 상태를 바꾸는 간격(초)")]
    private float blinkInterval = 0.5f;

    [SerializeField]
    [Tooltip("오브젝트가 활성화될 때 텍스트를 보이는 상태로 시작할지 결정합니다.")]
    private bool startVisible = true;

    private Text targetText;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        targetText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        if (targetText == null)
        {
            targetText = GetComponent<Text>();
        }

        targetText.enabled = startVisible;
        blinkCoroutine = StartCoroutine(BlinkLoop());
    }

    private void OnDisable()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (targetText != null)
        {
            targetText.enabled = true;
        }
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(blinkInterval);
            targetText.enabled = !targetText.enabled;
        }
    }

    private void OnValidate()
    {
        blinkInterval = Mathf.Max(0.05f, blinkInterval);
    }
}
