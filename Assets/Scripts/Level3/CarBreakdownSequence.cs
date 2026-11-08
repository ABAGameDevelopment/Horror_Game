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

    private void Awake()
    {
        EliminarResiduosCofre();
    }

    public void EliminarResiduosCofre()
    {
        Transform hood = transform.parent != null ? transform.parent.Find("Cofre_Abierto_Vapor") : null;
        if (hood == null)
        {
            GameObject hGo = GameObject.Find("Cofre_Abierto_Vapor");
            if (hGo != null) hood = hGo.transform;
        }
        if (hood != null) Destroy(hood.gameObject);

        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (obj != null && (obj.name.Contains("Cofre_Abierto_Vapor") || obj.name.Contains("CarEngineBay") || obj.name.Contains("CarHood")))
            {
                Destroy(obj);
            }
        }
    }

    private void Start()
    {
        EliminarResiduosCofre();

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
