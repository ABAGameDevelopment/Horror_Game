using UnityEngine;

public class CarBreakdownSequence : MonoBehaviour
{
    [Header("Luces de Emergencia (Intermitentes)")]
    [SerializeField] private Light[] lucesIntermitentes;
    [SerializeField] private float frecuenciaParpadeo = 0.55f;

    [Header("Efectos de Motor")]
    [SerializeField] private ParticleSystem particulasHumoMotor;
    [SerializeField] private AudioSource audioSourceHiss;

    private float temporizador = 0f;
    private bool lucesEncendidas = true;

    private void Start()
    {
        // Generar sonido siseo de radiador si no hay clip
        if (audioSourceHiss == null)
        {
            audioSourceHiss = gameObject.AddComponent<AudioSource>();
            audioSourceHiss.loop = true;
            audioSourceHiss.spatialBlend = 1f;
            audioSourceHiss.minDistance = 2f;
            audioSourceHiss.maxDistance = 18f;
            audioSourceHiss.volume = 0.45f;
            audioSourceHiss.clip = GenerarSiseoRadiador();
            audioSourceHiss.Play();
        }
    }

    private void Update()
    {
        temporizador += Time.deltaTime;
        if (temporizador >= frecuenciaParpadeo)
        {
            temporizador = 0f;
            lucesEncendidas = !lucesEncendidas;
            if (lucesIntermitentes != null)
            {
                foreach (var luz in lucesIntermitentes)
                {
                    if (luz != null) luz.enabled = lucesEncendidas;
                }
            }
        }
    }

    private AudioClip GenerarSiseoRadiador()
    {
        int samplerate = 44100;
        int samples = samplerate * 2;
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float noise = (UnityEngine.Random.value * 2f - 1f) * 0.25f;
            data[i] = noise;
        }
        AudioClip clip = AudioClip.Create("SFX_SiseoRadiadorLoop", samples, 1, samplerate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
