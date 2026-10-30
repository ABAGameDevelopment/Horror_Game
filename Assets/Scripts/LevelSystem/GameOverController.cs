using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    private GameObject raiz;
    private TextMeshProUGUI textMotivo;
    private TextMeshProUGUI textPuntaje;
    private Button botonReintentar;

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
        Canvas canvas = RuntimeUI.CrearCanvas("PantallaGameOver_Nivel2", 61);
        raiz = canvas.gameObject;

        Image fondo = RuntimeUI.CrearPanel(canvas.transform, "Fondo", new Color(0.08f, 0f, 0f, 0.94f));
        RuntimeUI.Estirar(fondo.rectTransform, 0f, 0f, 0f, 0f);
        RuntimeUI.CrearVigneta(canvas.transform, 0.65f);

        // Panel con marco de terror sangriento
        Image panel = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "PanelGameOver", new Vector2(720f, 600f), RuntimeUI.ColorPanel);
        RuntimeUI.Anclar(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 600f));

        // Título imponente con efecto Glitch
        TextMeshProUGUI titulo = RuntimeUI.CrearTituloHorror(panel.transform, "TituloGameOver", "HAS MUERTO", 92f, RuntimeUI.ColorRojoBrillante);
        RuntimeUI.Anclar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(680f, 95f));

        // Subtítulo temático en rojo sangre
        TextMeshProUGUI subtitulo = RuntimeUI.CrearSubtituloHorror(panel.transform, "Subtitulo", "—   L  A     O  S  C  U  R  I  D  A  D     T  E     H  A     A  L  C  A  N  Z  A  D  O   —");
        RuntimeUI.Anclar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -125f), new Vector2(680f, 32f));

        // Motivo de la muerte
        textMotivo = RuntimeUI.CrearTexto(panel.transform, "Motivo", "Tu corazón se ha detenido en el estacionamiento.", 22f, TextAlignmentOptions.Center, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(textMotivo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -185f), new Vector2(640f, 50f));

        textPuntaje = RuntimeUI.CrearTexto(panel.transform, "Puntaje", "PUNTAJE FINAL       0000 PTS", 24f, TextAlignmentOptions.Center, RuntimeUI.ColorTextoTenue);
        textPuntaje.fontStyle = FontStyles.Bold;
        RuntimeUI.Anclar(textPuntaje.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -245f), new Vector2(640f, 34f));

        TextMeshProUGUI pista = RuntimeUI.CrearTexto(panel.transform, "Pista", "Las piezas recogidas y el progreso se han restablecido.", 16f, TextAlignmentOptions.Center, RuntimeUI.ColorTextoTenue);
        RuntimeUI.Anclar(pista.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(640f, 26f));

        // Botones de acción estilo Menú Principal
        botonReintentar = RuntimeUI.CrearBotonHorror(panel.transform, "BotonReintentar", "REINTENTAR", Reintentar);
        RuntimeUI.Anclar(botonReintentar.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -360f), new Vector2(440f, 60f));

        Button botonMenu = RuntimeUI.CrearBotonHorror(panel.transform, "BotonMenu", "SALIR AL MENÚ PRINCIPAL", IrAlMenu);
        RuntimeUI.Anclar(botonMenu.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -440f), new Vector2(440f, 60f));
    }

    private void AlCambiarEstado(EstadoNivel estado)
    {
        bool gameOver = estado == EstadoNivel.GameOver;
        if (raiz != null)
            raiz.SetActive(gameOver);

        if (!gameOver || LevelStateManager.Instance == null)
            return;

        textMotivo.text = string.IsNullOrEmpty(LevelStateManager.Instance.MotivoGameOver)
            ? "Tu corazón se ha detenido en el estacionamiento."
            : LevelStateManager.Instance.MotivoGameOver;

        textPuntaje.text = "PUNTAJE FINAL       " + LevelStateManager.Instance.Puntaje.ToString("0000") + " PTS";

        if (EventSystem.current != null && botonReintentar != null)
            EventSystem.current.SetSelectedGameObject(botonReintentar.gameObject);
    }

    private void Reintentar()
    {
        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.Reiniciar();
    }

    private void IrAlMenu()
    {
        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.IrAlMenu();
    }
}
