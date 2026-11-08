
using UnityEngine;
using UnityEngine.AI;

public class MonstruoIA : MonoBehaviour
{
    public enum Estado
    {
        Merodeando,
        Persiguiendo,
        Capturando
    }

    [Header("Referencias")]
    [SerializeField] private NavMeshAgent agente;
    [SerializeField] private Transform jugador;
    [SerializeField] private Transform puntoVision;
    [SerializeField] private Transform[] puntosPatrullaje;
    [SerializeField] private MuerteJugador muerteJugador;

    [Header("Velocidades")]
    [SerializeField] private float velocidadPatrulla = 1.8f;
    [SerializeField] private float velocidadPersecucion = 4.5f;

    [Header("Deteccion")]
    [SerializeField] private float distanciaVision = 9f;
    [SerializeField, Range(0f, 180f)]
    private float anguloVision = 100f;

    [SerializeField] private LayerMask obstaculos;

    [Header("Persecucion")]
    [SerializeField] private float distanciaPerdida = 18f;
    [SerializeField] private float tiempoBusqueda = 5f;

    [Header("Captura")]
    [SerializeField] private float distanciaCaptura = 1.5f;

    private Estado estadoActual = Estado.Merodeando;

    private int indiceAnterior = -1;
    private Transform ultimoPuntoSeleccionado;
    private float ultimoTiempoVisto;
    private bool capturado;
    private NavMeshPath caminoTemporal;
    private float siguienteIntentoPatrulla;

    private void Awake()
    {
        if (agente == null)
            agente = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (agente == null || jugador == null ||
            puntosPatrullaje == null || puntosPatrullaje.Length == 0)
        {
            Debug.LogError(
                "Faltan referencias en MonstruoIA.", this);
            enabled = false;
            return;
        }

        agente.speed = velocidadPatrulla;
        caminoTemporal = new NavMeshPath();
        ElegirDestinoPatrulla();
    }

    private void Update()
    {
        if (capturado)
            return;

        switch (estadoActual)
        {
            case Estado.Merodeando:
                Merodear();
                DetectarJugador();
                break;

            case Estado.Persiguiendo:
                Perseguir();
                break;

            case Estado.Capturando:
                break;
        }
    }

    private void Merodear()
    {
        if (Time.time < siguienteIntentoPatrulla)
            return;

        if (agente.pathPending)
            return;

        bool llegoAlDestino =
            agente.hasPath &&
            agente.remainingDistance <= agente.stoppingDistance + 0.2f;

        bool caminoInvalido =
            !agente.hasPath ||
            agente.pathStatus != NavMeshPathStatus.PathComplete;

        if (llegoAlDestino || caminoInvalido)
        {
            ElegirDestinoPatrulla();
        }
    }


    private void ElegirDestinoPatrulla()
    {
        if (puntosPatrullaje == null ||
            puntosPatrullaje.Length == 0)
            return;

        Transform[] puntosDisponibles =
            new Transform[puntosPatrullaje.Length];

        int cantidadDisponibles = 0;

        // Buscar los puntos que tengan un camino completo.
        foreach (Transform punto in puntosPatrullaje)
        {
            if (punto == null)
                continue;

            bool existeCamino = agente.CalculatePath(
                punto.position,
                caminoTemporal
            );

            if (existeCamino &&
                caminoTemporal.status == NavMeshPathStatus.PathComplete)
            {
                puntosDisponibles[cantidadDisponibles] = punto;
                cantidadDisponibles++;
            }
        }

        // No hay destinos accesibles.
        if (cantidadDisponibles == 0)
        {
            agente.isStopped = true;
            siguienteIntentoPatrulla = Time.time + 1f;
            return;
        }

        // Construir una lista sin el último punto visitado.
        Transform[] alternativas =
            new Transform[cantidadDisponibles];

        int cantidadAlternativas = 0;

        for (int i = 0; i < cantidadDisponibles; i++)
        {
            if (puntosDisponibles[i] != ultimoPuntoSeleccionado)
            {
                alternativas[cantidadAlternativas] =
                    puntosDisponibles[i];

                cantidadAlternativas++;
            }
        }

        // Si no hay alternativas, se permite repetir el último punto.
        Transform destino;

        if (cantidadAlternativas > 0)
        {
            int indice = Random.Range(0, cantidadAlternativas);
            destino = alternativas[indice];
        }
        else
        {
            destino = puntosDisponibles[0];
        }

        // Guardar el último punto seleccionado.
        ultimoPuntoSeleccionado = destino;

        agente.isStopped = false;
        agente.speed = velocidadPatrulla;
        agente.SetDestination(destino.position);

        siguienteIntentoPatrulla = Time.time + 0.5f;
    }


    private void DetectarJugador()
    {
        Vector3 origen = puntoVision != null
            ? puntoVision.position
            : transform.position + Vector3.up * 1.5f;

        Vector3 destino = jugador.position + Vector3.up * 1f;
        Vector3 direccion = destino - origen;

        float distancia = direccion.magnitude;

        if (distancia > distanciaVision)
            return;

        float angulo = Vector3.Angle(
            transform.forward, direccion);

        if (angulo > anguloVision / 2f)
            return;

        if (Physics.Raycast(
            origen,
            direccion.normalized,
            distancia,
            obstaculos))
        {
            return;
        }

        IniciarPersecucion();
    }

    private void IniciarPersecucion()
    {
        estadoActual = Estado.Persiguiendo;
        ultimoTiempoVisto = Time.time;

        agente.speed = velocidadPersecucion;
        agente.isStopped = false;
    }

    private void Perseguir()
    {
        agente.SetDestination(jugador.position);

        if (PuedeVerJugador())
        {
            ultimoTiempoVisto = Time.time;
        }

        float distancia = Vector3.Distance(
            transform.position, jugador.position);

        if (distancia <= distanciaCaptura)
        {
            CapturarJugador();
            return;
        }

        bool pasoTiempo =
            Time.time - ultimoTiempoVisto > tiempoBusqueda;

        bool demasiadoLejos =
            distancia > distanciaPerdida;

        if (pasoTiempo || demasiadoLejos)
        {
            VolverAMerodear();
        }
    }

    private bool PuedeVerJugador()
    {
        Vector3 origen = puntoVision != null
            ? puntoVision.position
            : transform.position + Vector3.up * 1.5f;

        Vector3 destino = jugador.position + Vector3.up;
        Vector3 direccion = destino - origen;

        float distancia = direccion.magnitude;

        if (distancia > distanciaVision)
            return false;

        if (Vector3.Angle(transform.forward, direccion) >
            anguloVision / 2f)
            return false;

        return !Physics.Raycast(
            origen,
            direccion.normalized,
            distancia,
            obstaculos);
    }

    private void VolverAMerodear()
    {
        estadoActual = Estado.Merodeando;
        ElegirDestinoPatrulla();
    }

    private void CapturarJugador()
    {
        if (capturado)
            return;

        capturado = true;
        estadoActual = Estado.Capturando;

        agente.isStopped = true;
        agente.ResetPath();
        agente.enabled = false;

        Debug.Log("¡El monstruo ha capturado al jugador!");

        if (muerteJugador != null)
        {
            muerteJugador.IniciarMuerte(transform);
        }
        else
        {
            Debug.LogError(
                "No se ha asignado MuerteJugador en MonstruoIA."
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaVision);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaCaptura);
    }
}
