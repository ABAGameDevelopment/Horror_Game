
using System.Collections;
using UnityEngine;

public class CinematicaInicioNivel3 : MonoBehaviour
{
    [Header("Movimiento del vehiculo")]
    [SerializeField] private float duracionMovimiento = 2f;
    [SerializeField] private float velocidad = 12f;
    [SerializeField] private float duracionDesaceleracion = 1.2f;

    [Header("Jugador")]
    [SerializeField] private GameObject jugador;

    [Header("Camaras")]
    [SerializeField] private Camera camaraCinematica;
    [SerializeField] private Camera camaraJugador;

    [Header("Camara cinematografica")]
    [SerializeField] private float alturaCamara = 14f;
    [SerializeField] private float distanciaCamara = 0f;
    [SerializeField] private float suavizadoCamara = 5f;

    private bool cinematicaTerminada = false;

    public bool CinematicaTerminada => cinematicaTerminada;

    private void Start()
    {
        if (camaraCinematica == null ||
            camaraJugador == null ||
            jugador == null)
        {
            Debug.LogError("Falta asignar alguna referencia de la cinematica.");
            enabled = false;
            return;
        }

        // Ocultar y desactivar al jugador durante la cinematica.
        jugador.SetActive(false);

        // Activar la camara cinematografica.
        camaraCinematica.enabled = true;

        ActualizarCamara(true);

        StartCoroutine(SecuenciaInicial());
    }

    private IEnumerator SecuenciaInicial()
    {
        float tiempo = 0f;

        float inicioDesaceleracion =
            Mathf.Max(0f, duracionMovimiento - duracionDesaceleracion);

        while (tiempo < duracionMovimiento)
        {
            float velocidadActual = velocidad;

            // Desaceleracion progresiva al final del recorrido.
            if (tiempo >= inicioDesaceleracion)
            {
                float tiempoFrenado = tiempo - inicioDesaceleracion;

                float progreso = duracionDesaceleracion > 0f
                    ? Mathf.Clamp01(tiempoFrenado / duracionDesaceleracion)
                    : 1f;

                float desaceleracion = Mathf.SmoothStep(0f, 1f, progreso);

                velocidadActual = Mathf.Lerp(velocidad, 0f, desaceleracion);
            }

            transform.position +=
                transform.forward * velocidadActual * Time.deltaTime;

            tiempo += Time.deltaTime;

            yield return null;
        }

        cinematicaTerminada = true;

        // Separar al jugador del automovil conservando su posicion.
        jugador.transform.SetParent(null, true);

        // Volver a activar al jugador.
        jugador.SetActive(true);

        // Cambiar a la camara del jugador.
        camaraCinematica.enabled = false;
        camaraJugador.enabled = true;

        Debug.Log("Cinematica terminada. Jugador activado.");
    }

    private void LateUpdate()
    {
        if (camaraCinematica == null || !camaraCinematica.enabled)
            return;

        ActualizarCamara(false);
    }

    private void ActualizarCamara(bool instantanea)
    {
        Vector3 posicionDeseada =
            transform.position +
            Vector3.up * alturaCamara -
            transform.forward * distanciaCamara;

        Quaternion rotacionDeseada = Quaternion.LookRotation(
            transform.position - posicionDeseada,
            transform.forward
        );

        if (instantanea)
        {
            camaraCinematica.transform.position = posicionDeseada;
            camaraCinematica.transform.rotation = rotacionDeseada;
        }
        else
        {
            float factor = 1f - Mathf.Exp(-suavizadoCamara * Time.deltaTime);

            camaraCinematica.transform.position = Vector3.Lerp(
                camaraCinematica.transform.position,
                posicionDeseada,
                factor
            );

            camaraCinematica.transform.rotation = Quaternion.Slerp(
                camaraCinematica.transform.rotation,
                rotacionDeseada,
                factor
            );
        }
    }
}
