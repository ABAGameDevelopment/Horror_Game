using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pantalla de carga con barra de progreso.
/// Se activa automáticamente cuando se hace una carga de escena asíncrona
/// a través de GameManager. Muestra el progreso y frases de atmósfera.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup grupoTotal;
    [SerializeField] private Slider      barraProgreso;
    [SerializeField] private TMP_Text    textoPorcentaje;
    [SerializeField] private TMP_Text    textoFrase;
    [SerializeField] private CanvasGroup grupoOverlayFinal;

    [Header("Frases de carga (ambiente de horror)")]
    [SerializeField] private string[] frases = new string[]
    {
        "Los pasillos tienen memoria...",
        "Algo te observa en la oscuridad.",
        "No todos los que entran, salen.",
        "El silencio es el primer aviso.",
        "Busca las llaves. Rápido.",
        "Las puertas guardan secretos.",
        "El edificio respira.",
    };

    [Header("Velocidad de cambio de frases")]
    [SerializeField] private float intervaloFrase = 2.8f;

    private int fraseActual = 0;

    // -----------------------------------------------------------------------
    private void Start()
    {
        if (barraProgreso) barraProgreso.value = 0f;
        if (textoPorcentaje) textoPorcentaje.text = "0%";
        if (textoFrase && frases.Length > 0) textoFrase.text = frases[0];

        StartCoroutine(CambiarFrases());
    }

    // -----------------------------------------------------------------------
    /// <summary>
    /// Llamado externamente para actualizar la barra.
    /// progreso: 0.0 – 1.0
    /// </summary>
    public void ActualizarProgreso(float progreso)
    {
        if (barraProgreso)   barraProgreso.value = progreso;
        if (textoPorcentaje) textoPorcentaje.text = $"{Mathf.RoundToInt(progreso * 100f)}%";
    }

    // -----------------------------------------------------------------------
    private IEnumerator CambiarFrases()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloFrase);
            fraseActual = (fraseActual + 1) % frases.Length;
            if (textoFrase) yield return StartCoroutine(FraseFade(frases[fraseActual]));
        }
    }

    private IEnumerator FraseFade(string nuevaFrase)
    {
        // Fade out
        float t = 0f;
        Color c = textoFrase.color;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            textoFrase.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, t / 0.3f));
            yield return null;
        }
        textoFrase.text  = nuevaFrase;
        textoFrase.color = new Color(c.r, c.g, c.b, 0f);

        // Fade in
        t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            textoFrase.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, t / 0.4f));
            yield return null;
        }
        textoFrase.color = c;
    }
}
