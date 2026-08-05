//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;

//[RequireComponent(typeof(CanvasGroup))]
//public class RandomLoadingCanvas : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private Slider loadingSlider;

//    [Header("Loading Time")]
//    [SerializeField, Min(0.1f)] private float minimumDuration = 3f;
//    [SerializeField, Min(0.1f)] private float maximumDuration = 5f;
//    [SerializeField] private AnimationCurve progressCurve =
//        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

//    [Header("Fade Out")]
//    [SerializeField, Min(0f)] private float fullGaugeHoldTime = 0.15f;
//    [SerializeField, Min(0.01f)] private float fadeDuration = 0.65f;

//    private CanvasGroup canvasGroup;
//    private Coroutine loadingRoutine;

//    private void Awake()
//    {
//        canvasGroup = GetComponent<CanvasGroup>();
//    }

//    private void OnEnable()
//    {
//        ResetLoadingScreen();
//        loadingRoutine = StartCoroutine(FillAndFadeRoutine());
//    }

//    private void OnDisable()
//    {
//        if (loadingRoutine != null)
//        {
//            StopCoroutine(loadingRoutine);
//            loadingRoutine = null;
//        }
//    }

//    private void ResetLoadingScreen()
//    {
//        canvasGroup.alpha = 1f;
//        canvasGroup.interactable = true;
//        canvasGroup.blocksRaycasts = true;

//        if (loadingSlider != null)
//        {
//            loadingSlider.minValue = 0f;
//            loadingSlider.maxValue = 1f;
//            loadingSlider.wholeNumbers = false;
//            loadingSlider.SetValueWithoutNotify(0f);
//        }
//    }

//    private IEnumerator FillAndFadeRoutine()
//    {
//        float low = Mathf.Min(minimumDuration, maximumDuration);
//        float high = Mathf.Max(minimumDuration, maximumDuration);
//        float chosenDuration = Random.Range(low, high);
//        float elapsed = 0f;

//        while (elapsed < chosenDuration)
//        {
//            elapsed += Time.unscaledDeltaTime;
//            float normalizedTime = Mathf.Clamp01(elapsed / chosenDuration);
//            float progress = progressCurve.Evaluate(normalizedTime);

//            if (loadingSlider != null)
//            {
//                loadingSlider.SetValueWithoutNotify(Mathf.Clamp01(progress));
//            }

//            yield return null;
//        }

//        if (loadingSlider != null)
//        {
//            loadingSlider.SetValueWithoutNotify(1f);
//        }

//        if (fullGaugeHoldTime > 0f)
//        {
//            yield return new WaitForSecondsRealtime(fullGaugeHoldTime);
//        }

//        canvasGroup.interactable = false;
//        canvasGroup.blocksRaycasts = false;

//        float fadeElapsed = 0f;
//        while (fadeElapsed < fadeDuration)
//        {
//            fadeElapsed += Time.unscaledDeltaTime;
//            canvasGroup.alpha = 1f - Mathf.Clamp01(fadeElapsed / fadeDuration);
//            yield return null;
//        }

//        canvasGroup.alpha = 0f;
//        loadingRoutine = null;
//        gameObject.SetActive(false);
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class RandomLoadingCanvas : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider loadingSlider;

    [Header("Loading Time")]
    [SerializeField, Min(0.1f)] private float minimumDuration = 3f;
    [SerializeField, Min(0.1f)] private float maximumDuration = 5f;
    [SerializeField]
    private AnimationCurve progressCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Fade Out")]
    [SerializeField, Min(0f)] private float fullGaugeHoldTime = 0.15f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.65f;

    private CanvasGroup canvasGroup;
    private Coroutine loadingRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        ResetLoadingScreen();
        loadingRoutine = StartCoroutine(FillAndFadeRoutine());
    }

    private void OnDisable()
    {
        if (loadingRoutine != null)
        {
            StopCoroutine(loadingRoutine);
            loadingRoutine = null;
        }
    }

    private void ResetLoadingScreen()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (loadingSlider != null)
        {
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = 1f;
            loadingSlider.wholeNumbers = false;
            loadingSlider.SetValueWithoutNotify(0f);
        }
    }

    private IEnumerator FillAndFadeRoutine()
    {
        float low = Mathf.Min(minimumDuration, maximumDuration);
        float high = Mathf.Max(minimumDuration, maximumDuration);
        float chosenDuration = Random.Range(low, high);
        float elapsed = 0f;

        while (elapsed < chosenDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / chosenDuration);
            float progress = progressCurve.Evaluate(normalizedTime);

            if (loadingSlider != null)
            {
                loadingSlider.SetValueWithoutNotify(Mathf.Clamp01(progress));
            }

            yield return null;
        }

        if (loadingSlider != null)
        {
            loadingSlider.SetValueWithoutNotify(1f);
        }

        if (fullGaugeHoldTime > 0f)
        {
            yield return new WaitForSecondsRealtime(fullGaugeHoldTime);
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(fadeElapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        loadingRoutine = null;
        gameObject.SetActive(false);
    }
}