using UnityEngine;
using UnityEngine.Audio;

public class AudioVolumeManager : MonoBehaviour
{
    public static AudioVolumeManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField]
    private AudioMixer audioMixer;

    [Tooltip("AudioMixer에서 노출한 볼륨 파라미터 이름")]
    [SerializeField]
    private string volumeParameter = "MasterVolume";

    [Header("Default Volume")]
    [SerializeField, Range(0f, 1f)]
    private float defaultVolume = 1f;

    private const string VolumeSaveKey = "MasterVolumeValue";

    public float CurrentVolume { get; private set; }

    private void Awake()
    {
        // 이전 씬의 AudioVolumeManager가 이미 살아 있다면
        // 새로 생성된 중복 오브젝트는 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 제거되지 않게 설정
        DontDestroyOnLoad(gameObject);

        // 저장된 볼륨 불러오기
        CurrentVolume = PlayerPrefs.GetFloat(
            VolumeSaveKey,
            defaultVolume
        );

        ApplyVolume(CurrentVolume);
    }

    public void SetMasterVolume(float value)
    {
        CurrentVolume = Mathf.Clamp01(value);

        ApplyVolume(CurrentVolume);

        // 게임을 껐다 켜도 유지되도록 저장
        PlayerPrefs.SetFloat(
            VolumeSaveKey,
            CurrentVolume
        );

        PlayerPrefs.Save();
    }

    private void ApplyVolume(float value)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning(
                "AudioVolumeManager에 AudioMixer가 연결되지 않았습니다."
            );

            return;
        }

        // Slider의 0~1 값을 AudioMixer의 데시벨 값으로 변환
        float decibel;

        if (value <= 0.0001f)
        {
            decibel = -80f;
        }
        else
        {
            decibel = Mathf.Log10(value) * 20f;
        }

        audioMixer.SetFloat(
            volumeParameter,
            decibel
        );
    }
}