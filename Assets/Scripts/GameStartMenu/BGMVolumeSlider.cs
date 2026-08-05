using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class BGMVolumeSlider : MonoBehaviour
{
    private const string VolumePreferenceKey = "BGMVolume";

    [Header("References")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private Slider volumeSlider;

    [Header("Settings")]
    [SerializeField] private bool rememberVolume = true;
    [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.5f;

    private void Awake()
    {
        if (volumeSlider == null)
        {
            volumeSlider = GetComponent<Slider>();
        }

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.wholeNumbers = false;
    }

    private void OnEnable()
    {
        float startVolume = GetStartVolume();

        volumeSlider.SetValueWithoutNotify(startVolume);
        ApplyVolume(startVolume);
        volumeSlider.onValueChanged.AddListener(ApplyVolume);
    }

    private void OnDisable()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(ApplyVolume);
        }

        if (rememberVolume)
        {
            PlayerPrefs.Save();
        }
    }

    private float GetStartVolume()
    {
        if (rememberVolume && PlayerPrefs.HasKey(VolumePreferenceKey))
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePreferenceKey));
        }

        if (backgroundMusic != null)
        {
            return Mathf.Clamp01(backgroundMusic.volume);
        }

        return defaultVolume;
    }

    public void ApplyVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (backgroundMusic != null)
        {
            backgroundMusic.volume = value;
        }

        if (rememberVolume)
        {
            PlayerPrefs.SetFloat(VolumePreferenceKey, value);
        }
    }

    public void SyncSliderFromAudioSource()
    {
        if (backgroundMusic == null || volumeSlider == null)
        {
            return;
        }

        float currentVolume = Mathf.Clamp01(backgroundMusic.volume);
        volumeSlider.SetValueWithoutNotify(currentVolume);
    }
}
