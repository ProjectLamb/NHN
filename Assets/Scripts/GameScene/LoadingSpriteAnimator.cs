//using UnityEngine;
//using UnityEngine.UI;

//[RequireComponent(typeof(Image))]
//public class LoadingSpriteAnimator : MonoBehaviour
//{
//    [Header("Reference")]
//    [SerializeField] private Image targetImage;

//    [Header("Animation Frames")]
//    [SerializeField] private Sprite[] frames;
//    [SerializeField, Min(0.02f)] private float secondsPerFrame = 0.18f;

//    private float elapsed;
//    private int currentFrame;

//    private void Awake()
//    {
//        if (targetImage == null)
//        {
//            targetImage = GetComponent<Image>();
//        }
//    }

//    private void OnEnable()
//    {
//        elapsed = 0f;
//        currentFrame = 0;
//        ShowCurrentFrame();
//    }

//    private void Update()
//    {
//        if (frames == null || frames.Length == 0 || targetImage == null)
//        {
//            return;
//        }

//        elapsed += Time.unscaledDeltaTime;

//        while (elapsed >= secondsPerFrame)
//        {
//            elapsed -= secondsPerFrame;
//            currentFrame = (currentFrame + 1) % frames.Length;
//            ShowCurrentFrame();
//        }
//    }

//    private void ShowCurrentFrame()
//    {
//        if (targetImage == null || frames == null || frames.Length == 0)
//        {
//            return;
//        }

//        targetImage.sprite = frames[currentFrame];
//        targetImage.enabled = frames[currentFrame] != null;
//    }
//}

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LoadingSpriteAnimator : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Image targetImage;

    [Header("Animation Frames")]
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(0.02f)] private float secondsPerFrame = 0.18f;
    [SerializeField, Min(0f)] private float loopPauseSeconds;

    private float elapsed;
    private float pauseRemaining;
    private int currentFrame;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        elapsed = 0f;
        pauseRemaining = 0f;
        currentFrame = 0;
        ShowCurrentFrame();
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || targetImage == null)
        {
            return;
        }

        if (pauseRemaining > 0f)
        {
            pauseRemaining -= Time.unscaledDeltaTime;

            if (pauseRemaining <= 0f)
            {
                currentFrame = 0;
                ShowCurrentFrame();
            }

            return;
        }

        elapsed += Time.unscaledDeltaTime;

        while (elapsed >= secondsPerFrame)
        {
            elapsed -= secondsPerFrame;

            if (currentFrame >= frames.Length - 1)
            {
                elapsed = 0f;
                pauseRemaining = loopPauseSeconds;

                if (pauseRemaining <= 0f)
                {
                    currentFrame = 0;
                    ShowCurrentFrame();
                }

                break;
            }

            currentFrame++;
            ShowCurrentFrame();
        }
    }

    private void ShowCurrentFrame()
    {
        if (targetImage == null || frames == null || frames.Length == 0)
        {
            return;
        }

        targetImage.sprite = frames[currentFrame];
        targetImage.enabled = frames[currentFrame] != null;
    }
}
