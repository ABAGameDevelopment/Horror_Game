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

    private void Start()
    {
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
                    bool lista = Level3Controller.Instance.PuedeUsarBicicleta();
                    string prompt = lista
                        ? "[E] / [X] Montar la bicicleta y regresar a la universidad"
                        : "Bicicleta: Aún necesitas encontrar el expediente sobre la criatura";
                    Level3Controller.Instance.MostrarPrompt(prompt);
                }
            }

            bool presionoInteraccion = false;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                presionoInteraccion = true;
            if (Gamepad.current != null && (Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame))
                presionoInteraccion = true;

            if (presionoInteraccion)
            {
                if (Level3Controller.Instance != null && Level3Controller.Instance.PuedeUsarBicicleta())
                {
                    interactuado = true;
                    Level3Controller.Instance.LimpiarPrompt("");
                    if (indicadorVisual != null) indicadorVisual.SetActive(false);

                    CinematicaEscapeBicicleta escape = gameObject.AddComponent<CinematicaEscapeBicicleta>();
                    escape.IniciarEscapeCinematico(transform);
                }
                else if (Level3Controller.Instance != null)
                {
                    Level3Controller.Instance.MostrarMensaje(Level3Controller.Instance.ObtenerMensajeBicicleta(), 4f);
                }
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
}
