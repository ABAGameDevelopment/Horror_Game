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

        Image fondo = RuntimeUI.CrearPanel(canvas.transform, "Fondo", new Color(0.03f, 0f, 0f, 0.9f));
        RuntimeUI.Estirar(fondo.rectTransform, 0f, 0f, 0f, 0f);

        Image panel = RuntimeUI.CrearPanel(canvas.transform, "PanelGameOver", new Color(0.05f, 0.02f, 0.02f, 0.99f));
        RuntimeUI.Anclar(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 540f));

        TextMeshProUGUI titulo = RuntimeUI.CrearTexto(panel.transform, "Titulo", "GAME OVER", 56f, TextAlignmentOptions.Center, new Color32(214, 24, 24, 255));
        RuntimeUI.Anclar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(620f, 70f));

        textMotivo = RuntimeUI.CrearTexto(panel.transform, "Motivo", "La oscuridad te ha alcanzado.", 20f, TextAlignmentOptions.Center, new Color(0.82f, 0.84f, 0.88f, 1f));
        RuntimeUI.Anclar(textMotivo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(620f, 60f));

        textPuntaje = RuntimeUI.CrearTexto(panel.transform, "Puntaje", "PUNTAJE   0000", 22f, TextAlignmentOptions.Center, new Color(0.85f, 0.87f, 0.9f, 1f));
        RuntimeUI.Anclar(textPuntaje.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -196f), new Vector2(620f, 30f));

        TextMeshProUGUI pista = RuntimeUI.CrearTexto(panel.transform, "Pista", "Los objetos y el progreso se reinician.", 16f, TextAlignmentOptions.Center, new Color(0.6f, 0.63f, 0.68f, 1f));
        RuntimeUI.Anclar(pista.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -234f), new Vector2(620f, 26f));

        botonReintentar = RuntimeUI.CrearBoton(panel.transform, "BotonReintentar", "REINTENTAR", Reintentar);
        RuntimeUI.Anclar(botonReintentar.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(420f, 58f));

        Button botonMenu = RuntimeUI.CrearBoton(panel.transform, "BotonMenu", "VOLVER AL MENÚ", IrAlMenu);
        RuntimeUI.Anclar(botonMenu.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -374f), new Vector2(420f, 58f));
    }

    private void AlCambiarEstado(EstadoNivel estado)
    {
        bool gameOver = estado == EstadoNivel.GameOver;
        if (raiz != null)
            raiz.SetActive(gameOver);

        if (!gameOver || LevelStateManager.Instance == null)
            return;

        textMotivo.text = string.IsNullOrEmpty(LevelStateManager.Instance.MotivoGameOver)
            ? "La oscuridad te ha alcanzado."
            : LevelStateManager.Instance.MotivoGameOver;

        textPuntaje.text = "PUNTAJE   " + LevelStateManager.Instance.Puntaje.ToString("0000");

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
