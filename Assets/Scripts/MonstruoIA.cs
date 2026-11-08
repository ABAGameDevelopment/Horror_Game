using System.Collections.Generic;

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
    [SerializeField] private AudioSource audioEnemy;
    [SerializeField] private AudioClip walkEnemy;
    [SerializeField] private float distanciaVision = 9f;
    [SerializeField, Range(0f, 180f)]
    private float anguloVision = 100f;

    [SerializeField] private LayerMask obstaculos;

    [Header("Persecucion")]
    [SerializeField] private AudioClip runEnemy;
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
        if (agente == null)
            agente = GetComponent<NavMeshAgent>();

        if (jugador == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) jugador = p.transform;
            else
            {
                PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
                if (pc != null) jugador = pc.transform;
            }
        }

        if (agente == null || jugador == null)
        {
            Debug.LogError("Faltan referencias críticas en MonstruoIA (agente o jugador).", this);
            enabled = false;
            return;
        }

        // Asegurar que el monstruo esté colocado exactamente sobre el NavMesh
        if (!agente.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 12f, NavMesh.AllAreas))
            {
                agente.Warp(hit.position);
            }
        }

        agente.speed = velocidadPatrulla;
        agente.isStopped = false;
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

        ActualizarAudioMovimiento();
    }

    private void Merodear()
    {
        if (agente == null || !agente.isOnNavMesh)
            return;

        if (Time.time < siguienteIntentoPatrulla)
            return;

        if (agente.pathPending)
            return;

        bool llegoAlDestino =
            !agente.hasPath ||
            agente.remainingDistance <= agente.stoppingDistance + 0.4f;

        if (llegoAlDestino)
        {
            ElegirDestinoPatrulla();
        }
    }


    private void ElegirDestinoPatrulla()
    {
        if (agente == null || !agente.isOnNavMesh)
            return;

        Vector3 destinoElegido = Vector3.zero;
        bool destinoValido = false;

        // 1. Probar waypoints asignados
        if (puntosPatrullaje != null && puntosPatrullaje.Length > 0)
        {
            List<Vector3> destinosValidos = new List<Vector3>();
            for (int i = 0; i < puntosPatrullaje.Length; i++)
            {
                Transform punto = puntosPatrullaje[i];
                if (punto == null) continue;
                if (ultimoPuntoSeleccionado != null && punto == ultimoPuntoSeleccionado && puntosPatrullaje.Length > 1)
                    continue;

                if (NavMesh.SamplePosition(punto.position, out NavMeshHit hit, 8f, NavMesh.AllAreas))
                {
                    destinosValidos.Add(hit.position);
                }
            }

            if (destinosValidos.Count > 0)
            {
                destinoElegido = destinosValidos[Random.Range(0, destinosValidos.Count)];
                destinoValido = true;
            }
        }

        // 2. Si no hay waypoints o no están en el NavMesh, patrullar a lo largo de la carretera
        if (!destinoValido)
        {
            float randomZ = Random.Range(18f, 135f);
            float randomX = Random.Range(-4f, 4f);
            Vector3 randomPoint = new Vector3(randomX, 0.15f, randomZ);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit roadHit, 15f, NavMesh.AllAreas))
            {
                destinoElegido = roadHit.position;
                destinoValido = true;
            }
            else
            {
                Vector3 randomCircle = transform.position + Random.insideUnitSphere * 15f;
                if (NavMesh.SamplePosition(randomCircle, out NavMeshHit circleHit, 15f, NavMesh.AllAreas))
                {
                    destinoElegido = circleHit.position;
                    destinoValido = true;
                }
            }
        }

        if (destinoValido)
        {
            agente.isStopped = false;
            agente.speed = velocidadPatrulla;
            agente.SetDestination(destinoElegido);
            siguienteIntentoPatrulla = Time.time + 1.2f;
        }
        else
        {
            siguienteIntentoPatrulla = Time.time + 1.0f;
        }
    }


    private void DetectarJugador()
    {
        if (CarEscapeController.EstaEscondidoBajoAuto)
            return;

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
        if (CarEscapeController.EstaEscondidoBajoAuto)
        {
            // El jugador se metió debajo del auto mientras era perseguido.
            // Si transcurre el tiempo de búsqueda sin verlo, el monstruo desiste y regresa a merodear.
            if (Time.time - ultimoTiempoVisto > tiempoBusqueda)
            {
                VolverAMerodear();
            }
            return;
        }

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
        if (CarEscapeController.EstaEscondidoBajoAuto)
            return false;

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
        if (capturado || CarEscapeController.EstaEscondidoBajoAuto)
            return;

        capturado = true;
        estadoActual = Estado.Capturando;

        if (audioEnemy != null)
            audioEnemy.Stop();

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

    private void ActualizarAudioMovimiento()
    {
        if (audioEnemy == null)
            return;

        // Si el monstruo está detenido, no reproducir pasos.
        if (capturado ||
            agente == null ||
            !agente.enabled ||
            !agente.isOnNavMesh ||
            agente.isStopped ||
            agente.velocity.sqrMagnitude < 0.04f)
        {
            if (audioEnemy.isPlaying)
                audioEnemy.Stop();

            return;
        }

        // Seleccionar el sonido dependiendo del estado.
        AudioClip sonidoDeseado =
            estadoActual == Estado.Persiguiendo
            ? runEnemy
            : walkEnemy;

        if (sonidoDeseado == null)
        {
            audioEnemy.Stop();
            return;
        }

        // Cambiar el sonido solamente cuando sea necesario.
        if (audioEnemy.clip != sonidoDeseado)
        {
            audioEnemy.Stop();

            audioEnemy.clip = sonidoDeseado;
            audioEnemy.loop = true;
            audioEnemy.Play();
        }
        else if (!audioEnemy.isPlaying)
        {
            audioEnemy.Play();
        }
    }
}
