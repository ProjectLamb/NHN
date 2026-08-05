using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class CanvasSwitchButton : MonoBehaviour
{
    [Header("전환할 Canvas")]
    [SerializeField]
    [Tooltip("버튼을 클릭하면 비활성화할 Canvas")]
    private Canvas canvasToDisable;

    [SerializeField]
    [Tooltip("버튼을 클릭하면 활성화할 Canvas")]
    private Canvas canvasToEnable;

    private Button targetButton;

    private void Awake()
    {
        targetButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }

        targetButton.onClick.AddListener(SwitchCanvas);
    }

    private void OnDisable()
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(SwitchCanvas);
        }
    }

    public void SwitchCanvas()
    {
        if (canvasToDisable == canvasToEnable && canvasToDisable != null)
        {
            Debug.LogWarning("비활성화할 Canvas와 활성화할 Canvas가 같습니다.", this);
            return;
        }

        if (canvasToEnable != null)
        {
            canvasToEnable.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("활성화할 Canvas가 지정되지 않았습니다.", this);
        }

        if (canvasToDisable != null)
        {
            canvasToDisable.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("비활성화할 Canvas가 지정되지 않았습니다.", this);
        }
    }
}
