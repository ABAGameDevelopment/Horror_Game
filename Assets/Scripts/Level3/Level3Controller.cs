using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level3Controller : MonoBehaviour
{
    public static Level3Controller Instance { get; private set; }

    [Header("Progreso de Pistas")]
    [SerializeField] private int pistasRecolectadas = 0;
    [SerializeField] private int totalPistasRequeridas = 3;
    [SerializeField] private bool expedienteEncontrado = false;
    [SerializeField] private bool bicicletaDesbloqueada = false;

    [Header("Mensajes y Narrativa")]
    [SerializeField] private string mensajeAutoAveriado = "El motor se sobrecalentó y se apagó. Debo seguir a pie e investigar la carretera.";
    [SerializeField] private string mensajeExpediente = "¡El expediente confirma que la criatura proviene del laboratorio universitario! Para detenerla, tengo que regresar.";
    [SerializeField] private string mensajeBicicletaBloqueada = "Aún no tengo suficiente información sobre la criatura. Necesito encontrar el expediente antes de volver.";
    [SerializeField] private string mensajeBicicletaLista = "Presiona [E] para montar la bicicleta y emprender el regreso a la universidad.";

    // UI & Prompts
    private string promptActual = "";
    private string mensajeFlotante = "";
    private float tiempoMensaje = 0f;
    private AudioSource audioSource;

    public int PistasRecolectadas => pistasRecolectadas;
    public int TotalPistas => totalPistasRequeridas;
    public bool ExpedienteEncontrado => expedienteEncontrado;
    public bool BicicletaDesbloqueada => bicicletaDesbloqueada;
    public string PromptActual => promptActual;
    public string MensajeFlotante => mensajeFlotante;

    public event Action<int, int> OnProgresoPistasActualizado;
    public event Action OnExpedienteDescubierto;
    public event Action OnBicicletaHabilitada;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        CorregirMaterialesRosas();
        MostrarMensaje(mensajeAutoAveriado, 6f);
    }

    private void CorregirMaterialesRosas()
    {
        Material matBlanco = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "Mat_LineaBlanca");
#if UNITY_EDITOR
        if (matBlanco == null)
            matBlanco = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Level2/Mat_LineaBlanca.mat");
#endif
        if (matBlanco == null)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (sh == null) sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh != null)
            {
                matBlanco = new Material(sh);
                matBlanco.name = "Mat_LineaBlanca_Runtime";
                matBlanco.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            }
        }

        Material matAsfalto = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "Mat_AsfaltoEstacionamiento");
#if UNITY_EDITOR
        if (matAsfalto == null)
            matAsfalto = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Level2/Mat_AsfaltoEstacionamiento.mat");
