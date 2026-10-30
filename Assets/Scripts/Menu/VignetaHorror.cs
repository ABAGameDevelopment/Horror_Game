using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Efecto de viñeta oscura en las esquinas de la pantalla.
/// Reduce la intensidad en el centro para centrar la atención.
/// Se puede animar pulsando en código: PulsoMiedo(dur, intensidad).
/// </summary>
[RequireComponent(typeof(Image))]
public class VignetaHorror : MonoBehaviour
{
    [Header("Intensidad base")]
    [SerializeField] [Range(0f, 1f)] private float intensidadBase = 0.55f;

    [Header("Respiración lenta")]
    [SerializeField] private float velocidadRespiracion = 0.35f;
    [SerializeField] private float amplitudRespiracion  = 0.07f;

    private Image imagen;
    private Color colorBase;
    private bool  enPulso = false;

    private void Awake()
    {
        imagen     = GetComponent<Image>();
        colorBase  = new Color(0f, 0f, 0f, intensidadBase);
        imagen.color = colorBase;
    }

    public void Configurar(float intensidad, float velocidad = -1f, float amplitud = -1f)
    {
        intensidadBase = Mathf.Clamp01(intensidad);
        if (velocidad >= 0f) velocidadRespiracion = velocidad;
        if (amplitud >= 0f) amplitudRespiracion = amplitud;
        colorBase = new Color(0f, 0f, 0f, intensidadBase);

        if (imagen != null)
            imagen.color = colorBase;
    }

    private void Update()
    {
        if (enPulso) return;

        float a = intensidadBase +
                  Mathf.Sin(Time.time * velocidadRespiracion) * amplitudRespiracion;
        imagen.color = new Color(0f, 0f, 0f, Mathf.Clamp01(a));
    }

    // -----------------------------------------------------------------------
    /// <summary>
    /// Pulso de miedo: la viñeta se pone muy densa por un instante.
    /// </summary>
    public void PulsoMiedo(float duracion = 0.4f, float intensidadPico = 0.92f)
    {
        if (!enPulso)
            StartCoroutine(AnimarPulso(duracion, intensidadPico));
    }

    private System.Collections.IEnumerator AnimarPulso(float dur, float pico)
    {
        enPulso = true;
        float mitad = dur * 0.5f;
        float t = 0f;

        while (t < mitad)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(intensidadBase, pico, t / mitad);
            imagen.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }

        t = 0f;
        while (t < mitad)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(pico, intensidadBase, t / mitad);
            imagen.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }

        imagen.color = colorBase;
        enPulso = false;
    }
}
