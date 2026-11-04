using System.Collections;
using UnityEngine;

public class CarreteraEventosTerror : MonoBehaviour
{
    public static CarreteraEventosTerror Instance { get; private set; }

    [Header("Referencias de Eventos")]
    [SerializeField] private Transform posicionAutoAveriado;
    [SerializeField] private GameObject siluetaEnteNiebla;
    [SerializeField] private AudioSource audioRadioAuto;
    [SerializeField] private AudioSource audioAmbienteBosque;
    [SerializeField] private Light[] farolasParada;

    private bool avistamientoActivado = false;
    private Transform transformJugador;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuscarJugador();
        ConfigurarAmbienteBosque();
        ConfigurarRadioAuto();

        if (siluetaEnteNiebla != null)
            siluetaEnteNiebla.SetActive(false);
    }

    private void BuscarJugador()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null)
        {
            PlayerController pc = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) p = pc.gameObject;
        }
        if (p != null) transformJugador = p.transform;
    }

    private void Update()
    {
        if (transformJugador == null)
        {
            BuscarJugador();
            if (transformJugador == null) return;
        }

        // Trigger de avistamiento a mitad del camino (z = 60m - 70m)
        if (!avistamientoActivado && transformJugador.position.z >= 60f)
        {
            avistamientoActivado = true;
            StartCoroutine(SecuenciaAvistamientoEnte());
        }
    }

    private void ConfigurarAmbienteBosque()
    {
        if (audioAmbienteBosque == null)
        {
            audioAmbienteBosque = gameObject.AddComponent<AudioSource>();
            audioAmbienteBosque.loop = true;
            audioAmbienteBosque.volume = 0.35f;
            audioAmbienteBosque.spatialBlend = 0f; // 2D Ambiental
        }

        AudioClip clipAmbiente = Resources.Load<AudioClip>("Audios/AtmosphereLevel1");
        if (clipAmbiente != null)
        {
            audioAmbienteBosque.clip = clipAmbiente;
            audioAmbienteBosque.Play();
        }
        else
        {
            audioAmbienteBosque.clip = GenerarVientoOminoso();
            audioAmbienteBosque.Play();
        }
    }

    private void ConfigurarRadioAuto()
    {
        if (posicionAutoAveriado == null) return;

        if (audioRadioAuto == null)
        {
            GameObject radioObj = new GameObject("Radio_Auto_Estatica");
            radioObj.transform.SetParent(posicionAutoAveriado);
            radioObj.transform.localPosition = new Vector3(0f, 0.6f, 0.2f);
            audioRadioAuto = radioObj.AddComponent<AudioSource>();
            audioRadioAuto.loop = true;
            audioRadioAuto.spatialBlend = 1f; // 3D Espacial
            audioRadioAuto.minDistance = 2f;
            audioRadioAuto.maxDistance = 14f;
            audioRadioAuto.volume = 0.5f;
            audioRadioAuto.clip = GenerarEstaticaRadio();
            audioRadioAuto.Play();
        }
    }

    private IEnumerator SecuenciaAvistamientoEnte()
    {
        // 1. Mostrar la silueta entre los pinos en la niebla
        if (siluetaEnteNiebla != null)
        {
            siluetaEnteNiebla.SetActive(true);
        }

        // 2. Viñeta y pulso de miedo
        VignetaHorror vigneta = UnityEngine.Object.FindAnyObjectByType<VignetaHorror>();
        if (vigneta != null)
        {
            vigneta.PulsoMiedo(1.2f, 0.88f);
        }

        // 3. Sonido stinger de terror
        AudioSource audioStinger = gameObject.AddComponent<AudioSource>();
        audioStinger.spatialBlend = 0f;
        audioStinger.PlayOneShot(GenerarStingerSusto(), 0.9f);

        yield return new WaitForSeconds(2.2f);

        // 4. El ente se desvanece en la niebla
        if (siluetaEnteNiebla != null)
        {
            siluetaEnteNiebla.SetActive(false);
        }

        Destroy(audioStinger, 4f);
    }

    public void DispararParpadeoFarolasParada()
    {
        StartCoroutine(ParpadeoIntensoFarolas());
    }

    private IEnumerator ParpadeoIntensoFarolas()
    {
        if (farolasParada == null || farolasParada.Length == 0) yield break;

        for (int i = 0; i < 8; i++)
        {
            foreach (var l in farolasParada)
            {
                if (l != null) l.enabled = !l.enabled;
            }
            yield return new WaitForSeconds(Random.Range(0.06f, 0.15f));
        }

        foreach (var l in farolasParada)
        {
            if (l != null) l.enabled = true;
        }
    }

    private AudioClip GenerarVientoOminoso()
    {
        int samplerate = 44100;
        int samples = samplerate * 4;
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samplerate;
            float lowRumble = Mathf.Sin(2f * Mathf.PI * (45f + Mathf.Sin(t * 0.4f) * 15f) * t) * 0.4f;
            float windHiss = (UnityEngine.Random.value * 2f - 1f) * (0.12f + Mathf.Sin(t * 0.8f) * 0.08f);
            data[i] = Mathf.Clamp(lowRumble + windHiss, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("SFX_VientoOminoso", samples, 1, samplerate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerarEstaticaRadio()
    {
        int samplerate = 44100;
        int samples = samplerate * 3;
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samplerate;
            float noise = (UnityEngine.Random.value * 2f - 1f) * 0.35f;
            float beep = Mathf.Sin(2f * Mathf.PI * 1000f * t) * (Mathf.Repeat(t, 0.8f) < 0.05f ? 0.3f : 0f);
            data[i] = Mathf.Clamp(noise + beep, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("SFX_EstaticaRadio", samples, 1, samplerate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerarStingerSusto()
    {
        int samplerate = 44100;
        int samples = (int)(samplerate * 1.8f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samplerate;
            float disharmony1 = Mathf.Sin(2f * Mathf.PI * 311.13f * t); // Eb4
            float disharmony2 = Mathf.Sin(2f * Mathf.PI * 440f * t);    // A4 (Tritono diabólico)
            float disharmony3 = Mathf.Sin(2f * Mathf.PI * 622.25f * t); // Eb5
            float drop = Mathf.Exp(-t * 2.2f);
            float impact = (Mathf.Sin(2f * Mathf.PI * 65f * t) + (UnityEngine.Random.value * 2f - 1f) * 0.5f) * Mathf.Exp(-t * 9f);
            data[i] = Mathf.Clamp((disharmony1 + disharmony2 + disharmony3) * 0.28f * drop + impact * 0.6f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("SFX_StingerTerror", samples, 1, samplerate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
