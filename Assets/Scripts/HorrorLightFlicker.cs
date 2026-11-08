using UnityEngine;

public class HorrorLightFlicker : MonoBehaviour
{
    [SerializeField] private float minIntensity = 0.25f;
    [SerializeField] private float maxIntensity = 2.2f;
    [SerializeField] private float flickerSpeed = 6f;
    [SerializeField] private float glitchChance = 0.015f;

    private Light targetLight;
    private float noiseOffset;
    private float glitchTimer;

    public void Configure(float min, float max, float speed, float chance)
    {
        minIntensity = min;
        maxIntensity = max;
        flickerSpeed = speed;
        glitchChance = chance;
    }

    private void Awake()
    {
        targetLight = GetComponent<Light>();
        noiseOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (targetLight == null)
        {
            return;
        }

        if (glitchTimer > 0f)
        {
            glitchTimer -= Time.deltaTime;
            targetLight.intensity = minIntensity * 0.15f;
            return;
        }

        if (Random.value < glitchChance)
        {
            glitchTimer = Random.Range(0.05f, 0.25f);
            return;
        }

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
