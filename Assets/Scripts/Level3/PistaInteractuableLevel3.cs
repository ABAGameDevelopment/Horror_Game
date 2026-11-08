#pragma warning disable 0414
using UnityEngine;
using UnityEngine.InputSystem;

public class PistaInteractuableLevel3 : MonoBehaviour
{
    [Header("Configuracion de la Pista")]
    [SerializeField] private string nombrePista = "Fotografia de la Universidad";
    [SerializeField] [TextArea(2, 5)] private string descripcionPista = "Una fotografia aerea del campus donde se aprecian anomalias en el laboratorio subterraneo.";
    [SerializeField] private bool esExpedientePrincipal = false;
    [SerializeField] private Sprite imagenNota;
    [SerializeField] private float distanciaInteraccion = 3.2f;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject efectoBrillo;
    [SerializeField] private bool rotarSuavemente = true;

    private bool recolectada = false;
    private bool jugadorEnRango = false;
    private Transform transformJugador;
    private NotasUI notasUI;

    private void Start()
    {
        notasUI = NotasUI.Instance != null ? NotasUI.Instance : UnityEngine.Object.FindAnyObjectByType<NotasUI>();
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

        // Desactivar colision y mallas en el suelo
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
            r.enabled = false;

        // Mostrar la imagen de la nota en pantalla exactamente como en el Nivel 1
        NotasUI nUI = NotasUI.Instance != null ? NotasUI.Instance : UnityEngine.Object.FindAnyObjectByType<NotasUI>();
        if (nUI == null)
        {
            GameObject nGo = new GameObject("NotasUI_Auto");
            nUI = nGo.AddComponent<NotasUI>();
        }

        if (nUI != null)
        {
            nUI.MostrarNota(imagenNota);
        }

        Destroy(gameObject, 5f);
    }
}
