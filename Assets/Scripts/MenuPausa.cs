
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [Header("Referencias del jugador")]
    [SerializeField] private PlayerController playerController;

    [Header("Paneles principales")]
    [SerializeField] private GameObject panelOscuroPausa;
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private GameObject panelOpciones;

    [Header("Paneles de opciones")]
    [SerializeField] private GameObject panelMusica;
    [SerializeField] private GameObject panelGraficos;
    [SerializeField] private GameObject panelControles;
    [SerializeField] private GameObject panelOtros;

    [Header("Escenas")]
    [SerializeField] private string escenaMenuPrincipal = "MenuPrincipal";

    private bool juegoPausado;
    private bool jugadorEstabaActivo;

    private void Start()
    {
        // El juego comienza sin estar pausado.
        Time.timeScale = 1f;
        juegoPausado = false;

        if (panelOscuroPausa != null)
            panelOscuroPausa.SetActive(false);

        if (panelPausa != null)
            panelPausa.SetActive(false);

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        OcultarPanelesOpciones();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Se ejecuta desde la acción Pause del Input System.
    public void OnPause(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (juegoPausado)
            Reanudar();
        else
            Pausar();
    }

    public void Pausar()
    {
        if (juegoPausado)
            return;

        juegoPausado = true;

        // Guardar el estado del controlador antes de desactivarlo.
        if (playerController != null)
        {
            jugadorEstabaActivo = playerController.enabled;
            playerController.enabled = false;
        }

        Time.timeScale = 0f;

        if (panelOscuroPausa != null)
            panelOscuroPausa.SetActive(true);

        if (panelPausa != null)
            panelPausa.SetActive(true);

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        OcultarPanelesOpciones();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        if (!juegoPausado)
            return;

        juegoPausado = false;

        Time.timeScale = 1f;

        if (panelOscuroPausa != null)
            panelOscuroPausa.SetActive(false);

        if (panelPausa != null)
            panelPausa.SetActive(false);

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        OcultarPanelesOpciones();

        // Recuperar el estado anterior del controlador.
        if (playerController != null)
            playerController.enabled = jugadorEstabaActivo;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void SalirAlMenuPrincipal()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(escenaMenuPrincipal);
    }

    // Abrir el menú de opciones.
    public void AbrirOpciones()
    {
        if (panelPausa != null)
            panelPausa.SetActive(false);

        if (panelOpciones != null)
            panelOpciones.SetActive(true);

        OcultarPanelesOpciones();
    }

    // Volver desde opciones al menú de pausa.
    public void VolverAlMenuPausa()
    {
        OcultarPanelesOpciones();

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        if (panelPausa != null)
            panelPausa.SetActive(true);
    }

    // Mostrar cada categoría de opciones.
    public void MostrarMusica()
    {
        OcultarPanelesOpciones();

        if (panelMusica != null)
            panelMusica.SetActive(true);
    }

    public void MostrarGraficos()
    {
        OcultarPanelesOpciones();

        if (panelGraficos != null)
            panelGraficos.SetActive(true);
    }

    public void MostrarControles()
    {
        OcultarPanelesOpciones();

        if (panelControles != null)
            panelControles.SetActive(true);
    }

    public void MostrarOtros()
    {
        OcultarPanelesOpciones();

        if (panelOtros != null)
            panelOtros.SetActive(true);
    }

    // Ocultar todas las categorías.
    private void OcultarPanelesOpciones()
    {
        if (panelMusica != null)
            panelMusica.SetActive(false);

        if (panelGraficos != null)
            panelGraficos.SetActive(false);

        if (panelControles != null)
            panelControles.SetActive(false);

        if (panelOtros != null)
            panelOtros.SetActive(false);
    }

    public void VolverAOpciones()
    {
        OcultarPanelesOpciones();

        if (panelOpciones != null)
            panelOpciones.SetActive(true);
    }
}
