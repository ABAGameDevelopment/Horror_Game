using UnityEngine;

public class LlaveInteractuable : MonoBehaviour
{
    public enum TipoLlave
    {
        Auditorio,
        Salida
    }

    [Header("Configuración")]
    [SerializeField] private TipoLlave tipoLlave;
    public TipoLlave Tipo => tipoLlave;

    [Header("Referencias")]
    [SerializeField] private ObjetivoUI objetivoUI;

    private InteraccionJugador jugador;
    private bool recogida;

    private void OnTriggerEnter(Collider other)
    {
        if (recogida || !other.CompareTag("Player"))
            return;

        jugador = other.GetComponent<InteraccionJugador>();

        if (jugador != null)
            jugador.MostrarLlave(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (jugador != null)
            jugador.OcultarLlave(this);

        jugador = null;
    }

    public void Recoger(InteraccionJugador quienRecoge)
    {
        if (recogida || quienRecoge == null)
            return;

        recogida = true;

        // Registrar que el jugador tiene una llave.
        quienRecoge.RecogerLlave(tipoLlave);

        quienRecoge.OcultarLlave(this);

        // Si es la llave de salida, actualizar el objetivo.
        if (tipoLlave == TipoLlave.Salida && objetivoUI != null)
        {
            objetivoUI.RegistrarLlaveSalidaEncontrada();
        }

        // Desaparecer del escenario.
        gameObject.SetActive(false);
    }
}