using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightController : MonoBehaviour
{
    public Light sun;
    public float secondsInFullDay = 120f;
    [Range(0, 1)]
    public float currentTimeOfDay = 0;
    [HideInInspector]
    public float timeMultiplier = 1f;

    float sunInitialIntensity;

    void Start()
    {
        sunInitialIntensity = sun.intensity;
        currentTimeOfDay = 0.22f;
    }

    void Update()
    {
        UpdateSun();

        currentTimeOfDay += (Time.deltaTime / secondsInFullDay) * timeMultiplier;

        if (currentTimeOfDay >= 0.8f)
            currentTimeOfDay = 0.22f;
    }

    void UpdateSun()
    {
        sun.transform.localRotation = Quaternion.Euler((currentTimeOfDay * 360f) - 90, 170, 0);

        float intensityMultiplier = 1;
        if (currentTimeOfDay <= 0.221f || currentTimeOfDay >= 0.799f)
            intensityMultiplier = 0;
        else if (currentTimeOfDay <= 0.221f)
            intensityMultiplier = Mathf.Clamp01((currentTimeOfDay - 0.221f) * (1 / 0.02f));
        else if (currentTimeOfDay >= 0.799f)
            intensityMultiplier = Mathf.Clamp01(1 - ((currentTimeOfDay - 0.799f) * (1 / 0.02f)));

        sun.intensity = sunInitialIntensity * intensityMultiplier;
    }
}
