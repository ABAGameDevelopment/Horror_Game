using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Muestra los créditos del equipo con scroll automático.
/// Puede abrirse desde el Menú Principal.
/// </summary>
public class CreditosController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RectTransform contenidoCreditos;
    [SerializeField] private float         velocidadScroll = 40f;  // px/seg
    [SerializeField] private float         pausaInicial    = 1.5f;

    [Header("Botón volver")]
    [SerializeField] private Button botonVolver;

    [Header("Texto de créditos (editable en Inspector)")]
    [SerializeField, TextArea(10, 30)]
    private string textoCreditos =
        "— HORROR GAME —\n\n" +
        "Desarrollo\n" +
        "Tu nombre aquí\n\n" +
        "Diseño de niveles\n" +
        "Tu nombre aquí\n\n" +
        "Programación\n" +
        "Tu nombre aquí\n\n" +
        "Arte 3D y materiales\n" +
        "Tu nombre aquí\n\n" +
        "Audio y música\n" +
        "Tu nombre aquí\n\n" +
        "Assets externos utilizados:\n" +
        "• Realistic Terrain Textures Lite\n" +
        "• Conifers [BOTD]\n" +
        "• House Interior Pack (nappin)\n" +
        "• TextMesh Pro\n\n" +
        "Motor gráfico: Unity 6\n\n" +
        "Universidad XXXX — 2026\n\n\n";

    private TMP_Text textoUI;
    private bool scrollActivo = false;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        textoUI = contenidoCreditos?.GetComponentInChildren<TMP_Text>();
        if (textoUI) textoUI.text = textoCreditos;
        if (botonVolver) botonVolver.onClick.AddListener(OnVolver);
    }

    private void OnEnable()
    {
        if (contenidoCreditos)
            contenidoCreditos.anchoredPosition = new Vector2(0f, -Screen.height);

        StopAllCoroutines();
        StartCoroutine(EsperarYScroll());
    }

    // -----------------------------------------------------------------------
    private IEnumerator EsperarYScroll()
    {
        scrollActivo = false;
        yield return new WaitForSeconds(pausaInicial);
        scrollActivo = true;
    }

    private void Update()
    {
        if (!scrollActivo || contenidoCreditos == null) return;

        contenidoCreditos.anchoredPosition +=
            Vector2.up * velocidadScroll * Time.deltaTime;
    }

    // -----------------------------------------------------------------------
    private void OnVolver()
    {
        // El MainMenuController maneja el cierre del panel
        var menu = FindFirstObjectByType<MainMenuController>();
        if (menu != null) menu.OnCerrarSubpanel();
    }
}
