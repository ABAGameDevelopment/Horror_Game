
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjetivoUI : MonoBehaviour
{
    [Header("Introduccion")]
    [SerializeField] private IntroNivel introNivel;

    [Header("Panel central")]
    [SerializeField] private RectTransform panelNuevoObjetivo;
    [SerializeField] private CanvasGroup grupoNuevoObjetivo;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoDescripcion;

    [Header("Objetivo fijo")]
    [SerializeField] private RectTransform panelObjetivoHUD;
    [SerializeField] private CanvasGroup grupoObjetivoHUD;
    [SerializeField] private TMP_Text textoCasilla;
    [SerializeField] private TMP_Text textoObjetivo;

    [Header("Tiempos")]
    [SerializeField] private float duracionAparicion = 0.5f;
    [SerializeField] private float tiempoVisible = 2.5f;
    [SerializeField] private float duracionMovimiento = 1f;
    [SerializeField] private float velocidadTexto = 35f;

    [Header("Tutorial de linterna")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RectTransform panelTutorialLinterna;
    [SerializeField] private CanvasGroup grupoTutorialLinterna;
    [SerializeField] private TMP_Text textoTutorialLinterna;
    [SerializeField] private float duracionDesvanecerTutorial = 0.8f;

    private bool linternaEncendidaPorPrimeraVez;
    private Coroutine corrutinaDesvanecerTutorial;

    private Vector3 posicionInicial;
    private Vector3 escalaInicial;

    private void Start()
    {
        // Guardar la posicion y escala iniciales.
        posicionInicial = panelNuevoObjetivo.position;
        escalaInicial = panelNuevoObjetivo.localScale;

        // Preparar el mensaje central.
        grupoNuevoObjetivo.alpha = 0f;

        // Preparar el objetivo permanente.
        grupoObjetivoHUD.alpha = 0f;
        panelObjetivoHUD.gameObject.SetActive(false);

        // Configurar los textos.
        textoTitulo.text = "NUEVO OBJETIVO";
        textoDescripcion.text = "Busca las llaves de la salida";

        textoTitulo.maxVisibleCharacters = 0;
        textoDescripcion.maxVisibleCharacters = 0;

        textoCasilla.text = "\u25A1";
        textoObjetivo.text = "Busca las llaves de la salida";

        grupoTutorialLinterna.alpha = 0f;
        panelTutorialLinterna.gameObject.SetActive(false);

        if (playerController != null)
        {
            playerController.LinternaEncendidaPorPrimeraVez
                += AlEncenderLinterna;
        }

        StartCoroutine(SecuenciaObjetivo());
    }

    private IEnumerator SecuenciaObjetivo()
    {
        // Esperar a que termine la cinematica.
        while (introNivel != null &&
               !introNivel.IntroduccionTerminada)
        {
            yield return null;
        }

        // Asegurar que el panel comienza en el centro.
        panelNuevoObjetivo.position = posicionInicial;
        panelNuevoObjetivo.localScale = escalaInicial;

        // Aparecer gradualmente.
        yield return StartCoroutine(
            Desvanecer(
                grupoNuevoObjetivo,
                0f,
                1f,
                duracionAparicion
            )
        );

        // Escribir el titulo letra por letra.
        yield return StartCoroutine(
            EscribirGradualmente(textoTitulo)
        );

        // Escribir la descripcion letra por letra.
        yield return StartCoroutine(
            EscribirGradualmente(textoDescripcion)
        );

        // Mantener el mensaje visible.
        yield return new WaitForSeconds(tiempoVisible);

        // Activar el HUD para obtener su posicion real.
        panelObjetivoHUD.gameObject.SetActive(true);
        grupoObjetivoHUD.alpha = 0f;

        // Guardar el centro exacto del objetivo permanente.
        Vector3 posicionFinal = panelObjetivoHUD.position;

        // Mover el mensaje hacia el HUD.

        // Mover el mensaje hacia el HUD.
        float tiempo = 0f;
        Vector3 origen = panelNuevoObjetivo.position;
        Vector3 escalaFinal = escalaInicial * 0.65f;

        // El HUD comienza invisible.
        grupoObjetivoHUD.alpha = 0f;

        // Porcentaje del movimiento en el que comienza a desaparecer.
        float inicioDesvanecimiento = 0.75f;

        while (tiempo < duracionMovimiento)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracionMovimiento
            );

            float t = Mathf.SmoothStep(0f, 1f, progreso);

            // Desplazar y reducir el mensaje.
            panelNuevoObjetivo.position = Vector3.Lerp(
                origen,
                posicionFinal,
                t
            );

            panelNuevoObjetivo.localScale = Vector3.Lerp(
                escalaInicial,
                escalaFinal,
                t
            );

            // Desvanecer el mensaje durante el ultimo 25 %.
            if (progreso >= inicioDesvanecimiento)
            {
                float progresoDesvanecimiento =
                    (progreso - inicioDesvanecimiento)
                    / (1f - inicioDesvanecimiento);

                grupoNuevoObjetivo.alpha = Mathf.Lerp(
                    1f,
                    0f,
                    Mathf.Clamp01(progresoDesvanecimiento)
                );
            }

            // El objetivo fijo aparece al mismo tiempo.
            // Empieza al final del movimiento para evitar la superposicion.
            if (progreso >= inicioDesvanecimiento)
            {
                float progresoHUD =
                    (progreso - inicioDesvanecimiento)
                    / (1f - inicioDesvanecimiento);

                grupoObjetivoHUD.alpha = Mathf.Lerp(
                    0f,
                    1f,
                    Mathf.Clamp01(progresoHUD)
                );
            }

            yield return null;
        }

        // Finalizar las animaciones sin dejar restos visibles.
        panelNuevoObjetivo.position = posicionFinal;
        panelNuevoObjetivo.localScale = escalaFinal;

        grupoNuevoObjetivo.alpha = 0f;
        grupoObjetivoHUD.alpha = 1f;

        // Ocultar el mensaje central.
        panelNuevoObjetivo.gameObject.SetActive(false);

        // Mostrar la indicacion de la linterna al terminar el objetivo.
        MostrarTutorialLinterna();
    }

    private IEnumerator EscribirGradualmente(TMP_Text texto)
    {
        texto.ForceMeshUpdate();

        int totalCaracteres = texto.textInfo.characterCount;
        texto.maxVisibleCharacters = 0;

        float caracteresMostrados = 0f;

        while (texto.maxVisibleCharacters < totalCaracteres)
        {
            caracteresMostrados +=
                velocidadTexto * Time.deltaTime;

            texto.maxVisibleCharacters = Mathf.Min(
                Mathf.FloorToInt(caracteresMostrados),
                totalCaracteres
            );

            yield return null;
        }

        texto.maxVisibleCharacters = totalCaracteres;
    }

    private IEnumerator Desvanecer(
        CanvasGroup grupo,
        float inicio,
        float fin,
        float duracion)
    {
        grupo.alpha = inicio;

        if (duracion <= 0f)
        {
            grupo.alpha = fin;
            yield break;
        }

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);

            grupo.alpha = Mathf.Lerp(inicio, fin, t);

            yield return null;
        }

        grupo.alpha = fin;
    }


    private void MostrarTutorialLinterna()
    {
        // Elegir el texto segun el esquema de control activo.
        bool usaGamepad =
            Gamepad.current != null &&
            (Keyboard.current == null ||
            Gamepad.current.lastUpdateTime >
            Keyboard.current.lastUpdateTime);

        if (usaGamepad)
        {
            textoTutorialLinterna.text =
                "Presiona Y para encender / apagar la linterna";
        }
        else
        {
            textoTutorialLinterna.text =
                "Presiona F para encender / apagar la linterna";
        }

        panelTutorialLinterna.gameObject.SetActive(true);
        grupoTutorialLinterna.alpha = 0f;

        StartCoroutine(
            Desvanecer(
                grupoTutorialLinterna,
                0f,
                1f,
                duracionAparicion
            )
        );

        // Si ya encendio la linterna antes de que apareciera el tutorial,
        // mostrarlo y retirarlo suavemente.
        if (linternaEncendidaPorPrimeraVez)
        {
            AlEncenderLinterna();
        }
    }

    private void AlEncenderLinterna()
    {
        if (linternaEncendidaPorPrimeraVez)
            return;

        linternaEncendidaPorPrimeraVez = true;

        if (panelTutorialLinterna == null ||
            !panelTutorialLinterna.gameObject.activeInHierarchy)
            return;

        if (corrutinaDesvanecerTutorial != null)
            StopCoroutine(corrutinaDesvanecerTutorial);

        corrutinaDesvanecerTutorial =
            StartCoroutine(DesvanecerYOcultarTutorial());
    }

    private IEnumerator DesvanecerYOcultarTutorial()
    {
        yield return StartCoroutine(
            Desvanecer(
                grupoTutorialLinterna,
                grupoTutorialLinterna.alpha,
                0f,
                duracionDesvanecerTutorial
            )
        );

        panelTutorialLinterna.gameObject.SetActive(false);
        corrutinaDesvanecerTutorial = null;
    }

}
