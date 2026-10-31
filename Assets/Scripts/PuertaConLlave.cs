
using System.Collections;
using UnityEngine;

public enum TipoPuerta
{
    Auditorio,
    Salida
}

public class PuertaConLlave : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform pivotePuerta;
    [SerializeField] private ObjetivoUI objetivoUI;

    [Header("Sonido del seguro")]
    [SerializeField] private AudioSource audioSeguro;
    [SerializeField] private AudioClip sonidoSeguro;

    [Header("Sonido de apertura")]
    [SerializeField] private AudioSource audioApertura;
    [SerializeField] private AudioClip sonidoApertura;

    [Header("Sonido de puerta bloqueada")]
    [SerializeField] private AudioSource audioBloqueada;
    [SerializeField] private AudioClip sonidoBloqueada;

    [Header("Apertura")]
    [SerializeField] private float anguloApertura = 90f;
    [SerializeField] private float velocidadApertura = 2f;
    [SerializeField] private float retrasoApertura = 0.3f;

    [Header("Configuración de puerta")]
    [SerializeField] private TipoPuerta tipoPuerta;

    private InteraccionJugador jugador;
    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;

    private bool abierta;
    public bool EstaAbierta => abierta;
    private bool abriendo;

    private void Awake()
    {
        if (pivotePuerta == null)
        {
            Debug.LogError(
                "Asigna el pivote de la puerta en PuertaConLlave.", this);
            enabled = false;
            return;
        }

        rotacionCerrada = pivotePuerta.localRotation;

        rotacionAbierta = rotacionCerrada *
            Quaternion.Euler(0f, anguloApertura, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        jugador = other.GetComponent<InteraccionJugador>();

        if (jugador != null && !abierta)
            jugador.MostrarPuertaConLlave(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (jugador != null)
            jugador.OcultarPuertaConLlave(this);

        jugador = null;
    }

    public void Interactuar(InteraccionJugador quienInteractua)
    {
        if (abierta || abriendo)
            return;

        if (quienInteractua == null)
            return;

        bool tieneLlaveCorrecta = false;

        // Comprobar qué llave necesita esta puerta.
        if (tipoPuerta == TipoPuerta.Auditorio)
        {
            tieneLlaveCorrecta = quienInteractua.TieneLlaveDe(
                LlaveInteractuable.TipoLlave.Auditorio
            );
        }
        else if (tipoPuerta == TipoPuerta.Salida)
        {
            tieneLlaveCorrecta = quienInteractua.TieneLlaveDe(
                LlaveInteractuable.TipoLlave.Salida
            );
        }

        // Si no tiene la llave correspondiente.
        if (!tieneLlaveCorrecta)
        {
            if (audioBloqueada != null && sonidoBloqueada != null)
                audioBloqueada.PlayOneShot(sonidoBloqueada);

            quienInteractua.MostrarMensajeTemporal(
                "Necesitas la llave correspondiente para abrir esta puerta"
            );

            return;
        }

        abriendo = true;
        StartCoroutine(AbrirPuerta(quienInteractua));
    }

    private IEnumerator AbrirPuerta(InteraccionJugador quienInteractua)
    {
        // Sonido del seguro.
        if (audioSeguro != null && sonidoSeguro != null)
            audioSeguro.PlayOneShot(sonidoSeguro);

        if (retrasoApertura > 0f)
            yield return new WaitForSeconds(retrasoApertura);

        // Sonido de la puerta al comenzar a abrirse.
        if (audioApertura != null && sonidoApertura != null)
            audioApertura.PlayOneShot(sonidoApertura);

        float velocidad = Mathf.Max(0.01f, velocidadApertura);

        while (Quaternion.Angle(
            pivotePuerta.localRotation, rotacionAbierta) > 0.1f)
        {
            pivotePuerta.localRotation = Quaternion.Slerp(
                pivotePuerta.localRotation,
                rotacionAbierta,
                Time.deltaTime * velocidad
            );

            yield return null;
        }

        pivotePuerta.localRotation = rotacionAbierta;

        abierta = true;
        abriendo = false;

        // Actualizar los objetivos al abrir el auditorio.
        if (objetivoUI != null)
        {
            if (tipoPuerta == TipoPuerta.Auditorio)
            {
                objetivoUI.CompletarObjetivoAuditorio();
            }
            else if (tipoPuerta == TipoPuerta.Salida)
            {
                objetivoUI.CompletarObjetivoFinal();
            }
        }

        if (jugador != null)
            jugador.OcultarPuertaConLlave(this);
    }
}
