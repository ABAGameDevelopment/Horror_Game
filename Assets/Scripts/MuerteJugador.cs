
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MuerteJugador : MonoBehaviour
{
    [Header("Referencias del jugador")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform camaraJugador;

    [Header("Screamer")]
    [SerializeField] private AudioSource audioScreamer;
    [SerializeField] private AudioClip sonidoScreamer;
    [SerializeField] private float duracionScreamer = 2.5f;

    [SerializeField]
    private Vector3 posicionScreamer =
        new Vector3(0f, -0.1f, 1.1f);

    [SerializeField] private float distanciaAtaque = 0.55f;
    [SerializeField] private float velocidadAtaque = 8f;

    [Header("Pantalla de muerte")]
    [SerializeField] private Image panelOscuro;
    [SerializeField] private GameObject panelMuerte;

    [Header("Transicion")]
    [SerializeField] private float duracionDesvanecimiento = 1.5f;

    [Header("Escenas")]
    [SerializeField] private string escenaMenuPrincipal = "MenuPrincipal";
    [SerializeField] private string escenaReintentar = "Nivel1";

    private bool muerteIniciada;
    private Transform monstruoOriginalParent;

    private Vector3 posicionOriginalMonstruo;
    private Quaternion rotacionOriginalMonstruo;
    private Vector3 escalaOriginalMonstruo;

    private void Start()
    {
        if (panelOscuro != null)
        {
            Color color = panelOscuro.color;
            color.a = 0f;
            panelOscuro.color = color;

            panelOscuro.raycastTarget = false;
        }

        if (panelMuerte != null)
            panelMuerte.SetActive(false);
    }

    public void IniciarMuerte(Transform monstruo)
    {
        if (muerteIniciada || monstruo == null)
            return;

        muerteIniciada = true;

        StartCoroutine(SecuenciaMuerte(monstruo));
    }

    private IEnumerator SecuenciaMuerte(Transform monstruo)
    {
        // Guardar la transformación original del monstruo.
        monstruoOriginalParent = monstruo.parent;
        posicionOriginalMonstruo = monstruo.position;
        rotacionOriginalMonstruo = monstruo.rotation;
        escalaOriginalMonstruo = monstruo.localScale;

        // Bloquear los controles del jugador.
        if (playerInput != null)
            playerInput.enabled = false;

        if (playerController != null)
            playerController.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        // Desactivar colisiones del monstruo durante el screamer.
        Collider[] colliders = monstruo.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
            col.enabled = false;

        // Colocar al monstruo delante de la cámara.
        monstruo.SetParent(camaraJugador, false);

        monstruo.localPosition = posicionScreamer;
        monstruo.localRotation = Quaternion.Euler(0f, 180f, 0f);

        // Reproducir el sonido.
        if (audioScreamer != null && sonidoScreamer != null)
            audioScreamer.PlayOneShot(sonidoScreamer);

        // Animación del ataque.
        float tiempo = 0f;

        Vector3 posicionInicial = posicionScreamer;
        Vector3 posicionFinal = posicionScreamer +
            new Vector3(0f, 0f, -distanciaAtaque);

        while (tiempo < duracionScreamer)
        {
            tiempo += Time.deltaTime;

            float progreso = tiempo / duracionScreamer;

            // El monstruo se acerca y retrocede.
            float movimiento = Mathf.Sin(progreso * Mathf.PI * 3f);

            float intensidad = Mathf.Lerp(
                0f, 1f, Mathf.Clamp01(progreso * velocidadAtaque)
            );

            monstruo.localPosition = Vector3.Lerp(
                posicionInicial,
                posicionFinal,
                Mathf.Max(0f, movimiento) * intensidad
            );

            yield return null;
        }

        // Comenzar a oscurecer la pantalla.
        yield return StartCoroutine(DesvanecerPantalla());

        // Mostrar pantalla de derrota.
        if (panelMuerte != null)
            panelMuerte.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator DesvanecerPantalla()
    {
        if (panelOscuro == null)
        {
            Debug.LogError("No se ha asignado PanelOscuro.");
            yield break;
        }

        panelOscuro.gameObject.SetActive(true);
        panelOscuro.raycastTarget = true;

        Color color = panelOscuro.color;
        color.a = 0f;
        panelOscuro.color = color;

        float tiempo = 0f;

        while (tiempo < duracionDesvanecimiento)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracionDesvanecimiento
            );

            color.a = Mathf.SmoothStep(0f, 1f, progreso);
            panelOscuro.color = color;

            yield return null;
        }

        color.a = 1f;
        panelOscuro.color = color;
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenuPrincipal);
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaReintentar);
    }
}
