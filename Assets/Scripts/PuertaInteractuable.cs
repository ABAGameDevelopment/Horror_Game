
using System.Collections;
using UnityEngine;

public class PuertaInteractuable : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private InteraccionJugador jugador;

    [Header("Sonido de apertura")]
    [SerializeField] private AudioSource audioApertura;
    [SerializeField] private AudioClip sonidoApertura;

    [Header("Apertura")]
    [SerializeField] private float anguloApertura = 90f;
    [SerializeField] private float velocidadApertura = 2f;

    private bool abierta;
    public bool EstaAbierta => abierta;
    private bool abriendo;
    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;

    private void Awake()
    {
        rotacionCerrada = transform.localRotation;

        rotacionAbierta = rotacionCerrada *
            Quaternion.Euler(0f, anguloApertura, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        jugador = other.GetComponent<InteraccionJugador>();

        if (jugador != null && !abierta)
            jugador.MostrarPuerta(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (jugador != null)
            jugador.OcultarPuerta(this);

        jugador = null;
    }

    public void Interactuar()
    {
        if (abierta || abriendo)
            return;

        abierta = true;

        if (jugador != null)
            jugador.OcultarPuerta(this);

        StartCoroutine(AbrirPuerta());
    }

    private IEnumerator AbrirPuerta()
    {
        abriendo = true;

        // Reproducir el sonido al comenzar a abrir.
        if (audioApertura != null && sonidoApertura != null)
            audioApertura.PlayOneShot(sonidoApertura);

        float velocidad = Mathf.Max(0.01f, velocidadApertura);

        while (Quaternion.Angle(
            transform.localRotation, rotacionAbierta) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                rotacionAbierta,
                Time.deltaTime * velocidad);

            yield return null;
        }

        transform.localRotation = rotacionAbierta;
        abriendo = false;
    }
}
