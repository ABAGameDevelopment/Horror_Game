using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarEscapeController : MonoBehaviour
{
    [Header("Progreso de Reparacion")]
    [SerializeField] private bool tieneBateria = false;
    [SerializeField] private bool tieneAceite = false;
    [SerializeField] private bool tieneGasolina = false;
    [SerializeField] private bool tieneLlaves = false;

    [Header("Instalacion en el Vehiculo")]
    [SerializeField] private bool bateriaInstalada = false;
    [SerializeField] private bool aceiteLlenado = false;
    [SerializeField] private bool gasolinaCargada = false;

    [Header("Referencias de Escena")]
    [SerializeField] private Transform portonSalida;
    [SerializeField] private float velocidadEscape = 18f;

    private bool escapeIniciado = false;
    private Transform playerTransform;

    public bool EstaListoParaArrancar => bateriaInstalada && aceiteLlenado && gasolinaCargada && tieneLlaves;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null) playerTransform = player.transform;

        if (portonSalida == null)
        {
            GameObject gateGo = GameObject.Find("Porton_Salida_Estacionamiento");
            if (gateGo != null) portonSalida = gateGo.transform;
        }
    }

    private void Update()
    {
        if (escapeIniciado) return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player != null) playerTransform = player.transform;
            return;
        }

        float distanciaAlAuto = Vector3.Distance(transform.position, playerTransform.position);
        if (distanciaAlAuto <= 4.0f)
        {
            // Instalar recursos si el jugador los tiene
            if (tieneBateria && !bateriaInstalada)
            {
                bateriaInstalada = true;
                Debug.Log("<color=green>[Auto]</color> ¡Bateria instalada en el motor!");
            }
            if (tieneAceite && !aceiteLlenado)
            {
                aceiteLlenado = true;
                Debug.Log("<color=green>[Auto]</color> ¡Aceite vertido en el motor!");
            }
            if (tieneGasolina && !gasolinaCargada)
            {
                gasolinaCargada = true;
                Debug.Log("<color=green>[Auto]</color> ¡Tanque llenado con gasolina!");
            }

            // Si todo está listo y el jugador tiene las llaves, permitir arrancar al pulsar E o F
            if (EstaListoParaArrancar && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F)))
            {
                IniciarSecuenciaEscape();
            }
        }
    }

    public void RegistrarItemRecogido(TipoItemLevel2 tipo)
    {
        switch (tipo)
        {
            case TipoItemLevel2.Bateria:
                tieneBateria = true;
                break;
            case TipoItemLevel2.Aceite:
                tieneAceite = true;
                break;
            case TipoItemLevel2.Gasolina:
                tieneGasolina = true;
                break;
            case TipoItemLevel2.Llaves:
                tieneLlaves = true;
                break;
        }

        Debug.Log(string.Format("<color=cyan>[Progreso Auto]</color> Bateria:{0} | Aceite:{1} | Gasolina:{2} | Llaves:{3}",
            tieneBateria, tieneAceite, tieneGasolina, tieneLlaves));
    }

    public void IniciarSecuenciaEscape()
    {
        if (escapeIniciado) return;
        escapeIniciado = true;

        Debug.Log("<color=red>[ESCAPE]</color> ¡Motor encendido! Acelerando hacia el portón...");
        StartCoroutine(SecuenciaEmbestirPorton());
    }

    private IEnumerator SecuenciaEmbestirPorton()
    {
        // Ocultar al jugador a pie ya que subió al vehículo
        if (playerTransform != null)
        {
            playerTransform.gameObject.SetActive(false);
        }

        // Acelerar hacia el portón (eje Z positivo)
        float tiempo = 0f;
        while (tiempo < 3.5f)
        {
            tiempo += Time.deltaTime;
            transform.position += transform.forward * velocidadEscape * Time.deltaTime;

            // Al chocar con el portón
            if (portonSalida != null && Vector3.Distance(transform.position, portonSalida.position) < 4.5f)
            {
                Rigidbody rb = portonSalida.GetComponent<Rigidbody>();
                if (rb == null) rb = portonSalida.gameObject.AddComponent<Rigidbody>();
                rb.mass = 50f;
                rb.AddForce((transform.forward + Vector3.up * 0.3f) * 600f, ForceMode.Impulse);
            }

            yield return null;
        }

        Debug.Log("<color=green>[NIVEL 2 COMPLETADO]</color> ¡Atravesaste el portón y escapaste del ente!");
    }
}
