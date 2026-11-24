using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;

    const string Param = "Volume";   // must match the exposed parameter name

    void OnEnable()
    {
        if (volumeSlider == null) return;

        // Slider and mixer now share the same unit (dB), so just copy the value
        if (audioMixer.GetFloat(Param, out float db))
            volumeSlider.SetValueWithoutNotify(db);
    }

    public void SetVolume(float db)
    {
        audioMixer.SetFloat(Param, db);
    }

    public void setFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
}