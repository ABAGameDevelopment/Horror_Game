using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CarEscapeController : MonoBehaviour
{
    public static CarEscapeController Instance { get; private set; }

    [Header("Progreso de Recoleccion de Items")]
    [SerializeField] private bool tieneBateria = false;
    [SerializeField] private bool tieneAceite = false;
    [SerializeField] private bool tieneGasolina = false;
    [SerializeField] private bool tieneLlaves = false;

    [Header("Instalacion en el Vehiculo")]
    [SerializeField] private bool cofreAbierto = false;
    [SerializeField] private bool bateriaInstalada = false;
    [SerializeField] private bool aceiteLlenado = false;
    [SerializeField] private bool gasolinaCargada = false;

    [Header("Referencias del Cofre y Motor")]
    [SerializeField] private Transform pivoteCofre;
    [SerializeField] private GameObject modeloBateriaMotor;
    [SerializeField] private Transform portonSalida;
    [SerializeField] private float velocidadEscape = 22f;

    // Estado interno
    private bool animandoCofre = false;
    private bool escapeIniciado = false;
    private Transform playerTransform;
    private AudioSource audioSource;

    // UI Minimalista
    private string promptActual = "";
    private string mensajeFlotante = "";
    private float tiempoMensaje = 0f;

    public bool CofreAbierto => cofreAbierto;
    public bool EstaListoParaArrancar => bateriaInstalada && aceiteLlenado && gasolinaCargada && tieneLlaves;

    public bool TieneBateria => tieneBateria;
    public bool TieneAceite => tieneAceite;
    public bool TieneGasolina => tieneGasolina;
    public bool TieneLlaves => tieneLlaves;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.5f;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null) playerTransform = player.transform;

        if (portonSalida == null)
        {
            GameObject gateGo = GameObject.Find("Porton_Salida_Estacionamiento");
            if (gateGo != null) portonSalida = gateGo.transform;
        }

        AsegurarComponentesCofreYMotor();
    }

    private bool TeclaInteraccionPresionada()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame ||
                Keyboard.current.fKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame)
            {
                return true;
            }
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Crea proceduralmente el cofre articulado y el compartimento de motor si no existen en la escena
    /// </summary>
    public void AsegurarComponentesCofreYMotor()
    {
        if (pivoteCofre == null)
        {
            Transform existingPivot = transform.Find("Pivote_Cofre");
            if (existingPivot != null)
            {
                pivoteCofre = existingPivot;
            }
            else
            {
                // Crear el punto de pivote en la base del parabrisas
                GameObject pivotGo = new GameObject("Pivote_Cofre");
                pivotGo.transform.SetParent(transform, false);
                pivotGo.transform.localPosition = new Vector3(0f, 0.82f, 0.55f);
                pivoteCofre = pivotGo.transform;

                // Tapa del cofre
                GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lid.name = "Tapa_Cofre";
                lid.transform.SetParent(pivoteCofre, false);
                lid.transform.localPosition = new Vector3(0f, -0.05f, 0.72f);
                lid.transform.localRotation = Quaternion.Euler(-5f, 0f, 0f);
                lid.transform.localScale = new Vector3(1.68f, 0.07f, 1.45f);

                // Asignar material del auto si está disponible
                Renderer carRend = GetComponentInChildren<Renderer>();
                if (carRend != null && carRend.sharedMaterial != null)
                {
                    lid.GetComponent<Renderer>().sharedMaterial = carRend.sharedMaterial;
                }
                else
                {
                    lid.GetComponent<Renderer>().material.color = new Color(0.85f, 0.12f, 0.1f);
                }

                Collider lidCol = lid.GetComponent<Collider>();
                if (lidCol != null) Destroy(lidCol); // Evitar bloquear paso
            }
        }

        // Compartimento de motor debajo del cofre
        Transform existingEngine = transform.Find("Compartimento_Motor");
        if (existingEngine == null)
        {
            GameObject engineBay = new GameObject("Compartimento_Motor");
            engineBay.transform.SetParent(transform, false);
            engineBay.transform.localPosition = new Vector3(0f, 0.62f, 1.25f);

            // Bloque del motor V8 central
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "Bloque_Motor";
            block.transform.SetParent(engineBay.transform, false);
            block.transform.localPosition = new Vector3(0f, 0f, 0f);
            block.transform.localScale = new Vector3(0.72f, 0.42f, 0.85f);
            block.GetComponent<Renderer>().material.color = new Color(0.2f, 0.22f, 0.25f);
            Destroy(block.GetComponent<Collider>());

            // Filtro / toma de aire
            GameObject intake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            intake.name = "Filtro_Aire";
            intake.transform.SetParent(engineBay.transform, false);
            intake.transform.localPosition = new Vector3(0f, 0.23f, -0.05f);
            intake.transform.localScale = new Vector3(0.42f, 0.08f, 0.42f);
            intake.GetComponent<Renderer>().material.color = new Color(0.75f, 0.75f, 0.8f);
            Destroy(intake.GetComponent<Collider>());

            // Bandeja de batería (lado izquierdo frontal)
            GameObject tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tray.name = "Bandeja_Bateria";
            tray.transform.SetParent(engineBay.transform, false);
            tray.transform.localPosition = new Vector3(-0.55f, -0.02f, 0.22f);
            tray.transform.localScale = new Vector3(0.38f, 0.08f, 0.32f);
            tray.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.12f);
            Destroy(tray.GetComponent<Collider>());

            // Modelo visual de la batería dentro del compartimento (inicialmente apagada)
            GameObject batInCar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            batInCar.name = "Bateria_Instalada";
            batInCar.transform.SetParent(engineBay.transform, false);
            batInCar.transform.localPosition = new Vector3(-0.55f, 0.12f, 0.22f);
            batInCar.transform.localScale = new Vector3(0.32f, 0.22f, 0.25f);
            batInCar.GetComponent<Renderer>().material.color = new Color(0.95f, 0.82f, 0.1f); // Amarillo/negro
            Destroy(batInCar.GetComponent<Collider>());
            batInCar.SetActive(bateriaInstalada);
            modeloBateriaMotor = batInCar;

            // Tapón de llenado de aceite (lado derecho)
            GameObject oilCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            oilCap.name = "Tapon_Aceite";
            oilCap.transform.SetParent(engineBay.transform, false);
            oilCap.transform.localPosition = new Vector3(0.42f, 0.22f, 0.12f);
            oilCap.transform.localScale = new Vector3(0.14f, 0.06f, 0.14f);
            oilCap.GetComponent<Renderer>().material.color = new Color(0.9f, 0.75f, 0.05f); // Tapa amarilla
            Destroy(oilCap.GetComponent<Collider>());
        }
    }

    private void Update()
    {
        if (escapeIniciado) return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player != null) playerTransform = player.transform;
            return;
        }

        if (tiempoMensaje > 0f)
        {
            tiempoMensaje -= Time.deltaTime;
            if (tiempoMensaje <= 0f) mensajeFlotante = "";
        }

        // Evaluar posiciones clave alrededor del auto
        Vector3 posFrenteCofre = transform.TransformPoint(new Vector3(0f, 0.6f, 2.3f));
        Vector3 posTanqueGasolina = transform.TransformPoint(new Vector3(-1.3f, 0.6f, -1.2f));
        Vector3 posPuertaConductor = transform.TransformPoint(new Vector3(-1.3f, 0.7f, 0.2f));

        float distFrente = Vector3.Distance(playerTransform.position, posFrenteCofre);
        float distTanque = Vector3.Distance(playerTransform.position, posTanqueGasolina);
        float distPuerta = Vector3.Distance(playerTransform.position, posPuertaConductor);

        promptActual = "";
        bool teclaPresionada = TeclaInteraccionPresionada();

        // 1. ZONA COFRE Y MOTOR (FRENTE)
        if (distFrente <= 2.8f)
        {
            if (!cofreAbierto)
            {
                promptActual = "[E] Abrir cofre del auto";
                if (teclaPresionada && !animandoCofre)
                {
                    AbrirCofre();
                }
            }
            else
            {
                if (tieneBateria && !bateriaInstalada)
                {
                    promptActual = "[E] Instalar Batería en el motor";
                    if (teclaPresionada)
                    {
                        InstalarBateria();
                    }
                }
                else if (tieneAceite && !aceiteLlenado)
                {
                    promptActual = "[E] Rellenar Aceite de motor";
                    if (teclaPresionada)
                    {
                        RellenarAceite();
                    }
                }
                else
                {
                    string estadoBat = bateriaInstalada ? "<color=#2ecc71>Batería [✓]</color>" : "<color=#e74c3c>Falta Batería [✗]</color>";
                    string estadoAceite = aceiteLlenado ? "<color=#2ecc71>Aceite [✓]</color>" : "<color=#e74c3c>Falta Aceite [✗]</color>";
                    promptActual = string.Format("Cofre abierto  |  {0}  |  {1}", estadoBat, estadoAceite);
                }
            }
        }
        // 2. ZONA TANQUE DE GASOLINA (COSTADO TRASERO)
        else if (distTanque <= 2.2f)
        {
            if (gasolinaCargada)
            {
                promptActual = "Tanque de Combustible: <color=#2ecc71>Lleno [✓]</color>";
            }
            else if (tieneGasolina)
            {
                promptActual = "[E] Cargar Gasolina al tanque";
                if (teclaPresionada)
                {
                    CargarGasolina();
                }
            }
            else
            {
                promptActual = "Tanque de Combustible vacío: <color=#e74c3c>Falta Gasolina [✗]</color>";
            }
        }
        // 3. ZONA PUERTA DEL CONDUCTOR (COSTADO DELANTERO)
        else if (distPuerta <= 2.4f)
        {
            if (EstaListoParaArrancar)
            {
                promptActual = "[E] Subir al auto y ARRANCAR para escapar";
                if (teclaPresionada)
                {
                    IniciarSecuenciaEscape();
                }
            }
            else
            {
                System.Text.StringBuilder faltan = new System.Text.StringBuilder("Auto averiado. Faltan piezas:\n");
                if (!bateriaInstalada) faltan.Append(" • Instalar Batería en el cofre\n");
                if (!aceiteLlenado) faltan.Append(" • Verter Aceite en el motor\n");
                if (!gasolinaCargada) faltan.Append(" • Cargar Gasolina en el tanque\n");
                if (!tieneLlaves) faltan.Append(" • Encontrar Llaves del auto");
                promptActual = faltan.ToString();
            }
        }
    }

    public void AbrirCofre()
    {
        if (cofreAbierto || animandoCofre) return;
        StartCoroutine(AnimarAperturaCofre());
    }

    private IEnumerator AnimarAperturaCofre()
    {
        animandoCofre = true;
        cofreAbierto = true;

        ReproducirSonidoProcedural(TipoSonido.AperturaMetalica);
        MostrarMensajeFlotante("¡Cofre abierto! Revisa el compartimento del motor.", 4f);

        if (pivoteCofre != null)
        {
            Quaternion rotInicial = pivoteCofre.localRotation;
            Quaternion rotFinal = Quaternion.Euler(-55f, 0f, 0f);

            float duracion = 0.7f;
            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float factor = Mathf.SmoothStep(0f, 1f, t / duracion);
                pivoteCofre.localRotation = Quaternion.Slerp(rotInicial, rotFinal, factor);
                yield return null;
            }
            pivoteCofre.localRotation = rotFinal;
        }

        animandoCofre = false;
    }

    public void InstalarBateria()
    {
        if (bateriaInstalada) return;
        bateriaInstalada = true;

        if (modeloBateriaMotor != null)
        {
            modeloBateriaMotor.SetActive(true);
        }

        ReproducirSonidoProcedural(TipoSonido.ImpactoPesado);
        MostrarMensajeFlotante("¡Batería instalada y conectada al alternador! [✓]", 3.5f);
        VerificarListoParaArrancar();
    }

    public void RellenarAceite()
    {
        if (aceiteLlenado) return;
        aceiteLlenado = true;

        ReproducirSonidoProcedural(TipoSonido.Liquido);
        MostrarMensajeFlotante("¡Aceite de motor vertido en el depósito! [✓]", 3.5f);
        VerificarListoParaArrancar();
    }

    public void CargarGasolina()
    {
        if (gasolinaCargada) return;
        gasolinaCargada = true;

        ReproducirSonidoProcedural(TipoSonido.Liquido);
        MostrarMensajeFlotante("¡Tanque de gasolina abastecido al máximo! [✓]", 3.5f);
        VerificarListoParaArrancar();
    }

    private void VerificarListoParaArrancar()
    {
        if (EstaListoParaArrancar)
        {
            MostrarMensajeFlotante("¡EL AUTO ESTÁ LISTO! Ve a la puerta del conductor [E] para escapar.", 5f);
        }
    }

    public void RegistrarItemRecogido(TipoItemLevel2 tipo)
    {
        ReproducirSonidoProcedural(TipoSonido.CampanaItem);

        switch (tipo)
        {
            case TipoItemLevel2.Bateria:
                tieneBateria = true;
                MostrarMensajeFlotante("+ Recogiste: Batería de Auto (Llévala al cofre)", 3.5f);
                break;
            case TipoItemLevel2.Aceite:
                tieneAceite = true;
                MostrarMensajeFlotante("+ Recogiste: Aceite de Motor (Llévalo al cofre)", 3.5f);
                break;
            case TipoItemLevel2.Gasolina:
                tieneGasolina = true;
                MostrarMensajeFlotante("+ Recogiste: Bidón de Gasolina (Cárgalo en el tanque)", 3.5f);
                break;
            case TipoItemLevel2.Llaves:
                tieneLlaves = true;
                MostrarMensajeFlotante("+ Recogiste: Llaves del Automóvil", 3.5f);
                break;
        }

        VerificarListoParaArrancar();
    }

    public void MostrarMensajeFlotante(string mensaje, float duracion)
    {
        mensajeFlotante = mensaje;
        tiempoMensaje = duracion;
    }

    public void SetPromptExterno(string prompt)
    {
        if (string.IsNullOrEmpty(promptActual))
        {
            promptActual = prompt;
        }
    }

    public void IniciarSecuenciaEscape()
    {
        if (escapeIniciado) return;
        escapeIniciado = true;

        MostrarMensajeFlotante("¡MOTOR EN MARCHA! Derribando portón de salida...", 5f);
        ReproducirSonidoProcedural(TipoSonido.EscapeMotor);
        StartCoroutine(SecuenciaEmbestirPorton());
    }

    private IEnumerator SecuenciaEmbestirPorton()
    {
        if (playerTransform != null)
        {
            playerTransform.gameObject.SetActive(false);
        }

        float tiempo = 0f;
        while (tiempo < 4f)
        {
            tiempo += Time.deltaTime;
            transform.position += transform.forward * velocidadEscape * Time.deltaTime;

            if (portonSalida != null && Vector3.Distance(transform.position, portonSalida.position) < 5f)
            {
                Rigidbody rb = portonSalida.GetComponent<Rigidbody>();
                if (rb == null) rb = portonSalida.gameObject.AddComponent<Rigidbody>();
                rb.mass = 40f;
                rb.AddForce((transform.forward + Vector3.up * 0.4f) * 900f, ForceMode.Impulse);
                ReproducirSonidoProcedural(TipoSonido.ImpactoPesado);
            }

            yield return null;
        }

        MostrarMensajeFlotante("¡¡HAS ESCAPADO DE LA CRIATURA!! NIVEL 2 COMPLETADO", 8f);
    }

    private void OnGUI()
    {
        // 1. Mensaje de interacción en el centro de la pantalla
        if (!string.IsNullOrEmpty(promptActual))
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.box);
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.fontSize = 17;
            promptStyle.normal.textColor = Color.white;
            promptStyle.wordWrap = true;

            float width = 460f;
            float height = promptActual.Contains("\n") ? 100f : 45f;
            float x = (Screen.width - width) / 2f;
            float y = Screen.height * 0.72f;

            GUI.color = new Color(0.05f, 0.05f, 0.08f, 0.88f);
            GUI.Box(new Rect(x, y, width, height), GUIContent.none);
            GUI.color = Color.white;

            GUI.Label(new Rect(x + 10, y + 5, width - 20, height - 10), promptActual, promptStyle);
        }

        // 2. Notificación flotante superior
        if (!string.IsNullOrEmpty(mensajeFlotante))
        {
            GUIStyle notifStyle = new GUIStyle(GUI.skin.box);
            notifStyle.alignment = TextAnchor.MiddleCenter;
            notifStyle.fontSize = 16;
            notifStyle.fontStyle = FontStyle.Bold;
            notifStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);

            float nWidth = 520f;
            float nHeight = 42f;
            float nx = (Screen.width - nWidth) / 2f;
            float ny = 35f;

            GUI.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
            GUI.Box(new Rect(nx, ny, nWidth, nHeight), GUIContent.none);
            GUI.color = Color.white;

            GUI.Label(new Rect(nx + 10, ny + 2, nWidth - 20, nHeight - 4), mensajeFlotante, notifStyle);
        }

        // 3. Mini indicador de progreso en la esquina superior derecha
        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = 13;
        statusStyle.fontStyle = FontStyle.Bold;
        statusStyle.normal.textColor = new Color(0.85f, 0.85f, 0.9f);

        float sWidth = 190f;
        float sHeight = 110f;
        float sx = Screen.width - sWidth - 20f;
        float sy = 20f;

        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.Box(new Rect(sx, sy, sWidth, sHeight), GUIContent.none);
        GUI.color = Color.white;

        string batMark = bateriaInstalada ? "<color=#2ecc71>[✓] Instalada</color>" : (tieneBateria ? "<color=#f39c12>[!] En mano</color>" : "<color=#e74c3c>[✗] Falta</color>");
        string aceMark = aceiteLlenado ? "<color=#2ecc71>[✓] Vertido</color>" : (tieneAceite ? "<color=#f39c12>[!] En mano</color>" : "<color=#e74c3c>[✗] Falta</color>");
        string gasMark = gasolinaCargada ? "<color=#2ecc71>[✓] Llena</color>" : (tieneGasolina ? "<color=#f39c12>[!] En mano</color>" : "<color=#e74c3c>[✗] Falta</color>");
        string keyMark = tieneLlaves ? "<color=#2ecc71>[✓] Lista</color>" : "<color=#e74c3c>[✗] Falta</color>";

        string statusText = string.Format("<b>ESTADO DEL AUTO</b>\nBatería: {0}\nAceite: {1}\nGasolina: {2}\nLlaves: {3}",
            batMark, aceMark, gasMark, keyMark);

        GUI.Label(new Rect(sx + 10, sy + 8, sWidth - 20, sHeight - 16), statusText, statusStyle);
    }

    #region Generacion de Sonidos Procedurales
    private enum TipoSonido { AperturaMetalica, ImpactoPesado, Liquido, CampanaItem, EscapeMotor }

    private void ReproducirSonidoProcedural(TipoSonido tipo)
    {
        if (audioSource == null) return;
        AudioClip clip = GenerarClip(tipo);
        if (clip != null) audioSource.PlayOneShot(clip, 0.85f);
    }

    private AudioClip GenerarClip(TipoSonido tipo)
    {
        int samplerate = 44100;
        float duration = 0.5f;
        if (tipo == TipoSonido.EscapeMotor) duration = 3.5f;
        else if (tipo == TipoSonido.AperturaMetalica) duration = 0.7f;
        else if (tipo == TipoSonido.Liquido) duration = 1.2f;

        int samplesCount = (int)(samplerate * duration);
        float[] samples = new float[samplesCount];

        for (int i = 0; i < samplesCount; i++)
        {
            float t = (float)i / samplerate;
            float val = 0f;

            switch (tipo)
            {
                case TipoSonido.AperturaMetalica:
                    // Crujido metálico con golpe inicial
                    float clunk = Mathf.Sin(2f * Mathf.PI * 140f * t) * Mathf.Exp(-t * 22f);
                    float squeak = Mathf.Sin(2f * Mathf.PI * (520f + 300f * Mathf.Sin(t * 18f)) * t) * (1f - (t / duration));
                    val = (clunk * 0.7f) + (squeak * 0.35f);
                    break;

                case TipoSonido.ImpactoPesado:
                    // Golpe metálico sordo
                    val = (Mathf.Sin(2f * Mathf.PI * 85f * t) + Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.5f) * Mathf.Exp(-t * 15f);
                    break;

                case TipoSonido.Liquido:
                    // Burbujeo / vertido
                    float noise = (Random.value * 2f - 1f) * 0.3f;
                    float bubble = Mathf.Sin(2f * Mathf.PI * (340f + 160f * Mathf.Sin(t * 40f)) * t);
                    val = (bubble * 0.4f + noise * 0.6f) * (1f - (t / duration));
                    break;

                case TipoSonido.CampanaItem:
                    // Campanilla agradable de recolección
                    float freq = t < 0.12f ? 523.25f : 659.25f;
                    val = Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t * 6f);
                    break;

                case TipoSonido.EscapeMotor:
                    // Rugido de motor acelerando progresivamente
                    float pitchRamp = 65f + (t / duration) * 220f;
                    float engine = Mathf.Sin(2f * Mathf.PI * pitchRamp * t) + 0.4f * Mathf.Sin(2f * Mathf.PI * pitchRamp * 2f * t);
                    float engineNoise = (Random.value * 2f - 1f) * 0.25f;
                    val = (engine + engineNoise) * Mathf.Clamp01(t * 4f);
                    break;
            }

            samples[i] = Mathf.Clamp(val, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_" + tipo.ToString(), samplesCount, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }
    #endregion
}
