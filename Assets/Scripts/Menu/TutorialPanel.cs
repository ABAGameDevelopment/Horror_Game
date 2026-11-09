using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel de Tutorial / Instrucciones que aparece al inicio del Level1.
/// Muestra una serie de diapositivas con controles y objetivos.
/// Se puede omitir presionando cualquier tecla o el botón "Entendido".
/// Se integra con IntroNivel: espera a que la intro termine antes de aparecer.
/// </summary>
public class TutorialPanel : MonoBehaviour
{
    // -----------------------------------------------------------------------
    //  Estructura de una diapositiva
    // -----------------------------------------------------------------------
    [System.Serializable]
    public class DiapositivalTutorial
    {
        [TextArea] public string titulo;
        [TextArea] public string descripcion;
        public Sprite icono;
    }

    // -----------------------------------------------------------------------
    [Header("Referencias UI")]
    [SerializeField] private CanvasGroup grupoPanel;
    [SerializeField] private TMP_Text    textoTitulo;
    [SerializeField] private TMP_Text    textoDescripcion;
    [SerializeField] private Image       imagenIcono;
    [SerializeField] private TMP_Text    textoContador;    // "1 / 4"
    [SerializeField] private Button      botonSiguiente;
    [SerializeField] private Button      botonSaltar;
    [SerializeField] private TMP_Text    textoBotonSiguiente; // "Siguiente" / "Entendido"

    [Header("Diapositivas")]
    [SerializeField] private DiapositivalTutorial[] diapositivas = new DiapositivalTutorial[]
    {
        new DiapositivalTutorial
        {
            titulo      = "MOVERSE",
            descripcion = "Usa  W A S D  para desplazarte\npor el edificio.\nMueve el ratón para mirar alrededor."
        },
        new DiapositivalTutorial
        {
            titulo      = "SALTAR",
            descripcion = "Presiona  ESPACIO  para saltar.\nPuedes saltar hasta dos veces seguidas."
        },
        new DiapositivalTutorial
        {
            titulo      = "LINTERNA",
            descripcion = "Presiona  F  para encender o apagar\ntu linterna.\n¡La oscuridad es tu mayor enemigo!"
        },
        new DiapositivalTutorial
        {
            titulo      = "OBJETIVO",
            descripcion = "Busca las llaves de la salida\nescondidas por el edificio.\nEncuéntralas antes de que sea tarde..."
        }
    };

    [Header("Intro del nivel")]
    [SerializeField] private IntroNivel introNivel;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxPagina;

    // -----------------------------------------------------------------------
    private int diapositvaActual = 0;

    // -----------------------------------------------------------------------
    private void Start()
    {
        OcultarPanel();

        // Esperar a que la intro termine para mostrar el tutorial
        StartCoroutine(EsperarIntroYMostrar());

        if (botonSiguiente) botonSiguiente.onClick.AddListener(OnSiguiente);
        if (botonSaltar)    botonSaltar.onClick.AddListener(Cerrar);
    }

    // -----------------------------------------------------------------------
    private IEnumerator EsperarIntroYMostrar()
    {
        // Esperar que la intro del nivel termine
        while (introNivel != null && !introNivel.IntroduccionTerminada)
            yield return null;

        // Si ya vio el tutorial antes, no mostrarlo
        if (PlayerPrefs.GetInt("TutorialVisto", 0) == 1)
            yield break;

        yield return new WaitForSeconds(1.5f);
        MostrarDiapositiva(0);
        yield return StartCoroutine(FadePanel(0f, 1f, 0.5f));
    }

    // -----------------------------------------------------------------------
    private void OnSiguiente()
    {
        if (sfxPagina) sfxPagina.Play();

        if (diapositvaActual < diapositivas.Length - 1)
        {
            diapositvaActual++;
            MostrarDiapositiva(diapositvaActual);
        }
        else
        {
            Cerrar();
        }
    }

    // -----------------------------------------------------------------------
    private void MostrarDiapositiva(int index)
    {
        if (index < 0 || index >= diapositivas.Length) return;

        var d = diapositivas[index];
        if (textoTitulo)      textoTitulo.text      = d.titulo;
        if (textoDescripcion) textoDescripcion.text  = d.descripcion;
        if (imagenIcono)      imagenIcono.sprite     = d.icono;
        if (imagenIcono)      imagenIcono.gameObject.SetActive(d.icono != null);

        if (textoContador)
            textoContador.text = $"{index + 1} / {diapositivas.Length}";

        bool esUltima = (index == diapositivas.Length - 1);
        if (textoBotonSiguiente)
            textoBotonSiguiente.text = esUltima ? "¡Entendido!" : "Siguiente >";

        if (botonSaltar)
            botonSaltar.gameObject.SetActive(!esUltima);
    }

    // -----------------------------------------------------------------------
    private void Cerrar()
    {
        PlayerPrefs.SetInt("TutorialVisto", 1);
        PlayerPrefs.Save();
        StartCoroutine(FadePanel(1f, 0f, 0.4f,
            () => grupoPanel.gameObject.SetActive(false)));
    }

    // -----------------------------------------------------------------------
    private void OcultarPanel()
    {
        grupoPanel.alpha          = 0f;
        grupoPanel.interactable   = false;
        grupoPanel.blocksRaycasts = false;
    }

    private IEnumerator FadePanel(float desde, float hasta, float dur,
                                  System.Action onFinish = null)
    {
        grupoPanel.alpha = desde;
        if (hasta > 0)
        {
            grupoPanel.interactable   = true;
            grupoPanel.blocksRaycasts = true;
        }

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            grupoPanel.alpha = Mathf.Lerp(desde, hasta, t / dur);
            yield return null;
        }
        grupoPanel.alpha = hasta;

        if (hasta <= 0)
        {
            grupoPanel.interactable   = false;
            grupoPanel.blocksRaycasts = false;
        }

        onFinish?.Invoke();
    }
}
