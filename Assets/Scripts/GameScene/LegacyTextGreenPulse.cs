using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPTextGreenPulse : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField]
    private TextMeshProUGUI targetText;

    [Header("Green Colors")]
    [SerializeField]
    private Color darkGreen =
        new Color(0.02f, 0.22f, 0.07f, 1f);

    [SerializeField]
    private Color brightGreen =
        new Color(0.25f, 1f, 0.42f, 1f);

    [Header("Timing")]
    [Tooltip("어두운 색에서 밝은 색까지 변하는 시간입니다.")]
    [SerializeField, Min(0.01f)]
    private float halfCycleDuration = 0.5f;

    private float elapsed;

    private void Awake()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        elapsed = 0f;

        if (targetText != null)
        {
            targetText.color = darkGreen;
        }
    }

    private void Update()
    {
        if (targetText == null)
        {
            return;
        }

        elapsed += Time.unscaledDeltaTime;

        float t = Mathf.PingPong(
            elapsed / halfCycleDuration,
            1f
        );

        // 색상이 부드럽게 변화하도록 보간
        t = t * t * (3f - 2f * t);

        targetText.color = Color.Lerp(
            darkGreen,
            brightGreen,
            t
        );
    }
}