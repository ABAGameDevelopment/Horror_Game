
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

    [Header("Notas del edificio")]
    [SerializeField] private int totalNotas = 3;
    [SerializeField] private GameObject[] notasEdificio;
    [SerializeField] private GameObject llaveSalida;

    [Header("Final del nivel")]
    [SerializeField] private NivelCompletado nivelCompletado;

    private int notasEncontradas;
    private bool llaveSalidaEncontrada;


    private bool linternaEncendidaPorPrimeraVez;
    private bool objetivoAuditorioActivado;
    private bool objetivoSalidaActivado;

    private Vector3 posicionInicial;
    private Vector3 escalaInicial;

    private void Start()
    {
        // Guardar posicion y escala iniciales.
        posicionInicial = panelNuevoObjetivo.position;
        escalaInicial = panelNuevoObjetivo.localScale;

        // Preparar el mensaje central.
        grupoNuevoObjetivo.alpha = 0f;

        // Preparar el objetivo permanente.
        grupoObjetivoHUD.alpha = 0f;
        panelObjetivoHUD.gameObject.SetActive(false);

        // Configurar los textos.
        textoTitulo.text = "NUEVO OBJETIVO";
        textoDescripcion.text = "Encuentra la llave y dirígete al auditorio.";

        textoTitulo.maxVisibleCharacters = 0;
        textoDescripcion.maxVisibleCharacters = 0;

        textoCasilla.text = "□";
        textoObjetivo.text = "Encuentra la llave y dirígete al auditorio.";

        // Preparar el tutorial de la linterna.
        grupoTutorialLinterna.alpha = 0f;
        panelTutorialLinterna.gameObject.SetActive(false);

        // Suscribirse al evento de encendido de la linterna.
        if (playerController != null)
        {
            playerController.LinternaEncendidaPorPrimeraVez
                += AlEncenderLinterna;
        }

        StartCoroutine(SecuenciaObjetivo());
    }

    private IEnumerator SecuenciaObjetivo()
    {
        // 1. Esperar a que termine la cinematica.
        while (introNivel != null &&
               !introNivel.IntroduccionTerminada)
        {
            yield return null;
        }

        // 2. Mostrar el tutorial y esperar a que encienda la linterna.
        yield return StartCoroutine(MostrarTutorialLinterna());

        // 3. Mostrar el nuevo objetivo.
        panelNuevoObjetivo.gameObject.SetActive(true);

        panelNuevoObjetivo.position = posicionInicial;
        panelNuevoObjetivo.localScale = escalaInicial;

        grupoNuevoObjetivo.alpha = 0f;

        textoTitulo.maxVisibleCharacters = 0;
        textoDescripcion.maxVisibleCharacters = 0;

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

        Vector3 posicionFinal = panelObjetivoHUD.position;

        // Preparar el movimiento.
        float tiempo = 0f;

        Vector3 origen = panelNuevoObjetivo.position;
        Vector3 escalaFinal = escalaInicial * 0.65f;

        float inicioDesvanecimiento = 0.75f;

        // Mover el mensaje hacia el HUD.
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
                    (progreso - inicioDesvanecimiento) /
                    (1f - inicioDesvanecimiento);

                grupoNuevoObjetivo.alpha = Mathf.Lerp(
                    1f,
                    0f,
                    Mathf.Clamp01(progresoDesvanecimiento)
                );
            }

            // Mostrar el objetivo fijo al mismo tiempo.
            if (progreso >= inicioDesvanecimiento)
            {
                float progresoHUD =
                    (progreso - inicioDesvanecimiento) /
                    (1f - inicioDesvanecimiento);

                grupoObjetivoHUD.alpha = Mathf.Lerp(
                    0f,
                    1f,
                    Mathf.Clamp01(progresoHUD)
                );
            }

            yield return null;
        }

        // Finalizar las animaciones.
        panelNuevoObjetivo.position = posicionFinal;
        panelNuevoObjetivo.localScale = escalaFinal;

        grupoNuevoObjetivo.alpha = 0f;
        grupoObjetivoHUD.alpha = 1f;

        // Ocultar el mensaje central.
        panelNuevoObjetivo.gameObject.SetActive(false);
    }

    private IEnumerator MostrarTutorialLinterna()
    {
        // Elegir el texto segun el control utilizado.
        bool usaGamepad =
            Gamepad.current != null &&
            (Keyboard.current == null ||
             Gamepad.current.lastUpdateTime >
             Keyboard.current.lastUpdateTime);

        if (usaGamepad)
        {
            textoTutorialLinterna.text =
                "Presiona Y para encender la linterna";
        }
        else
        {
            textoTutorialLinterna.text =
                "Presiona F para encender la linterna";
        }

        // Mostrar el panel.
        panelTutorialLinterna.gameObject.SetActive(true);
        grupoTutorialLinterna.alpha = 0f;

        // Aparecer gradualmente.
        yield return StartCoroutine(
            Desvanecer(
                grupoTutorialLinterna,
                0f,
                1f,
                duracionAparicion
            )
        );

        // Esperar hasta que el jugador encienda la linterna.
        // Si ya la encendio antes, no se queda esperando.
        while (!linternaEncendidaPorPrimeraVez)
        {
            yield return null;
        }

        // Desaparecer gradualmente.
        yield return StartCoroutine(
            Desvanecer(
                grupoTutorialLinterna,
                grupoTutorialLinterna.alpha,
                0f,
                duracionDesvanecerTutorial
            )
        );

        panelTutorialLinterna.gameObject.SetActive(false);
    }

    private void AlEncenderLinterna()
    {
        // Registrar que la linterna ya fue encendida.
        linternaEncendidaPorPrimeraVez = true;
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

    private void OnDestroy()
    {
        // Quitar la suscripcion al evento.
        if (playerController != null)
        {
            playerController.LinternaEncendidaPorPrimeraVez
                -= AlEncenderLinterna;
        }
    }

    public void CompletarObjetivoAuditorio()
    {
        // Evitar activar el objetivo varias veces.
        if (objetivoAuditorioActivado)
            return;

        objetivoAuditorioActivado = true;

        // Activar las notas del auditorio.
        foreach (GameObject nota in notasEdificio)
        {
            if (nota != null)
            {
                nota.SetActive(true);
            }
        }

        Debug.Log("Auditorio abierto. Las notas ya están disponibles.");

        // Mostrar el nuevo objetivo.
        StartCoroutine(MostrarNuevoObjetivoAuditorio());
    }

    private IEnumerator MostrarNuevoObjetivoAuditorio()
    {
        // Preparar el mensaje central.
        panelNuevoObjetivo.gameObject.SetActive(true);

        panelNuevoObjetivo.position = posicionInicial;
        panelNuevoObjetivo.localScale = escalaInicial;

        grupoNuevoObjetivo.alpha = 0f;

        textoTitulo.text = "NUEVO OBJETIVO";
        textoDescripcion.text = "Encuentra las 3 notas del Edificio";

        textoTitulo.maxVisibleCharacters = 0;
        textoDescripcion.maxVisibleCharacters = 0;

        // Mostrar el mensaje.
        yield return StartCoroutine(
            Desvanecer(
                grupoNuevoObjetivo,
                0f,
                1f,
                duracionAparicion
            )
        );

        yield return StartCoroutine(
            EscribirGradualmente(textoTitulo)
        );

        yield return StartCoroutine(
            EscribirGradualmente(textoDescripcion)
        );

        yield return new WaitForSeconds(tiempoVisible);

        // Obtener la posicion del HUD.
        Vector3 posicionFinal = panelObjetivoHUD.position;

        Vector3 origen = panelNuevoObjetivo.position;
        Vector3 escalaFinal = escalaInicial * 0.65f;

        float tiempo = 0f;
        float inicioDesvanecimiento = 0.75f;

        // Desplazar el mensaje hacia el HUD.
        while (tiempo < duracionMovimiento)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracionMovimiento
            );

            float t = Mathf.SmoothStep(0f, 1f, progreso);

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

            if (progreso >= inicioDesvanecimiento)
            {
                float progresoDesvanecimiento =
                    (progreso - inicioDesvanecimiento) /
                    (1f - inicioDesvanecimiento);

                grupoNuevoObjetivo.alpha = Mathf.Lerp(
                    1f,
                    0f,
                    progresoDesvanecimiento
                );
            }

            yield return null;
        }

        // Finalizar animacion.
        panelNuevoObjetivo.position = posicionFinal;
        panelNuevoObjetivo.localScale = escalaFinal;

        grupoNuevoObjetivo.alpha = 0f;

        panelNuevoObjetivo.gameObject.SetActive(false);

        // Actualizar el HUD:
        // primer objetivo completado y segundo objetivo pendiente.
        textoCasilla.text = "-\n□";

        textoObjetivo.text =
            "Encuentra la llave y dirígete al auditorio.\n" +
            "Encuentra las 3 notas del Edificio";
    }

    private IEnumerator MostrarNuevoObjetivoSalida()
    {
        // Preparar el mensaje central.
        panelNuevoObjetivo.gameObject.SetActive(true);

        panelNuevoObjetivo.position = posicionInicial;
        panelNuevoObjetivo.localScale = escalaInicial;

        grupoNuevoObjetivo.alpha = 0f;

        textoTitulo.text = "NUEVO OBJETIVO";
        textoDescripcion.text = "Encuentra la llave de la salida y escapa del edificio.";

        textoTitulo.maxVisibleCharacters = 0;
        textoDescripcion.maxVisibleCharacters = 0;

        // Mostrar el mensaje.
        yield return StartCoroutine(
            Desvanecer(
                grupoNuevoObjetivo,
                0f,
                1f,
                duracionAparicion
            )
        );

        yield return StartCoroutine(
            EscribirGradualmente(textoTitulo)
        );

        yield return StartCoroutine(
            EscribirGradualmente(textoDescripcion)
        );

        yield return new WaitForSeconds(tiempoVisible);

        // Obtener la posición del HUD.
        Vector3 posicionFinal = panelObjetivoHUD.position;

        Vector3 origen = panelNuevoObjetivo.position;
        Vector3 escalaFinal = escalaInicial * 0.65f;

        float tiempo = 0f;
        float inicioDesvanecimiento = 0.75f;

        // Mover el mensaje hacia el HUD.
        while (tiempo < duracionMovimiento)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracionMovimiento
            );

            float t = Mathf.SmoothStep(0f, 1f, progreso);

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

            if (progreso >= inicioDesvanecimiento)
            {
                float progresoDesvanecimiento =
                    (progreso - inicioDesvanecimiento) /
                    (1f - inicioDesvanecimiento);

                grupoNuevoObjetivo.alpha = Mathf.Lerp(
                    1f,
                    0f,
                    progresoDesvanecimiento
                );
            }

            yield return null;
        }

        // Finalizar animación.
        panelNuevoObjetivo.position = posicionFinal;
        panelNuevoObjetivo.localScale = escalaFinal;

        grupoNuevoObjetivo.alpha = 0f;

        panelNuevoObjetivo.gameObject.SetActive(false);

        // Actualizar el HUD con el nuevo objetivo.
        textoCasilla.text = "-\n-\n□";

        textoObjetivo.text =
            "Encuentra la llave y dirígete al auditorio.\n" +
            "Encuentra las 3 notas del edificio (3/3).\n" +
            "Encuentra la llave de la salida y escapa del edificio.";
    }

    public void RegistrarNotaEncontrada()
    {
        if (!objetivoAuditorioActivado)
            return;

        if (notasEncontradas >= totalNotas)
            return;

        notasEncontradas++;

        if (notasEncontradas >= totalNotas)
        {
            notasEncontradas = totalNotas;

            textoCasilla.text = "-\n-";

            textoObjetivo.text =
                "Encuentra la llave y dirígete al auditorio.\n" +
                "Encuentra las 3 notas del edificio (3/3)";

            Debug.Log("¡Has encontrado las tres notas!");

            // Activar la llave de salida.
            if (llaveSalida != null)
            {
                llaveSalida.SetActive(true);
            }

            // Activar el siguiente objetivo.
            if (!objetivoSalidaActivado)
            {
                objetivoSalidaActivado = true;
                StartCoroutine(MostrarNuevoObjetivoSalida());
            }

        }
        else
        {
            textoCasilla.text = "-\n□";

            textoObjetivo.text =
                "Encuentra la llave y dirígete al auditorio.\n" +
                $"Encuentra las 3 notas del edificio ({notasEncontradas}/{totalNotas})";
        }
    }

    public void RegistrarLlaveSalidaEncontrada()
    {
        if (llaveSalidaEncontrada)
            return;

        // Evitar registrar la llave antes de encontrar las notas.
        if (notasEncontradas < totalNotas)
            return;

        llaveSalidaEncontrada = true;

        // Actualizar el HUD.
        textoCasilla.text = "-\n-\n□";

        textoObjetivo.text =
            "Encuentra la llave y dirígete al auditorio.\n" +
            $"Encuentra las 3 notas del edificio ({totalNotas}/{totalNotas}).\n" +
            "Sal del edificio.";

        Debug.Log("¡Has encontrado la llave de salida!");
    }

    public void CompletarObjetivoFinal()
    {
        Debug.Log("¡El jugador ha escapado del edificio!");

        if (nivelCompletado != null)
        {
            nivelCompletado.IniciarFinal();
        }
    }
}
