
using System.Collections;
using UnityEngine;

public class PuertaSusto : MonoBehaviour
{
    [Header("Referencias de la puerta")]
    [SerializeField] private Transform pivotePuerta;
    [SerializeField] private AudioSource audioPuerta;
    [SerializeField] private AudioClip sonidoPortazo;

    [Header("Segundo sonido en otra zona")]
    [SerializeField] private AudioSource audioSegundoSonido;
    [SerializeField] private AudioClip segundoSonido;
    [SerializeField] private float retrasoSegundoSonido = 2f;

    [Header("Cierre")]
    [SerializeField] private float anguloCierre = 90f;
    [SerializeField] private float duracionCierre = 0.15f;
    [SerializeField] private float retrasoCierre = 0.1f;

    private Quaternion rotacionAbierta;
    private Quaternion rotacionCerrada;
    private bool activada;

    private void Awake()
    {
        if (pivotePuerta == null)
        {
            Debug.LogError("Falta asignar el pivote de la puerta.", this);
            enabled = false;
            return;
        }

        rotacionAbierta = pivotePuerta.localRotation;

        rotacionCerrada = rotacionAbierta *
            Quaternion.Euler(0f, anguloCierre, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activada || !other.CompareTag("Player"))
            return;

        activada = true;
        StartCoroutine(CerrarDeGolpe());
    }

    private IEnumerator CerrarDeGolpe()
    {
        if (retrasoCierre > 0f)
            yield return new WaitForSeconds(retrasoCierre);

        float tiempo = 0f;
        float duracion = Mathf.Max(0.01f, duracionCierre);

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(tiempo / duracion);
            float suavizado = progreso * progreso;

            pivotePuerta.localRotation = Quaternion.Slerp(
                rotacionAbierta,
                rotacionCerrada,
                suavizado
            );

            yield return null;
        }

        pivotePuerta.localRotation = rotacionCerrada;

        // Primer sonido: portazo en la puerta.
        if (audioPuerta != null && sonidoPortazo != null)
            audioPuerta.PlayOneShot(sonidoPortazo);

        // Esperar 2 segundos después del cierre.
        yield return new WaitForSeconds(retrasoSegundoSonido);

        // Segundo sonido: se reproduce desde su propia ubicación.
        if (audioSegundoSonido != null && segundoSonido != null)
            audioSegundoSonido.PlayOneShot(segundoSonido);
    }
}
