
using System.Collections;
using UnityEngine;

public class IntroNivel : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform camara;
    [SerializeField] private Light[] luces;

    [Header("Animacion inicial")]
    [SerializeField] private float duracionMirada = 4f;
    [SerializeField] private float anguloMirada = 25f;
    [SerializeField] private float velocidadMirada = 1.5f;
    [SerializeField] private float pausaAntesApagar = 0.5f;

    private Quaternion rotacionInicial;
    private bool introduccionTerminada = false;

    public bool IntroduccionTerminada => introduccionTerminada;

    private void Start()
    {
        if (camara == null)
        {
            Debug.LogError("IntroNivel: asigna la camara del jugador.");
            enabled = false;
            return;
        }

        rotacionInicial = camara.localRotation;

        introduccionTerminada = false;

        StartCoroutine(ReproducirIntroduccion());
    }


    private IEnumerator ReproducirIntroduccion()
    {
        // La cámara observa lentamente el entorno.
        float tiempo = 0f;
        bool lucesApagadas = false;

        while (tiempo < duracionMirada)
        {
            tiempo += Time.deltaTime;

            float progreso = tiempo / duracionMirada;

            // Apagar las luces cuando la animación llegue a la mitad.
            if (!lucesApagadas && progreso >= 0.5f)
            {
                foreach (Light luz in luces)
                {
                    if (luz != null)
                        luz.enabled = false;
                }

                lucesApagadas = true;
            }

            // Mira a la izquierda, al centro y a la derecha.
            float desplazamiento =
                Mathf.Sin(progreso * Mathf.PI * 4f) * anguloMirada;

            float inclinacion =
                Mathf.Sin(progreso * Mathf.PI * 2f) * 5f;

            Quaternion objetivo =
                rotacionInicial *
                Quaternion.Euler(inclinacion, desplazamiento, 0f);

            camara.localRotation = Quaternion.Slerp(
                camara.localRotation,
                objetivo,
                Time.deltaTime * velocidadMirada
            );

            yield return null;
        }

        // Regresa suavemente a la posición original.
        float retorno = 0f;
        Quaternion rotacionActual = camara.localRotation;

        while (retorno < 0.6f)
        {
            retorno += Time.deltaTime;

            camara.localRotation = Quaternion.Slerp(
                rotacionActual,
                rotacionInicial,
                retorno / 0.6f
            );

            yield return null;
        }

        camara.localRotation = rotacionInicial;

        yield return new WaitForSeconds(pausaAntesApagar);

        // El jugador ya puede controlar la cámara y moverse.
        introduccionTerminada = true;
    }

}
