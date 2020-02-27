using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private bool _isShaking;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _isShaking = false;
    }

    public void Shake(float duration = 0.15f, float strength = 0.30f)
    {
        if (!_isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, strength));
        }
    }
    
    private IEnumerator ShakeCoroutine(float duration, float strength)
    {
        _isShaking = true;
        Vector3 originalPos = transform.localPosition;
        float targetTime = Time.time + duration;

        while (Time.time < targetTime)
        {
            float x = Random.Range(-1.0f, 1.0f) * strength;
            float y = Random.Range(-1.0f, 1.0f) * strength;
            transform.localPosition = new Vector3(x, y, originalPos.z);
            yield return null;
        }
        transform.localPosition = originalPos;
        _isShaking = false;
    }
}