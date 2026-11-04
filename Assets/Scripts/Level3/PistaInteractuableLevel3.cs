using UnityEngine;
using UnityEngine.InputSystem;

public class PistaInteractuableLevel3 : MonoBehaviour
{
    [Header("Configuración de la Pista")]
    [SerializeField] private string nombrePista = "Fotografía de la Universidad";
    [SerializeField] [TextArea(2, 5)] private string descripcionPista = "Una fotografía aérea del campus donde se aprecian anomalías en el laboratorio subterráneo.";
    [SerializeField] private bool esExpedientePrincipal = false;
    [SerializeField] private Sprite imagenNota;
    [SerializeField] private float distanciaInteraccion = 2.8f;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject efectoBrillo;
    [SerializeField] private bool rotarSuavemente = true;

    private bool recolectada = false;
    private bool jugadorEnRango = false;
    private Transform transformJugador;
    private NotasUI notasUI;

    private void Start()
    {
        notasUI = UnityEngine.Object.FindAnyObjectByType<NotasUI>();
        BuscarJugador();
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
        if (recolectada) return;

        if (rotarSuavemente && efectoBrillo != null)
        {
            efectoBrillo.transform.Rotate(0f, 40f * Time.deltaTime, 0f, Space.World);
        }

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
                    string tipo = esExpedientePrincipal ? "el expediente confidencial" : "la pista";
                    Level3Controller.Instance.MostrarPrompt($"[E] / [X] Examinar {tipo}: {nombrePista}");
                }
            }

            // Input System o tecla E
            bool presionoInteraccion = false;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                presionoInteraccion = true;
            if (Gamepad.current != null && (Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame))
                presionoInteraccion = true;

            if (presionoInteraccion)
            {
                Examinar();
            }
        }
        else if (jugadorEnRango)
        {
            jugadorEnRango = false;
            if (Level3Controller.Instance != null)
            {
                Level3Controller.Instance.LimpiarPrompt($"[E] / [X] Examinar {(esExpedientePrincipal ? "el expediente confidencial" : "la pista")}: {nombrePista}");
            }
        }
    }

    public void Examinar()
    {
        if (recolectada) return;
        recolectada = true;
        jugadorEnRango = false;

        if (Level3Controller.Instance != null)
        {
            Level3Controller.Instance.LimpiarPrompt("");
            Level3Controller.Instance.RegistrarPista(nombrePista, esExpedientePrincipal);
        }

        if (efectoBrillo != null)
            efectoBrillo.SetActive(false);

        // Desactivar colisión y renderers del objeto en el suelo
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
            r.enabled = false;

        // Abrir Lector Inmersivo de Documentos
        if (LectorDocumentoUI.Instance != null)
        {
            string sello = esExpedientePrincipal
                ? "— EXPEDIENTE CLASIFICADO // NIVEL DE ACCESO ULTRA —"
                : "— EVIDENCIA FOTOGRÁFICA // DIVISIÓN DE SEGURIDAD —";

            LectorDocumentoUI.Instance.MostrarDocumento(
                nombrePista,
                descripcionPista,
                imagenNota,
                sello,
                alCerrar: () =>
                {
                    if (Level3Controller.Instance != null)
                    {
                        Level3Controller.Instance.MostrarMensaje($"Pista registrada en tu bitácora: {nombrePista}", 4.5f);
                    }
                }
            );
        }
        else if (notasUI != null && imagenNota != null)
        {
            notasUI.MostrarNota(imagenNota);
        }

        Destroy(gameObject, 1f);
    }
}
