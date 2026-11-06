using System.Collections;
using System.Linq;
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
    [SerializeField] private PasosJugador pasosJugador;

    [Header("Screamer")]
    [SerializeField] private AudioSource audioScreamer;
    [SerializeField] private AudioClip sonidoScreamer;
    [SerializeField] private AudioSource audioMusicaScreamer;
    [SerializeField] private AudioClip musicaScreamer;
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
    [SerializeField] private string escenaMenuPrincipal = "MainMenu";
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

    public void CancelarYBloquearMuerte()
    {
        muerteIniciada = true;

        StopAllCoroutines();

        if (audioScreamer != null)
            audioScreamer.Stop();

        if (audioMusicaScreamer != null)
            audioMusicaScreamer.Stop();

        if (panelMuerte != null)
            panelMuerte.SetActive(false);

        if (panelOscuro != null)
        {
            Color col = panelOscuro.color;
            col.a = 0f;
            panelOscuro.color = col;
            panelOscuro.gameObject.SetActive(false);
        }

        enabled = false;
    }

    private IEnumerator SecuenciaMuerte(Transform monstruo)
    {
        // Guardar la transformacion original del monstruo.
        monstruoOriginalParent = monstruo.parent;
        posicionOriginalMonstruo = monstruo.position;
        rotacionOriginalMonstruo = monstruo.rotation;
        escalaOriginalMonstruo = monstruo.localScale;

        // Detener los sonidos de los pasos.
        if (pasosJugador != null)
            pasosJugador.DetenerPasos();

        // Detener audio previo del monstruo para que resalte el grito
        AudioSource audioMonstruo = monstruo.GetComponent<AudioSource>();
        if (audioMonstruo != null)
            audioMonstruo.Stop();

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

        // Asegurar referencia a la camara del jugador
        if (camaraJugador == null)
        {
            Camera cam = Camera.main;
            if (cam != null) camaraJugador = cam.transform;
            else if (playerController != null) camaraJugador = playerController.transform.Find("Camera");
        }

        // Colocar al monstruo delante de la camara.
        monstruo.SetParent(camaraJugador, false);

        monstruo.localPosition = posicionScreamer;
        monstruo.localRotation = Quaternion.Euler(0f, 180f, 0f);

        // Reproducir el sonido.
        if (audioScreamer != null && sonidoScreamer != null)
            audioScreamer.PlayOneShot(sonidoScreamer);
        if (audioMusicaScreamer != null && musicaScreamer != null)
            audioMusicaScreamer.PlayOneShot(musicaScreamer);

        // ILUMINACION INTENSA DIRECTA AL ROSTRO DEL MONSTRUO
        GameObject luzScreamerObj = new GameObject("LuzRostroScreamer");
        if (camaraJugador != null)
        {
            luzScreamerObj.transform.SetParent(camaraJugador, false);
            luzScreamerObj.transform.localPosition = new Vector3(0f, 0f, 0.15f);
            luzScreamerObj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            luzScreamerObj.transform.SetParent(monstruo, false);
            luzScreamerObj.transform.localPosition = new Vector3(0f, 1.4f, 0.5f);
        }

        Light luzSpot = luzScreamerObj.AddComponent<Light>();
        luzSpot.type = LightType.Spot;
        luzSpot.spotAngle = 85f;
        luzSpot.innerSpotAngle = 45f;
        luzSpot.color = new Color(1f, 0.96f, 0.92f); // Luz blanca brillante
        luzSpot.intensity = 8.5f;
        luzSpot.range = 6.0f;
        luzSpot.shadows = LightShadows.None;

        GameObject luzFillObj = new GameObject("LuzRellenoScreamer");
        luzFillObj.transform.SetParent(monstruo, false);
        luzFillObj.transform.localPosition = new Vector3(0f, 1.2f, 0.5f);
        Light luzFill = luzFillObj.AddComponent<Light>();
        luzFill.type = LightType.Point;
        luzFill.color = new Color(1f, 0.93f, 0.88f);
        luzFill.intensity = 6.0f;
        luzFill.range = 5.0f;
        luzFill.shadows = LightShadows.None;

        // SONIDO DEL SCREAMER ASEGURADO (EN 2D Y A MAXIMO VOLUMEN)
        if (sonidoScreamer == null)
        {
#if UNITY_EDITOR
            sonidoScreamer = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/Screamer.mp3");
#endif
            if (sonidoScreamer == null)
            {
                sonidoScreamer = Resources.FindObjectsOfTypeAll<AudioClip>().FirstOrDefault(c => c.name.ToLower().Contains("screamer"));
            }
        }

        if (audioScreamer == null)
        {
            if (camaraJugador != null)
                audioScreamer = camaraJugador.GetComponent<AudioSource>();
            if (audioScreamer == null)
                audioScreamer = gameObject.AddComponent<AudioSource>();
        }

        if (audioScreamer != null)
        {
            audioScreamer.spatialBlend = 0f; // 2D Stereo directo
            audioScreamer.volume = 1f;
            if (sonidoScreamer != null)
            {
                audioScreamer.PlayOneShot(sonidoScreamer);
            }
        }

        // Animacion del ataque.
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

        // Eliminar las luces del screamer antes de oscurecer
        if (luzScreamerObj != null) Destroy(luzScreamerObj);
        if (luzFillObj != null) Destroy(luzFillObj);

        // Comenzar a oscurecer la pantalla.
        yield return StartCoroutine(DesvanecerPantalla());

        // NIVEL 2: utilizar el sistema de Game Over.
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level2")
        {
            if (LevelStateManager.Instance != null)
            {
                LevelStateManager.Instance.GameOver("Has caído en la oscuridad.");
            }
            else
            {
                Debug.LogError("[MuerteJugador] No se encontró LevelStateManager en Nivel 2.");
            }
        }
        // NIVEL 3: utilizar su pantalla de muerte original.
        else
        {
            if (panelMuerte != null)
            {
                panelMuerte.SetActive(true);
            }
            else
            {
                Debug.LogError("[MuerteJugador] No se ha asignado PanelMuerte en Nivel 3.");
            }
        }

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
        string nombreEscena = escenaMenuPrincipal;
        if (string.IsNullOrEmpty(nombreEscena) || nombreEscena == "MenuPrincipal")
        {
            nombreEscena = "MainMenu";
        }
        SceneManager.LoadScene(nombreEscena);
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaReintentar);
    }
}
