using UnityEngine;
using UnityEngine.InputSystem;

public class BicicletaInteractuable : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float distanciaInteraccion = 3.2f;
    [SerializeField] private Light luzFarolBici;
    [SerializeField] private GameObject indicadorVisual;

    private bool jugadorEnRango = false;
    private bool interactuado = false;
    private Transform transformJugador;

    private void Awake()
    {
        AsegurarModelo3D();
    }

    public void AsegurarModelo3D()
    {
        // 1. Eliminar primitivas viejas si aún existen en la escena
        string[] viejasPartes = new string[] { "Cuadro", "Rueda_Trasera", "Rueda_Delantera", "Manillar", "Asiento" };
        foreach (var p in viejasPartes)
        {
            Transform t = transform.Find(p);
            if (t != null)
            {
                Destroy(t.gameObject);
            }
        }

        // 2. Si no tiene el modelo oficial 3D, instanciar Low-Poly Bicycle # 5
        if (transform.Find("Modelo_Bicicleta_3D") == null)
        {
            GameObject prefab = null;
#if UNITY_EDITOR
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Bicycle Models/Bicycle Model #5/Prefab/Low-Poly Bicycle # 5.prefab");
#endif
            if (prefab != null)
            {
                GameObject model = Instantiate(prefab, transform);
                model.name = "Modelo_Bicicleta_3D";
                model.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
            }
        }
    }

    private void Start()
    {
        AsegurarModelo3D();
        BuscarJugador();
        if (luzFarolBici != null)
        {
            luzFarolBici.intensity = 1.2f;
        }
    }

    private void BuscarJugador()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null)
        {
            PlayerController pc = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) p = pc.gameObject;
        }
        if (p != null) transformJugador = p.transform;
    }

    private void Update()
    {
        if (interactuado) return;

        if (transformJugador == null)
        {
            BuscarJugador();
            if (transformJugador == null) return;
        }

        float dist = Vector3.Distance(transform.position, transformJugador.position);
        if (dist <= distanciaInteraccion)
        {
            if (!jugadorEnRango)
            {
                jugadorEnRango = true;
                if (Level3Controller.Instance != null)
                {
                    bool puedeUsar = Level3Controller.Instance.PuedeUsarBicicleta();
                    if (puedeUsar)
                    {
                        Level3Controller.Instance.MostrarPrompt("[E] / [X] Montar la bicicleta para escapar y regresar a la universidad");
                    }
                    else
                    {
                        Level3Controller.Instance.MostrarPrompt("[Bicicleta lista para el escape - Encuentra antes el expediente]");
                    }
                }
            }

            bool presionoInteraccion = false;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                presionoInteraccion = true;
            if (Gamepad.current != null && (Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame))
                presionoInteraccion = true;

            if (presionoInteraccion)
            {
                IntentarInteractuar();
            }
        }
        else if (jugadorEnRango)
        {
            jugadorEnRango = false;
            if (Level3Controller.Instance != null)
            {
                Level3Controller.Instance.LimpiarPrompt("");
            }
        }
    }

    private void IntentarInteractuar()
    {
        if (Level3Controller.Instance == null) return;

        if (!Level3Controller.Instance.PuedeUsarBicicleta())
        {
            Level3Controller.Instance.MostrarMensaje("No puedes huir todavía. Debes encontrar primero el expediente del origen en la caseta de transbordo.");
            return;
        }

        interactuado = true;
        jugadorEnRango = false;
        Level3Controller.Instance.LimpiarPrompt("");

        CinematicaEscapeBicicleta cine = UnityEngine.Object.FindAnyObjectByType<CinematicaEscapeBicicleta>();
        if (cine == null)
        {
            GameObject cineGo = new GameObject("CinematicaEscape_Auto");
            cine = cineGo.AddComponent<CinematicaEscapeBicicleta>();
        }

        cine.IniciarEscapeCinematico(transform);
    }
}
