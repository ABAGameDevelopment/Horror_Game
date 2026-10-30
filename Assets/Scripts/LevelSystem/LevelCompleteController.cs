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

        Image fondo = RuntimeUI.CrearPanel(canvas.transform, "Fondo", new Color(0f, 0f, 0f, 0.86f));
        RuntimeUI.Estirar(fondo.rectTransform, 0f, 0f, 0f, 0f);

        Image panel = RuntimeUI.CrearPanel(canvas.transform, "PanelVictoria", new Color(0.04f, 0.05f, 0.06f, 0.99f));
        RuntimeUI.Anclar(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 620f));

        TextMeshProUGUI titulo = RuntimeUI.CrearTexto(panel.transform, "Titulo", "NIVEL COMPLETADO", 44f, TextAlignmentOptions.Center, new Color32(57, 211, 83, 255));
        RuntimeUI.Anclar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(640f, 56f));

        TextMeshProUGUI subtitulo = RuntimeUI.CrearTexto(panel.transform, "Subtitulo", "Has escapado del estacionamiento.\nLa ciudad sigue en silencio...", 20f, TextAlignmentOptions.Center, new Color(0.78f, 0.8f, 0.84f, 1f));
        RuntimeUI.Anclar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -94f), new Vector2(640f, 70f));

        textPuntaje = RuntimeUI.CrearTexto(panel.transform, "Puntaje", "PUNTAJE   0000", 24f, TextAlignmentOptions.Center, Color.white);
        RuntimeUI.Anclar(textPuntaje.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -188f), new Vector2(640f, 32f));

        textTiempo = RuntimeUI.CrearTexto(panel.transform, "Tiempo", "TIEMPO   00:00", 22f, TextAlignmentOptions.Center, new Color(0.85f, 0.87f, 0.9f, 1f));
        RuntimeUI.Anclar(textTiempo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -226f), new Vector2(640f, 30f));

        textMejor = RuntimeUI.CrearTexto(panel.transform, "Mejor", "MEJOR PUNTAJE   0000", 18f, TextAlignmentOptions.Center, new Color(0.7f, 0.73f, 0.78f, 1f));
        RuntimeUI.Anclar(textMejor.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -264f), new Vector2(640f, 26f));

        textRecord = RuntimeUI.CrearTexto(panel.transform, "Record", "¡NUEVO RÉCORD!", 20f, TextAlignmentOptions.Center, new Color32(242, 199, 68, 255));
        RuntimeUI.Anclar(textRecord.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -296f), new Vector2(640f, 28f));

        textDesbloqueo = RuntimeUI.CrearTexto(panel.transform, "Desbloqueo", "Nivel 3 desbloqueado", 18f, TextAlignmentOptions.Center, new Color32(57, 211, 83, 255));
        RuntimeUI.Anclar(textDesbloqueo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(640f, 26f));

        botonSiguiente = RuntimeUI.CrearBoton(panel.transform, "BotonSiguiente", "SIGUIENTE NIVEL", IrAlSiguiente);
        RuntimeUI.Anclar(botonSiguiente.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -392f), new Vector2(420f, 56f));

        Button botonMenu = RuntimeUI.CrearBoton(panel.transform, "BotonMenu", "VOLVER AL MENÚ", IrAlMenu);
        RuntimeUI.Anclar(botonMenu.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -462f), new Vector2(420f, 56f));
    }

    private void AlCambiarEstado(EstadoNivel estado)
    {
        bool completado = estado == EstadoNivel.Completado;
        if (raiz != null)
            raiz.SetActive(completado);

        if (!completado || LevelStateManager.Instance == null)
            return;

        LevelStateManager gestor = LevelStateManager.Instance;
        textPuntaje.text = "PUNTAJE   " + gestor.Puntaje.ToString("0000");
        textTiempo.text = "TIEMPO   " + FormatearTiempo(gestor.TiempoJugado);
        textMejor.text = "MEJOR PUNTAJE   " + SaveManager.MejorPuntaje(gestor.NombreNivel).ToString("0000");
        textRecord.gameObject.SetActive(gestor.NuevoRecord);

        bool haySiguiente = !string.IsNullOrEmpty(gestor.SiguienteEscena) && Application.CanStreamedLevelBeLoaded(gestor.SiguienteEscena);
        botonSiguiente.gameObject.SetActive(haySiguiente);
        textDesbloqueo.gameObject.SetActive(haySiguiente);
        textDesbloqueo.text = haySiguiente ? "Nivel " + gestor.IndiceSiguienteNivel + " desbloqueado" : "Continuará...";

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
