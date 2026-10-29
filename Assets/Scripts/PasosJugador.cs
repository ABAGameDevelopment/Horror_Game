
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PasosJugador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CharacterController controlador;
    [SerializeField] private AudioSource audioPasos;

    [Header("Sonidos")]
    [SerializeField] private AudioClip sonidoCaminar;
    [SerializeField] private AudioClip sonidoCorrer;

    [Header("Configuracion")]
    [SerializeField] private float velocidadMinima = 0.15f;
    [SerializeField] private float velocidadParaCorrer = 5.5f;

    private void Awake()
    {
        if (controlador == null)
            controlador = GetComponent<CharacterController>();

        if (audioPasos == null)
            audioPasos = GetComponent<AudioSource>();

        audioPasos.playOnAwake = false;
        audioPasos.loop = true;
    }

    private void Update()
    {
        if (controlador == null || audioPasos == null)
            return;

        Vector3 velocidad = controlador.velocity;
        velocidad.y = 0f;

        bool caminando =
            controlador.isGrounded &&
            velocidad.magnitude >= velocidadMinima;

        if (!caminando)
        {
            if (audioPasos.isPlaying)
                audioPasos.Stop();

            return;
        }

        bool corriendo =
            velocidad.magnitude >= velocidadParaCorrer;

        AudioClip sonidoActual = corriendo
            ? sonidoCorrer
            : sonidoCaminar;

        if (sonidoActual == null)
            return;

        // Cambiar de sonido solo cuando cambia entre caminar y correr.
        if (audioPasos.clip != sonidoActual)
        {
            audioPasos.clip = sonidoActual;
            audioPasos.Play();
        }
        else if (!audioPasos.isPlaying)
        {
            audioPasos.Play();
        }
    }
}
