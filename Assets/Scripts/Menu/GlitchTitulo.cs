using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Aplica el efecto de glitch de texto en el título del juego.
/// El texto parpadea con pequeños desplazamientos de color para
/// conseguir el estilo de terror / VHS.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class GlitchTitulo : MonoBehaviour
{
    [Header("Glitch")]
    [SerializeField] private float probabilidadGlitch  = 0.04f;  // por frame
    [SerializeField] private float duracionGlitch      = 0.08f;
    [SerializeField] private float desplazamientoMax   = 6f;
    [SerializeField] private Color colorGlitch1        = new Color(1f, 0f, 0.1f, 0.8f);
    [SerializeField] private Color colorGlitch2        = new Color(0.1f, 0f, 1f, 0.8f);

    private TMP_Text texto;
    private Color    colorOriginal;
    private Vector3  posOriginal;

    private void Awake()
    {
        texto         = GetComponent<TMP_Text>();
        colorOriginal = texto.color;
        posOriginal   = transform.localPosition;
    }

    private void Update()
    {
        if (Random.value < probabilidadGlitch)
            StartCoroutine(RealizarGlitch());
    }

    private IEnumerator RealizarGlitch()
    {
        // Primer flash de color
        texto.color = colorGlitch1;
        transform.localPosition = posOriginal +
            new Vector3(Random.Range(-desplazamientoMax, desplazamientoMax), 0, 0);
        yield return new WaitForSeconds(duracionGlitch * 0.4f);

        // Segundo flash
        texto.color = colorGlitch2;
        transform.localPosition = posOriginal +
            new Vector3(Random.Range(-desplazamientoMax * 0.5f, desplazamientoMax * 0.5f), 0, 0);
        yield return new WaitForSeconds(duracionGlitch * 0.3f);

        // Restaurar
        texto.color             = colorOriginal;
        transform.localPosition = posOriginal;
    }
}
