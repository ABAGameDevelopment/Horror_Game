using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompleteController : MonoBehaviour
{
    private GameObject raiz;
    private TextMeshProUGUI textPuntaje;
    private TextMeshProUGUI textTiempo;
    private TextMeshProUGUI textMejor;
    private TextMeshProUGUI textRecord;
    private TextMeshProUGUI textRango;
    private TextMeshProUGUI textDesbloqueo;
    private Button botonSiguiente;

    private void Start()
    {
        RuntimeUI.AsegurarEventSystem();
        Construir();
        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.EstadoCambiado += AlCambiarEstado;
        raiz.SetActive(false);
    }

    private void OnDestroy()
    {
        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.EstadoCambiado -= AlCambiarEstado;
    }

    private void Construir()
    {
        Canvas canvas = RuntimeUI.CrearCanvas("PantallaVictoria_Nivel2", 60);
        raiz = canvas.gameObject;

        Image fondo = RuntimeUI.CrearPanel(canvas.transform, "Fondo", new Color(0f, 0f, 0f, 0.90f));
        RuntimeUI.Estirar(fondo.rectTransform, 0f, 0f, 0f, 0f);
        RuntimeUI.CrearVigneta(canvas.transform, 0.55f);

        // Panel central con marco sangriento de terror
        Image panel = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "PanelVictoria", new Vector2(760f, 660f), RuntimeUI.ColorPanel);
        RuntimeUI.Anclar(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 660f));

        // Título imponente con efecto Glitch
        TextMeshProUGUI titulo = RuntimeUI.CrearTituloHorror(panel.transform, "TituloVictoria", "¡HAS ESCAPADO!", 80f, RuntimeUI.ColorTitulo);
        RuntimeUI.Anclar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(720f, 85f));

        // Subtítulo temático en rojo sangre
        TextMeshProUGUI subtitulo = RuntimeUI.CrearSubtituloHorror(panel.transform, "Subtitulo", "—   S  O  B  R  E  V  I  V  I  S  T  E       A     L  A     N  O  C  H  E   —");
        RuntimeUI.Anclar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(720f, 32f));

        // Tarjeta interior de estadísticas
        Image tarjeta = RuntimeUI.CrearPanel(panel.transform, "TarjetaStats", new Color(0.02f, 0.02f, 0.02f, 0.88f));
        RuntimeUI.Anclar(tarjeta.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -165f), new Vector2(680f, 250f));

        textPuntaje = RuntimeUI.CrearTexto(tarjeta.transform, "Puntaje", "PUNTAJE FINAL       0000 PTS", 24f, TextAlignmentOptions.Center, RuntimeUI.ColorTexto);
        textPuntaje.fontStyle = FontStyles.Bold;
        RuntimeUI.Anclar(textPuntaje.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(640f, 32f));

        textTiempo = RuntimeUI.CrearTexto(tarjeta.transform, "Tiempo", "TIEMPO DE ESCAPE    00:00", 22f, TextAlignmentOptions.Center, RuntimeUI.ColorTextoTenue);
        RuntimeUI.Anclar(textTiempo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -66f), new Vector2(640f, 30f));

        textMejor = RuntimeUI.CrearTexto(tarjeta.transform, "Mejor", "MEJOR RÉCORD        0000 PTS", 20f, TextAlignmentOptions.Center, RuntimeUI.ColorTextoTenue);
        RuntimeUI.Anclar(textMejor.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -108f), new Vector2(640f, 28f));

        textRango = RuntimeUI.CrearTexto(tarjeta.transform, "Rango", "CALIFICACIÓN DE SUPERVIVENCIA: RANGO S", 22f, TextAlignmentOptions.Center, RuntimeUI.ColorAviso);
        textRango.fontStyle = FontStyles.Bold;
        RuntimeUI.Anclar(textRango.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -152f), new Vector2(640f, 30f));

        textRecord = RuntimeUI.CrearTexto(tarjeta.transform, "Record", "★ ¡NUEVO RÉCORD DE PUNTUACIÓN! ★", 20f, TextAlignmentOptions.Center, RuntimeUI.ColorRojoBrillante);
        textRecord.fontStyle = FontStyles.Bold;
        RuntimeUI.Anclar(textRecord.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -192f), new Vector2(640f, 28f));

        textDesbloqueo = RuntimeUI.CrearTexto(panel.transform, "Desbloqueo", "Siguiente nivel desbloqueado", 18f, TextAlignmentOptions.Center, RuntimeUI.ColorExito);
        RuntimeUI.Anclar(textDesbloqueo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -435f), new Vector2(680f, 26f));

        // Botones de acción
        botonSiguiente = RuntimeUI.CrearBotonHorror(panel.transform, "BotonSiguiente", "SIGUIENTE NIVEL", IrAlSiguiente);
        RuntimeUI.Anclar(botonSiguiente.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -480f), new Vector2(440f, 58f));

        Button botonMenu = RuntimeUI.CrearBotonHorror(panel.transform, "BotonMenu", "SALIR AL MENÚ PRINCIPAL", IrAlMenu);
        RuntimeUI.Anclar(botonMenu.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -555f), new Vector2(440f, 58f));
    }

    private void AlCambiarEstado(EstadoNivel estado)
    {
        bool completado = estado == EstadoNivel.Completado;
        if (raiz != null)
            raiz.SetActive(completado);

        if (!completado || LevelStateManager.Instance == null)
            return;

        LevelStateManager gestor = LevelStateManager.Instance;
        textPuntaje.text = "PUNTAJE FINAL       " + gestor.Puntaje.ToString("0000") + " PTS";
        textTiempo.text = "TIEMPO DE ESCAPE    " + FormatearTiempo(gestor.TiempoJugado);
        textMejor.text = "MEJOR RÉCORD        " + SaveManager.MejorPuntaje(gestor.NombreNivel).ToString("0000") + " PTS";
        textRecord.gameObject.SetActive(gestor.NuevoRecord);

        // Cálculo de rango
        string rango = "C";
        if (gestor.Puntaje >= 2000 && gestor.TiempoJugado < 180f) rango = "S";
        else if (gestor.Puntaje >= 1200) rango = "A";
        else if (gestor.Puntaje >= 600) rango = "B";
        textRango.text = "CALIFICACIÓN DE SUPERVIVENCIA: RANGO " + rango;

        bool haySiguiente = !string.IsNullOrEmpty(gestor.SiguienteEscena) && Application.CanStreamedLevelBeLoaded(gestor.SiguienteEscena);
        botonSiguiente.gameObject.SetActive(haySiguiente);
        textDesbloqueo.gameObject.SetActive(haySiguiente);
        textDesbloqueo.text = haySiguiente ? "Nivel " + gestor.IndiceSiguienteNivel + " desbloqueado" : "¡Has completado todos los niveles disponibles!";

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(haySiguiente ? botonSiguiente.gameObject : null);
    }

    private void IrAlSiguiente()
    {
        if (LevelStateManager.Instance == null)
            return;

        string siguiente = LevelStateManager.Instance.SiguienteEscena;
        if (!string.IsNullOrEmpty(siguiente) && Application.CanStreamedLevelBeLoaded(siguiente))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(siguiente);
        }
    }

    private void IrAlMenu()
    {
        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.IrAlMenu();
    }

    private static string FormatearTiempo(float segundos)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(segundos));
        return string.Format("{0:00}:{1:00}", total / 60, total % 60);
    }
}
