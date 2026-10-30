using System.Collections;
using UnityEngine;

public class NotaInteractuable : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NotasUI notasUI;
    [SerializeField] private ObjetivoUI objetivoUI;

    [Header("Contenido de la nota")]
    [SerializeField] private Sprite imagenNota;

    private bool recogida;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        InteraccionJugador jugador =
            other.GetComponent<InteraccionJugador>();

        if (jugador != null && !recogida)
        {
            jugador.MostrarNota(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        InteraccionJugador jugador =
            other.GetComponent<InteraccionJugador>();

        if (jugador != null)
        {
            jugador.OcultarNota(this);
        }
    }

    public void Interactuar()
    {
        if (recogida)
            return;

        if (notasUI == null || imagenNota == null)
        {
            Debug.LogError(
                "Falta asignar NotasUI o la imagen de la nota.", this);
            return;
        }

        recogida = true;

        // Mostrar la imagen de la nota.
        notasUI.MostrarNota(imagenNota);

        // Actualizar el progreso del objetivo.
        if (objetivoUI != null)
        {
            objetivoUI.RegistrarNotaEncontrada();
        }

        // Desaparecer la nota fisica.
        gameObject.SetActive(false);
    }
}
