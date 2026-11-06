using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(CharacterController))]
public class PasosJugador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CharacterController controlador;
    [SerializeField] private AudioSource audioPasos;

    [Header("Sonidos")]
    [SerializeField] private AudioClip sonidoCaminar;
    [SerializeField] private AudioClip sonidoCorrer;

    [Header("Configuracion (Idéntica a Nivel 1)")]
    [SerializeField] private float velocidadMinima = 0.15f;
    [SerializeField] private float velocidadParaCorrer = 5.5f;

    private PlayerController playerCtrl;
    private float tiempoPaso = 0f;

    private void Awake()
    {
        AsegurarComponentes();
    }

    private void Start()
    {
        AsegurarComponentes();
    }

    public void AsegurarComponentes()
    {
        if (controlador == null)
            controlador = GetComponent<CharacterController>();

        if (playerCtrl == null)
            playerCtrl = GetComponent<PlayerController>();

        if (audioPasos == null)
            audioPasos = GetComponent<AudioSource>();

        if (audioPasos == null)
            audioPasos = gameObject.AddComponent<AudioSource>();

        audioPasos.playOnAwake = false;
        audioPasos.loop = false;
        audioPasos.spatialBlend = 0f; // 2D estéreo directo
        audioPasos.volume = 0.75f;    // Mismo volumen que Nivel 1

        // Asignar el grupo "Efectos" del MasterAudioMixer idéntico a Nivel 1
#if UNITY_EDITOR
        if (audioPasos.outputAudioMixerGroup == null)
        {
            AudioMixer mixer = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Audio/MasterAudioMixer.mixer");
            if (mixer != null)
            {
                AudioMixerGroup[] grupos = mixer.FindMatchingGroups("Efectos");
                if (grupos != null && grupos.Length > 0)
                {
                    audioPasos.outputAudioMixerGroup = grupos[0];
                }
            }
        }
#endif

        // Sonido oficial de caminar de Nivel 1 (Walk.mp3)
        if (sonidoCaminar == null)
        {
#if UNITY_EDITOR
            sonidoCaminar = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Walk.mp3");
#endif
            if (sonidoCaminar == null)
                sonidoCaminar = Resources.FindObjectsOfTypeAll<AudioClip>().FirstOrDefault(c => c != null && c.name.ToLower().Contains("walk"));
        }

        // Sonido de correr (Footsteps.mp3)
        if (sonidoCorrer == null)
        {
#if UNITY_EDITOR
            sonidoCorrer = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Footsteps.mp3");
#endif
            if (sonidoCorrer == null)
                sonidoCorrer = Resources.FindObjectsOfTypeAll<AudioClip>().FirstOrDefault(c => c != null && c.name.ToLower().Contains("footstep"));
            if (sonidoCorrer == null)
                sonidoCorrer = sonidoCaminar;
        }
    }

    private void Update()
    {
        if (audioPasos == null) return;
        if (playerCtrl == null) playerCtrl = GetComponent<PlayerController>();
        if (controlador == null) controlador = GetComponent<CharacterController>();

        bool moviendose = false;
        bool corriendo = false;

        if (playerCtrl != null)
        {
            moviendose = playerCtrl.EstaMoviendose;
            corriendo = playerCtrl.EstaCorriendo;
        }
        else if (controlador != null)
        {
            Vector3 vel = controlador.velocity;
            vel.y = 0f;
            moviendose = vel.sqrMagnitude > (velocidadMinima * velocidadMinima);
            corriendo = vel.magnitude >= velocidadParaCorrer;
        }

        if (!moviendose)
        {
            tiempoPaso = 0f;
            return;
        }

        // Cadencia rítmica de pasos (reproduce Walk.mp3 idéntico a Nivel 1 de forma continua)
        float cadencia = corriendo ? 0.30f : 0.48f;
        tiempoPaso += Time.deltaTime;

        if (tiempoPaso >= cadencia)
        {
            tiempoPaso = 0f;
            AudioClip clip = corriendo ? sonidoCorrer : sonidoCaminar;
            if (clip == null) clip = sonidoCaminar ?? sonidoCorrer;

            if (clip != null)
            {
                audioPasos.pitch = corriendo ? Random.Range(1.15f, 1.25f) : Random.Range(0.96f, 1.04f);
                audioPasos.PlayOneShot(clip, 0.75f);
            }
        }
    }

    public void DetenerPasos()
    {
        tiempoPaso = 0f;
        if (audioPasos != null)
            audioPasos.Stop();
        enabled = false;
    }
}