#endif

        foreach (var rend in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (rend == null) continue;
            string goName = rend.gameObject.name;

            if (goName == "Linea_Blanca_Acotamiento" && matBlanco != null)
            {
                rend.sharedMaterial = matBlanco;
                continue;
            }

            if ((goName.StartsWith("Acotamiento") || goName == "Suelo_Bosque_Carretera") && matAsfalto != null)
            {
                var curMat = rend.sharedMaterial;
                if (curMat == null || curMat.shader == null || curMat.shader.name == "Hidden/InternalErrorShader" || curMat.name.StartsWith("Default"))
                {
                    rend.sharedMaterial = matAsfalto;
                }
                continue;
            }

            var mats = rend.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null || mats[i].shader == null || mats[i].shader.name == "Hidden/InternalErrorShader" || mats[i].name.StartsWith("Default"))
                {
                    if (goName.Contains("Linea") || goName.Contains("Line") || goName.Contains("Acotamiento"))
                        mats[i] = matBlanco != null ? matBlanco : matAsfalto;
                    else if (matAsfalto != null)
                        mats[i] = matAsfalto;
                    changed = true;
                }
            }
            if (changed)
                rend.sharedMaterials = mats;
        }
    }

    private void Update()
    {
        if (tiempoMensaje > 0f)
        {
            tiempoMensaje -= Time.deltaTime;
            if (tiempoMensaje <= 0f)
                mensajeFlotante = "";
        }
    }

    public void MostrarPrompt(string prompt)
    {
        promptActual = prompt;
    }

    public void LimpiarPrompt(string prompt)
    {
        if (promptActual == prompt)
            promptActual = "";
    }

    public void MostrarMensaje(string mensaje, float duracion = 4f)
    {
        mensajeFlotante = mensaje;
        tiempoMensaje = duracion;
    }

    public void RegistrarPista(string nombrePista, bool esExpediente)
    {
        pistasRecolectadas++;
        if (esExpediente)
        {
            expedienteEncontrado = true;
            OnExpedienteDescubierto?.Invoke();
        }

        if (LevelStateManager.Instance != null)
        {
            LevelStateManager.Instance.AgregarPuntaje(esExpediente ? 500 : 250);
        }

        ReproducirSonidoPista();

        if (expedienteEncontrado || pistasRecolectadas >= totalPistasRequeridas)
        {
            bicicletaDesbloqueada = true;
            MostrarMensaje(mensajeExpediente, 7f);
            OnBicicletaHabilitada?.Invoke();
        }
        else
        {
            MostrarMensaje($"Pista examinada: {nombrePista} ({pistasRecolectadas}/{totalPistasRequeridas})", 4f);
        }

        OnProgresoPistasActualizado?.Invoke(pistasRecolectadas, totalPistasRequeridas);
    }

    public bool PuedeUsarBicicleta()
    {
        return bicicletaDesbloqueada || expedienteEncontrado;
    }

    public string ObtenerMensajeBicicleta()
    {
        return PuedeUsarBicicleta() ? mensajeBicicletaLista : mensajeBicicletaBloqueada;
    }

    public void CompletarRegresoUniversidad()
    {
        StartCoroutine(SecuenciaEscapeBicicleta());
    }

    private IEnumerator SecuenciaEscapeBicicleta()
    {
        MostrarMensaje("Pedaleando de vuelta hacia la universidad...", 5f);
        ReproducirSonidoBicicleta();

        if (LevelStateManager.Instance != null)
        {
            LevelStateManager.Instance.AgregarPuntaje(1000);
        }

        yield return new WaitForSeconds(3f);

        if (LevelStateManager.Instance != null)
        {
            LevelStateManager.Instance.CompletarNivel();
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }

    private void ReproducirSonidoPista()
    {
        if (audioSource != null)
        {
            AudioClip clip = GenerarClipPapel();
            audioSource.PlayOneShot(clip, 0.8f);
        }
    }

    private void ReproducirSonidoBicicleta()
    {
        if (audioSource != null)
        {
            AudioClip clip = GenerarClipCampanaBici();
            audioSource.PlayOneShot(clip, 1f);
        }
    }

    private AudioClip GenerarClipPapel()
    {
        int samplerate = 44100;
        int samples = (int)(samplerate * 0.4f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samplerate;
            float noise = (UnityEngine.Random.value * 2f - 1f) * Mathf.Exp(-t * 9f);
            float freq = Mathf.Sin(2f * Mathf.PI * 400f * t) * 0.2f;
            data[i] = Mathf.Clamp(noise * 0.7f + freq, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("SFX_HojaPapel", samples, 1, samplerate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerarClipCampanaBici()
    {
        int samplerate = 44100;
        int samples = (int)(samplerate * 1.5f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samplerate;
            // Campana timbal doble ring-ring
            float tone1 = Mathf.Sin(2f * Mathf.PI * 1760f * t) * Mathf.Exp(-Mathf.Repeat(t, 0.25f) * 12f);
            float tone2 = Mathf.Sin(2f * Mathf.PI * 2200f * t) * Mathf.Exp(-Mathf.Repeat(t, 0.25f) * 16f) * 0.6f;
            data[i] = Mathf.Clamp((tone1 + tone2) * 0.7f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("SFX_TimbreBicicleta", samples, 1, samplerate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
