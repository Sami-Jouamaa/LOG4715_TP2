using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class AutoRespawnToggle : MonoBehaviour
{
    void Start()
    {
        var toggle = GetComponent<Toggle>();
        toggle.isOn = PlayerPrefs.GetInt("AutoRespawn", 0) == 1;
        toggle.onValueChanged.AddListener(v => PlayerPrefs.SetInt("AutoRespawn", v ? 1 : 0));
    }
}