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

    [Header("Sonidos de Auto Realistas")]
    [SerializeField] private AudioClip sfxAbrirCofre;
    [SerializeField] private AudioClip sfxCerrarCofre;
    [SerializeField] private AudioClip sfxAbrirPuerta;
    [SerializeField] private AudioClip sfxCerrarPuerta;
    [SerializeField] private AudioClip sfxArranqueMotor;

    // Estado interno
    private bool animandoCofre = false;
    private bool escapeIniciado = false;
    private Transform playerTransform;
    private AudioSource audioSource;
    private Quaternion rotacionCofreCerrado = Quaternion.identity;
    private Quaternion rotacionCofreAbierto = Quaternion.Euler(-40f, 0f, 0f);

    // UI Minimalista
    private string promptActual = "";
    private string mensajeFlotante = "";
    private float tiempoMensaje = 0f;
    private string promptExterno = "";
    private float tiempoPromptExterno = -10f;

    public bool CofreAbierto => cofreAbierto;
    public bool EstaListoParaArrancar => bateriaInstalada && aceiteLlenado && gasolinaCargada && tieneLlaves;

    public bool TieneBateria => tieneBateria;
    public bool TieneAceite => tieneAceite;
    public bool TieneGasolina => tieneGasolina;
    public bool TieneLlaves => tieneLlaves;
    public bool BateriaInstalada => bateriaInstalada;
    public bool AceiteLlenado => aceiteLlenado;
    public bool GasolinaCargada => gasolinaCargada;
    public bool EscapeIniciado => escapeIniciado;
    public string PromptActual => promptActual;
    public string MensajeFlotante => mensajeFlotante;

    public bool UsarUIExterna { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ConfigurarAudios(AudioClip abrirCofre, AudioClip cerrarCofre, AudioClip abrirPuerta, AudioClip cerrarPuerta, AudioClip arranqueMotor)
    {
        sfxAbrirCofre = abrirCofre;
        sfxCerrarCofre = cerrarCofre;
        sfxAbrirPuerta = abrirPuerta;
        sfxCerrarPuerta = cerrarPuerta;
        sfxArranqueMotor = arranqueMotor;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.0f; // 2D directo para que el jugador siempre lo escuche alto y claro
        audioSource.volume = 1.0f;

#if UNITY_EDITOR
        if (sfxAbrirCofre == null) sfxAbrirCofre = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/Level2/Cofre_Abrir.wav");
        if (sfxCerrarCofre == null) sfxCerrarCofre = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/Level2/Cofre_Cerrar.wav");
        if (sfxAbrirPuerta == null) sfxAbrirPuerta = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/Level2/Puerta_Abrir.wav");
        if (sfxCerrarPuerta == null) sfxCerrarPuerta = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/Level2/Puerta_Cerrar.wav");
        if (sfxArranqueMotor == null) sfxArranqueMotor = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/Level2/Motor_Arranque.wav");
#endif

        // Inmovilizar físicamente el auto para que el jugador no lo empuje al chocarlo
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null) playerTransform = player.transform;

        if (portonSalida == null)
        {
            GameObject gateGo = GameObject.Find("Porton_Salida_Estacionamiento");
            if (gateGo != null) portonSalida = gateGo.transform;
        }

        AsegurarComponentesCofreYMotor();

        cofreAbierto = false;
        if (pivoteCofre != null)
        {
            pivoteCofre.localRotation = rotacionCofreCerrado;
            pivoteCofre.gameObject.SetActive(false);
        }
        Transform bay = transform.Find("Compartimento_Motor");
        if (bay != null)
        {
            bay.gameObject.SetActive(false);
        }
    }

    private bool TeclaInteraccionPresionada()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame ||
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

    public void AsegurarComponentesCofreYMotor()
    {
        ConstruirCofreYMotorReal();
    }

    private static void SafeDestroy(Object obj)
    {
        if (obj == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(obj);
            return;
        }
#endif
        Destroy(obj);
    }

    private static Material CrearMaterialMotor(Color col, float smoothness = 0.5f, float metallic = 0.2f)
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");
        Material mat = new Material(litShader);
        mat.color = col;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", col);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        return mat;
    }

    /// <summary>
    /// Construye el cofre 3D articulado de forma realista y el compartimento de motor
    /// con aislante térmico interior y bisagra funcional.
    /// </summary>
    public void ConstruirCofreYMotorReal()
    {
        // 1. Limpiar objetos antiguos si existen
        Transform viejoPivote = transform.Find("Pivote_Cofre");
        if (viejoPivote != null) SafeDestroy(viejoPivote.gameObject);

        Transform viejoMotor = transform.Find("Compartimento_Motor");
        if (viejoMotor != null) SafeDestroy(viejoMotor.gameObject);

        // 2. Crear el Punto de Giro / Bisagra en la base del parabrisas
        GameObject pivotGo = new GameObject("Pivote_Cofre");
        pivotGo.transform.SetParent(transform, false);
        pivotGo.transform.localPosition = new Vector3(0f, 0.81f, 0.58f);
        pivotGo.transform.localRotation = rotacionCofreCerrado;
        pivoteCofre = pivotGo.transform;
        rotacionCofreCerrado = Quaternion.identity;
        rotacionCofreAbierto = Quaternion.Euler(-40f, 0f, 0f);

        // 3. Tapa del cofre exterior (Rojo deportivo brillante a juego con el auto)
        GameObject hoodPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hoodPanel.name = "Tapa_Cofre_Panel";
        hoodPanel.transform.SetParent(pivoteCofre, false);
        hoodPanel.transform.localPosition = new Vector3(0f, -0.04f, 0.76f);
        hoodPanel.transform.localRotation = Quaternion.Euler(-5.5f, 0f, 0f);
        hoodPanel.transform.localScale = new Vector3(1.72f, 0.04f, 1.52f);

        Material matCofreRojo = null;
#if UNITY_EDITOR
        matCofreRojo = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Player/Red.mat");
#endif
        if (matCofreRojo == null)
        {
            matCofreRojo = CrearMaterialMotor(new Color(0.86f, 0.10f, 0.08f, 1f), 0.85f, 0.35f);
        }
        hoodPanel.GetComponent<Renderer>().sharedMaterial = matCofreRojo;
        SafeDestroy(hoodPanel.GetComponent<Collider>());

        // Bisel frontal aerodinámico (también en rojo deportivo)
        GameObject noseLip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        noseLip.name = "Bisel_Frontal";
        noseLip.transform.SetParent(hoodPanel.transform, false);
        noseLip.transform.localPosition = new Vector3(0f, -0.2f, 0.52f);
        noseLip.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
        noseLip.transform.localScale = new Vector3(0.98f, 0.8f, 0.15f);
        noseLip.GetComponent<Renderer>().sharedMaterial = matCofreRojo;
        SafeDestroy(noseLip.GetComponent<Collider>());

        // 5. Compartimento de Motor Detallado (revelado al levantar el cofre)
        GameObject engineBay = new GameObject("Compartimento_Motor");
        engineBay.transform.SetParent(transform, false);
        engineBay.transform.localPosition = new Vector3(0f, 0.68f, 1.35f);

        // Fondo oscuro del vano motor
        GameObject bayFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bayFloor.name = "Fondo_Vano_Motor";
        bayFloor.transform.SetParent(engineBay.transform, false);
        bayFloor.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        bayFloor.transform.localScale = new Vector3(1.5f, 0.04f, 1.3f);
        bayFloor.GetComponent<Renderer>().sharedMaterial = CrearMaterialMotor(new Color(0.12f, 0.12f, 0.14f), 0.1f, 0f);
        SafeDestroy(bayFloor.GetComponent<Collider>());

        // Bloque de motor V8
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = "Bloque_Motor_V8";
        block.transform.SetParent(engineBay.transform, false);
        block.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        block.transform.localScale = new Vector3(0.72f, 0.32f, 0.82f);
        block.GetComponent<Renderer>().sharedMaterial = CrearMaterialMotor(new Color(0.25f, 0.28f, 0.32f), 0.6f, 0.5f);
        SafeDestroy(block.GetComponent<Collider>());

        // Cubiertas de válvulas cromadas V8
        GameObject valveL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        valveL.name = "Cubierta_Valvulas_Izq";
        valveL.transform.SetParent(engineBay.transform, false);
        valveL.transform.localPosition = new Vector3(-0.25f, 0.26f, 0f);
        valveL.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
        valveL.transform.localScale = new Vector3(0.22f, 0.08f, 0.78f);
        Material matCromoValvulas = CrearMaterialMotor(new Color(0.75f, 0.78f, 0.82f), 0.85f, 0.8f);
        valveL.GetComponent<Renderer>().sharedMaterial = matCromoValvulas;
        SafeDestroy(valveL.GetComponent<Collider>());

        GameObject valveR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        valveR.name = "Cubierta_Valvulas_Der";
        valveR.transform.SetParent(engineBay.transform, false);
        valveR.transform.localPosition = new Vector3(0.25f, 0.26f, 0f);
        valveR.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
        valveR.transform.localScale = new Vector3(0.22f, 0.08f, 0.78f);
        valveR.GetComponent<Renderer>().sharedMaterial = matCromoValvulas;
        SafeDestroy(valveR.GetComponent<Collider>());

        // Filtro de aire cromado central
        GameObject filter = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        filter.name = "Filtro_Aire_Cromado";
        filter.transform.SetParent(engineBay.transform, false);
        filter.transform.localPosition = new Vector3(0f, 0.28f, -0.06f);
        filter.transform.localScale = new Vector3(0.42f, 0.07f, 0.42f);
        filter.GetComponent<Renderer>().sharedMaterial = CrearMaterialMotor(new Color(0.85f, 0.85f, 0.9f), 0.9f, 0.85f);
        SafeDestroy(filter.GetComponent<Collider>());

        // Bandeja de soporte de batería
        GameObject tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tray.name = "Soporte_Bateria";
        tray.transform.SetParent(engineBay.transform, false);
        tray.transform.localPosition = new Vector3(-0.55f, 0.06f, 0.26f);
        tray.transform.localScale = new Vector3(0.36f, 0.06f, 0.32f);
        tray.GetComponent<Renderer>().sharedMaterial = CrearMaterialMotor(new Color(0.15f, 0.15f, 0.18f), 0.2f, 0.1f);
        SafeDestroy(tray.GetComponent<Collider>());

        // Batería 3D instalada
        GameObject bat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bat.name = "Bateria_Visual_Instalada";
        bat.transform.SetParent(engineBay.transform, false);
        bat.transform.localPosition = new Vector3(-0.55f, 0.18f, 0.26f);
        bat.transform.localScale = new Vector3(0.32f, 0.20f, 0.26f);
        bat.GetComponent<Renderer>().sharedMaterial = CrearMaterialMotor(new Color(0.98f, 0.82f, 0.05f), 0.4f, 0f);
        SafeDestroy(bat.GetComponent<Collider>());
        bat.SetActive(bateriaInstalada);
        modeloBateriaMotor = bat;

        // Tapón de aceite
        GameObject oilCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        oilCap.name = "Tapon_Aceite_Motor";
        oilCap.transform.SetParent(engineBay.transform, false);
        oilCap.transform.localPosition = new Vector3(0.42f, 0.26f, 0.18f);
        oilCap.transform.localScale = new Vector3(0.14f, 0.06f, 0.14f);
        oilCap.GetComponent<Renderer>().sharedMaterial = CrearMaterialMotor(new Color(0.95f, 0.75f, 0.05f), 0.5f, 0.1f);
        SafeDestroy(oilCap.GetComponent<Collider>());

        // Luz de trabajo cálida que ilumina el motor cuando el cofre se levanta
        GameObject workLightGo = new GameObject("Luz_Vano_Motor");
        workLightGo.transform.SetParent(engineBay.transform, false);
        workLightGo.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        Light workLight = workLightGo.AddComponent<Light>();
        workLight.type = LightType.Point;
        workLight.range = 3.5f;
        workLight.color = new Color(1.0f, 0.90f, 0.70f);
        workLight.intensity = 3.0f;
        workLight.shadows = LightShadows.None;

        pivoteCofre.gameObject.SetActive(cofreAbierto);
        engineBay.SetActive(cofreAbierto);
    }

    private void Update()
    {
        if (escapeIniciado) return;

        if (LevelStateManager.Instance != null && !LevelStateManager.Instance.JuegoActivo) return;

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
        if (distFrente <= 3.0f)
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
                    promptActual = "[E] Instalar Batería   |   [Q] Cerrar cofre";
                    if (teclaPresionada)
                    {
                        InstalarBateria();
                    }
                    else if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame && !animandoCofre)
                    {
                        CerrarCofre();
                    }
                }
                else if (tieneAceite && !aceiteLlenado)
                {
                    promptActual = "[E] Rellenar Aceite de motor   |   [Q] Cerrar cofre";
                    if (teclaPresionada)
                    {
                        RellenarAceite();
                    }
                    else if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame && !animandoCofre)
                    {
                        CerrarCofre();
                    }
                }
                else
                {
                    string estadoBat = bateriaInstalada ? "<color=#2ecc71>Batería [✓]</color>" : "<color=#e74c3c>Falta Batería [✗]</color>";
                    string estadoAceite = aceiteLlenado ? "<color=#2ecc71>Aceite [✓]</color>" : "<color=#e74c3c>Falta Aceite [✗]</color>";
                    promptActual = string.Format("[E] Cerrar cofre   ({0} | {1})", estadoBat, estadoAceite);

                    if (teclaPresionada && !animandoCofre)
                    {
                        CerrarCofre();
                    }
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

        if (string.IsNullOrEmpty(promptActual) && !string.IsNullOrEmpty(promptExterno) && Time.time - tiempoPromptExterno <= 0.15f)
            promptActual = promptExterno;
    }

    public void AbrirCofre()
    {
        if (cofreAbierto || animandoCofre) return;
        StartCoroutine(AnimarAperturaCofre());
    }

    public void CerrarCofre()
    {
        if (!cofreAbierto || animandoCofre) return;
        StartCoroutine(AnimarCierreCofre());
    }

    private IEnumerator AnimarAperturaCofre()
    {
        animandoCofre = true;
        cofreAbierto = true;

        if (pivoteCofre != null)
        {
            pivoteCofre.gameObject.SetActive(true);
            pivoteCofre.localRotation = rotacionCofreCerrado;
        }
        Transform bay = transform.Find("Compartimento_Motor");
        if (bay != null) bay.gameObject.SetActive(true);

        ReproducirSonidoProcedural(TipoSonido.AperturaCofre);
        MostrarMensajeFlotante("¡Cofre abierto! Revisa el compartimento del motor.", 3.5f);

        if (pivoteCofre != null)
        {
            Quaternion rotInicial = pivoteCofre.localRotation;
            float duracion = 0.65f;
            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float factor = Mathf.SmoothStep(0f, 1f, t / duracion);
                pivoteCofre.localRotation = Quaternion.Slerp(rotInicial, rotacionCofreAbierto, factor);
                yield return null;
            }
            pivoteCofre.localRotation = rotacionCofreAbierto;
        }

        animandoCofre = false;
    }

    private IEnumerator AnimarCierreCofre()
    {
        animandoCofre = true;

        if (pivoteCofre != null)
        {
            Quaternion rotInicial = pivoteCofre.localRotation;
            float duracion = 0.45f;
            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float factor = Mathf.SmoothStep(0f, 1f, t / duracion);
                pivoteCofre.localRotation = Quaternion.Slerp(rotInicial, rotacionCofreCerrado, factor);
                yield return null;
            }
            pivoteCofre.localRotation = rotacionCofreCerrado;
            pivoteCofre.gameObject.SetActive(false);
        }

        Transform bay = transform.Find("Compartimento_Motor");
        if (bay != null) bay.gameObject.SetActive(false);

        cofreAbierto = false;
        ReproducirSonidoProcedural(TipoSonido.CierreCofre);
        MostrarMensajeFlotante("Cofre cerrado.", 2f);

        animandoCofre = false;
    }

    public void InstalarBateria()
    {
        if (bateriaInstalada) return;
        bateriaInstalada = true;

        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.AgregarPuntaje(150);

        if (modeloBateriaMotor != null)
        {
            modeloBateriaMotor.SetActive(true);
        }

        ReproducirSonidoProcedural(TipoSonido.ImpactoPesado);
        MostrarMensajeFlotante("¡Batería conectada al sistema eléctrico! +150 PTS", 3.5f);
    }

    public void RellenarAceite()
    {
        if (aceiteLlenado) return;
        aceiteLlenado = true;

        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.AgregarPuntaje(150);

        ReproducirSonidoProcedural(TipoSonido.Liquido);
        MostrarMensajeFlotante("¡Aceite vertido en el motor! Nivel óptimo. +150 PTS", 3.5f);
    }

    public void CargarGasolina()
    {
        if (gasolinaCargada) return;
        gasolinaCargada = true;

        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.AgregarPuntaje(150);

        ReproducirSonidoProcedural(TipoSonido.Liquido);
        MostrarMensajeFlotante("¡Tanque lleno de combustible! +150 PTS", 3.5f);
    }

    public void RegistrarItemRecogido(TipoItemLevel2 tipo)
    {
        switch (tipo)
        {
            case TipoItemLevel2.Bateria:
                tieneBateria = true;
                break;
            case TipoItemLevel2.Aceite:
                tieneAceite = true;
                break;
            case TipoItemLevel2.Gasolina:
                tieneGasolina = true;
                break;
            case TipoItemLevel2.Llaves:
                tieneLlaves = true;
                break;
        }

        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.AgregarPuntaje(100);

        ReproducirSonidoProcedural(TipoSonido.CampanaItem);
        MostrarMensajeFlotante("¡Objeto recogido: " + tipo + "! +100 PTS", 3.5f);
    }

    public void RegistrarItemRecogido(string nombreItem)
    {
        RecogerItem(nombreItem);
    }

    public void RecogerItem(string nombreItem)
    {
        string lower = nombreItem.ToLower();
        if (lower.Contains("bateria") || lower.Contains("battery")) tieneBateria = true;
        else if (lower.Contains("aceite") || lower.Contains("oil")) tieneAceite = true;
        else if (lower.Contains("gasolina") || lower.Contains("gas")) tieneGasolina = true;
        else if (lower.Contains("llave") || lower.Contains("key")) tieneLlaves = true;

        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.AgregarPuntaje(100);

        ReproducirSonidoProcedural(TipoSonido.CampanaItem);
        MostrarMensajeFlotante("¡Objeto recogido: " + nombreItem + "! +100 PTS", 3.5f);
    }

    public void MostrarMensajeFlotante(string mensaje, float duracion = 3.5f)
    {
        mensajeFlotante = mensaje;
        tiempoMensaje = duracion;
    }

    public void SetPromptExterno(string prompt)
    {
        promptExterno = prompt;
        tiempoPromptExterno = Time.time;
    }

    public void IniciarSecuenciaEscape()
    {
        if (escapeIniciado) return;
        escapeIniciado = true;

        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.AgregarPuntaje(500);

        StartCoroutine(SecuenciaCinematicaEscape());
    }

    private IEnumerator SecuenciaCinematicaEscape()
    {
        // 1. Abrir puerta del conductor
        ReproducirSonidoProcedural(TipoSonido.AperturaPuerta);
        MostrarMensajeFlotante("Subiendo al vehículo...", 2f);
        yield return new WaitForSeconds(0.4f);

        // 2. Cerrar puerta con golpe sólido
        ReproducirSonidoProcedural(TipoSonido.CierrePuerta);
        if (playerTransform != null)
        {
            playerTransform.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(0.35f);

        // 3. Encender el motor: Marcha de arranque y rugido V8
        ReproducirSonidoProcedural(TipoSonido.ArranqueMotor);
        MostrarMensajeFlotante("¡¡V8 RUGIENDO!! Derribando portón de salida...", 5f);
        yield return new WaitForSeconds(0.8f);

        // 4. Aceleración y embestida al portón
        float tiempo = 0f;
        while (tiempo < 3.5f)
        {
            tiempo += Time.deltaTime;
            transform.position += transform.forward * velocidadEscape * Time.deltaTime;

            if (portonSalida != null && Vector3.Distance(transform.position, portonSalida.position) < 5.5f)
            {
                Rigidbody rb = portonSalida.GetComponent<Rigidbody>();
                if (rb == null) rb = portonSalida.gameObject.AddComponent<Rigidbody>();
                rb.mass = 40f;
                rb.AddForce((transform.forward + Vector3.up * 0.4f) * 1100f, ForceMode.Impulse);
                ReproducirSonidoProcedural(TipoSonido.ImpactoPesado);
            }

            yield return null;
        }

        MostrarMensajeFlotante("¡¡HAS ESCAPADO DEL ESTACIONAMIENTO!! NIVEL 2 COMPLETADO", 8f);

        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.CompletarNivel();
    }

    #region Generacion de Sonidos Procedurales Realistas de Auto
    private enum TipoSonido
    {
        AperturaCofre,
        CierreCofre,
        AperturaPuerta,
        CierrePuerta,
        ArranqueMotor,
        ImpactoPesado,
        Liquido,
        CampanaItem
    }

    private void ReproducirSonidoProcedural(TipoSonido tipo)
    {
        AudioClip clip = null;
        switch (tipo)
        {
            case TipoSonido.AperturaCofre: clip = sfxAbrirCofre; break;
            case TipoSonido.CierreCofre: clip = sfxCerrarCofre; break;
            case TipoSonido.AperturaPuerta: clip = sfxAbrirPuerta; break;
            case TipoSonido.CierrePuerta: clip = sfxCerrarPuerta; break;
            case TipoSonido.ArranqueMotor: clip = sfxArranqueMotor; break;
        }

        if (clip == null) clip = GenerarClip(tipo);

        if (clip != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip, 1.0f);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, 1.0f);
            }
        }
    }

    private AudioClip GenerarClip(TipoSonido tipo)
    {
        int samplerate = 44100;
        float duration = 0.5f;

        if (tipo == TipoSonido.ArranqueMotor) duration = 4.0f;
        else if (tipo == TipoSonido.AperturaCofre) duration = 0.75f;
        else if (tipo == TipoSonido.CierreCofre) duration = 0.65f;
        else if (tipo == TipoSonido.AperturaPuerta) duration = 0.45f;
        else if (tipo == TipoSonido.CierrePuerta) duration = 0.50f;
        else if (tipo == TipoSonido.Liquido) duration = 1.2f;

        int samplesCount = (int)(samplerate * duration);
        float[] samples = new float[samplesCount];

        for (int i = 0; i < samplesCount; i++)
        {
            float t = (float)i / samplerate;
            float val = 0f;

            switch (tipo)
            {
                case TipoSonido.AperturaCofre:
                    // Click de pestillo + chirrido de bisagra
                    float latch = (t < 0.08f) ? Mathf.Sin(2f * Mathf.PI * 450f * t) * Mathf.Exp(-t * 80f) : 0f;
                    float hinge = Mathf.Sin(2f * Mathf.PI * (280f + 160f * Mathf.Sin(t * 12f)) * t) * Mathf.Exp(-t * 4f);
                    val = latch * 0.9f + hinge * 0.4f;
                    break;

                case TipoSonido.CierreCofre:
                    // Golpe metálico pesado de lámina de cofre cerrando con fuerza
                    float thud = Mathf.Sin(2f * Mathf.PI * 85f * t) * Mathf.Exp(-t * 16f);
                    float metalClang = (Mathf.Sin(2f * Mathf.PI * 320f * t) + Mathf.Sin(2f * Mathf.PI * 680f * t) * 0.4f) * Mathf.Exp(-t * 28f);
                    float rattle = (Random.value * 2f - 1f) * Mathf.Exp(-t * 35f) * 0.35f;
                    val = thud * 1.1f + metalClang * 0.6f + rattle;
                    break;

                case TipoSonido.AperturaPuerta:
                    // Manija destrabando + despegue de goma
                    float handle = Mathf.Sin(2f * Mathf.PI * 650f * t) * Mathf.Exp(-t * 55f);
                    float swing = (Random.value * 2f - 1f) * 0.15f * Mathf.Exp(-t * 6f);
                    val = handle * 0.8f + swing;
                    break;

                case TipoSonido.CierrePuerta:
                    // "Thunk" sólido y hermético de puerta automotriz
                    float doorBass = Mathf.Sin(2f * Mathf.PI * 95f * t) * Mathf.Exp(-t * 20f);
                    float doorLatch = Mathf.Sin(2f * Mathf.PI * 420f * t) * Mathf.Exp(-t * 45f);
                    val = doorBass * 1.2f + doorLatch * 0.5f;
                    break;

                case TipoSonido.ArranqueMotor:
                    if (t < 1.0f)
                    {
                        // 1. Motor de arranque girando (marcha: chug-chug-chug)
                        float crankFreq = 16f; // Rpm del arranque
                        float crankPulse = Mathf.Abs(Mathf.Sin(2f * Mathf.PI * crankFreq * t));
                        float crankSound = Mathf.Sin(2f * Mathf.PI * 180f * t) * crankPulse;
                        val = crankSound * 0.85f + (Random.value * 2f - 1f) * 0.2f;
                    }
                    else
                    {
                        // 2. Encendido y rugido V8 potente
                        float tMotor = t - 1.0f;
                        float rpm = 65f + Mathf.Sin(tMotor * 1.5f) * 35f + tMotor * 50f;
                        float cyl1 = Mathf.Sin(2f * Mathf.PI * rpm * t);
                        float cyl2 = Mathf.Sin(2f * Mathf.PI * (rpm * 2.1f) * t) * 0.6f;
                        float rumble = (Random.value * 2f - 1f) * 0.35f;
                        float roar = (cyl1 + cyl2 + rumble) * Mathf.Clamp01(tMotor * 4f);
                        val = roar * 0.95f;
                    }
                    break;

                case TipoSonido.ImpactoPesado:
                    val = (Mathf.Sin(2f * Mathf.PI * 80f * t) + (Random.value * 2f - 1f) * 0.6f) * Mathf.Exp(-t * 12f);
                    break;

                case TipoSonido.Liquido:
                    float bubble = Mathf.Sin(2f * Mathf.PI * (350f + 140f * Mathf.Sin(t * 35f)) * t);
                    float splash = (Random.value * 2f - 1f) * 0.35f;
                    val = (bubble * 0.5f + splash * 0.5f) * (1f - (t / duration));
                    break;

                case TipoSonido.CampanaItem:
                    float freq = t < 0.12f ? 523.25f : 659.25f;
                    val = Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t * 6f);
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
