using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TimeManager : MonoBehaviour
{
    [SerializeField] TMP_Text timeScaleText = null;

    readonly List<float> timePresets = new List<float>() { 0.5f, 0.6f, 0.7f, 0.8f, 0.9f,
                                                  1f,
                                                  1.1f, 1.2f, 1.3f, 1.4f, 1.5f};
    int currentIndex = 0;

    AudioSource[] sources = null;

    private void Start()
    {
        Time.timeScale = 1;

        sources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource a in sources)
            a.pitch = Time.timeScale;

        currentIndex = timePresets.IndexOf(1f);

        timeScaleText.text = Time.timeScale.ToString();
    }

    public void DecreaseTimeScale()
    {
        if (currentIndex - 1 < 0)
            return;

        currentIndex--;

        float t = timePresets[currentIndex];
        Time.timeScale = t;

        foreach (AudioSource a in sources)
            a.pitch = t;

        timeScaleText.text = t.ToString();
    }

    public void IncreaseTimeScale()
    {
        if (currentIndex + 1 >= timePresets.Count)
            return;

        currentIndex++;

        float t = timePresets[currentIndex];
        Time.timeScale = t;

        foreach (AudioSource a in sources)
            a.pitch = t;

        timeScaleText.text = t.ToString();
    }
}
