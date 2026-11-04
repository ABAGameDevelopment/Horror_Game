using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LectorDocumentoUI : MonoBehaviour
{
    public static LectorDocumentoUI Instance { get; private set; }

    private GameObject panelRaiz;
    private Image imagenDocumento;
    private TextMeshProUGUI textoTitulo;
    private TextMeshProUGUI textoContenido;
    private TextMeshProUGUI textoSello;
    private TextMeshProUGUI textoInstruccion;
    private AudioSource audioSource;

    private bool abierto = false;
    private Action onCerrarCallback;

    public bool EstaAbierto => abierto;

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

        ConstruirUI();
    }

    private void Update()
    {
        if (!abierto) return;

        bool cerrar = false;
        if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            cerrar = true;
        if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.buttonWest.wasPressedThisFrame))
            cerrar = true;

        if (cerrar)
        {
            CerrarDocumento();
        }
    }

    private void ConstruirUI()
    {
        Canvas canvas = RuntimeUI.CrearCanvas("Canvas_LectorDocumentos", 40);
        panelRaiz = canvas.gameObject;

        // Fondo oscuro translúcido
        Image fondo = RuntimeUI.CrearPanel(canvas.transform, "FondoOscuro", new Color(0.01f, 0.01f, 0.02f, 0.94f));
        RuntimeUI.Estirar(fondo.rectTransform, 0f, 0f, 0f, 0f);
        RuntimeUI.CrearVigneta(canvas.transform, 0.65f);

        // Marco contenedor de terror estilo Resident Evil
        Image marco = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "MarcoDocumento", new Vector2(980f, 620f), new Color(0.05f, 0.05f, 0.07f, 0.98f));
        RuntimeUI.Anclar(marco.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980f, 620f));

        // Cabecera con sello confidencial
        textoSello = RuntimeUI.CrearTexto(marco.transform, "SelloConfidencial", "— ARCHIVO CLASIFICADO // UNIVERSIDAD —", 16f, TextAlignmentOptions.Center, new Color(0.9f, 0.2f, 0.2f, 0.95f));
        textoSello.fontStyle = FontStyles.Bold;
        RuntimeUI.Anclar(textoSello.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(900f, 24f));

        textoTitulo = RuntimeUI.CrearTituloHorror(marco.transform, "TituloDocumento", "TITULO DE LA PISTA", 30f, new Color(0.95f, 0.90f, 0.80f, 1f));
        RuntimeUI.Anclar(textoTitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(900f, 40f));

        // Contenedor de Imagen de la Pista (Izquierda)
        Image panelFoto = RuntimeUI.CrearPanel(marco.transform, "PanelFoto", new Color(0.02f, 0.02f, 0.02f, 0.9f));
        RuntimeUI.Anclar(panelFoto.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(35f, -15f), new Vector2(400f, 450f));

        GameObject goImg = new GameObject("FotoDocumento");
        goImg.transform.SetParent(panelFoto.transform, false);
        imagenDocumento = goImg.AddComponent<Image>();
        imagenDocumento.preserveAspect = true;
        RuntimeUI.Estirar(imagenDocumento.rectTransform, 10f, 10f, 10f, 10f);

        // Contenedor de Texto y Transcripción (Derecha)
        Image panelTexto = RuntimeUI.CrearPanel(marco.transform, "PanelTexto", new Color(0.03f, 0.03f, 0.04f, 0.85f));
        RuntimeUI.Anclar(panelTexto.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-35f, -15f), new Vector2(480f, 450f));

        textoContenido = RuntimeUI.CrearTexto(panelTexto.transform, "TextoContenido", "Contenido del expediente...", 18f, TextAlignmentOptions.TopLeft, new Color(0.85f, 0.82f, 0.75f, 1f));
        RuntimeUI.Estirar(textoContenido.rectTransform, 20f, 20f, 20f, 20f);

        // Barra inferior: Instrucción para continuar
        textoInstruccion = RuntimeUI.CrearTexto(marco.transform, "TextoInstruccion", "[E] / [ESPACIO] Cerrar y continuar", 15f, TextAlignmentOptions.Center, RuntimeUI.ColorTextoTenue);
        RuntimeUI.Anclar(textoInstruccion.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(800f, 24f));

        panelRaiz.SetActive(false);
    }

    public void MostrarDocumento(string titulo, string contenido, Sprite foto, string etiquetaSello = "", Action alCerrar = null)
    {
        textoTitulo.text = titulo.ToUpper();
        textoContenido.text = contenido;
        onCerrarCallback = alCerrar;

        if (!string.IsNullOrEmpty(etiquetaSello))
            textoSello.text = etiquetaSello;

        if (foto != null)
        {
            imagenDocumento.gameObject.SetActive(true);
            imagenDocumento.sprite = foto;
        }
        else
        {
            imagenDocumento.gameObject.SetActive(false);
        }

        abierto = true;
        panelRaiz.SetActive(true);

        // Bloquear movimiento de jugador mientras lee
        CongelarJugador(true);
        ReproducirSonidoPapel();
    }

    public void CerrarDocumento()
    {
        if (!abierto) return;

        abierto = false;
        panelRaiz.SetActive(false);
        CongelarJugador(false);

        ReproducirSonidoCierre();
        onCerrarCallback?.Invoke();
        onCerrarCallback = null;
    }

    private void CongelarJugador(bool congelar)
    {
        PlayerController pc = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            pc.enabled = !congelar;
        }
    }

    private void ReproducirSonidoPapel()
    {
        if (audioSource != null)
        {
            AudioClip clip = GenerarClipPapel();
            audioSource.PlayOneShot(clip, 0.9f);
        }
    }

    private void ReproducirSonidoCierre()
    {
        if (audioSource != null)
        {
            AudioClip clip = GenerarClipCierre();
            audioSource.PlayOneShot(clip, 0.7f);
        }
    }

    private AudioClip GenerarClipPapel()
    {
        int samplerate = 44100;
        int samples = (int)(samplerate * 0.45f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samplerate;
            float noise = (UnityEngine.Random.value * 2f - 1f) * Mathf.Exp(-t * 8f);
            float low = Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.3f;
            data[i] = Mathf.Clamp(noise * 0.65f + low, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("SFX_InspeccionarDoc", samples, 1, samplerate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerarClipCierre()
    {
        int samplerate = 44100;
        int samples = (int)(samplerate * 0.25f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samplerate;
            float pulse = Mathf.Sin(2f * Mathf.PI * 220f * t) * Mathf.Exp(-t * 18f);
            data[i] = Mathf.Clamp(pulse, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("SFX_CerrarDoc", samples, 1, samplerate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
