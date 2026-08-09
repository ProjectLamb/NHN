using UnityEngine;
using UnityEngine.UI;

public class AudioVolumeSlider : MonoBehaviour
{
    [SerializeField]
    private Slider volumeSlider;

    private void Awake()
    {
        if (volumeSlider == null)
        {
            volumeSlider = GetComponent<Slider>();
        }
    }

    private void Start()
    {
        if (volumeSlider == null)
        {
            Debug.LogWarning(
                "AudioVolumeSlider에 Slider가 연결되지 않았습니다."
            );

            return;
        }

        if (AudioVolumeManager.Instance == null)
        {
            Debug.LogWarning(
                "씬에서 AudioVolumeManager를 찾지 못했습니다."
            );

            return;
        }

        // 저장되어 있는 볼륨으로 Slider 위치 맞추기
        volumeSlider.SetValueWithoutNotify(
            AudioVolumeManager.Instance.CurrentVolume
        );

        // Slider를 움직이면 볼륨 변경
        volumeSlider.onValueChanged.AddListener(
            OnVolumeChanged
        );
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(
                OnVolumeChanged
            );
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (AudioVolumeManager.Instance != null)
        {
            AudioVolumeManager.Instance.SetMasterVolume(value);
        }
    }
}