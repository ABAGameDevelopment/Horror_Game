using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CinematicaEscapeBicicleta : MonoBehaviour
{
    [Header("Referencias de Cinemática")]
    [SerializeField] private Transform transformBicicleta;
    [SerializeField] private float duracionCinematica = 5.5f;

    public void IniciarEscapeCinematico(Transform biciTransform)
    {
        transformBicicleta = biciTransform;
        StartCoroutine(SecuenciaEscape());
    }

    private IEnumerator SecuenciaEscape()
    {
        // 1. Desactivar controles de jugador y ocultar su cámara
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador != null)
        {
            PlayerController pc = jugador.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;

            CharacterController cc = jugador.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Camera playerCam = jugador.GetComponentInChildren<Camera>();
            if (playerCam != null) playerCam.enabled = false;
        }

        // 2. Crear Cámara Cinemática de Plano General (Frame 6)
        GameObject camGo = new GameObject("Camara_Cinematica_Escape");
        Camera cineCam = camGo.AddComponent<Camera>();
        cineCam.fieldOfView = 50f;
        camGo.transform.position = new Vector3(3.5f, 2.8f, 137f);
        camGo.transform.rotation = Quaternion.Euler(14f, 195f, 0f);

        // 3. Audio de escape: timbre, cadena y lamento distante
        AudioSource audioSource = camGo.AddComponent<AudioSource>();
        audioSource.PlayOneShot(GenerarAudioPedaleo(), 0.95f);

        // 4. Canvas cinematográfico con mensaje dramático
        Canvas canvas = RuntimeUI.CrearCanvas("Canvas_EscapeFinal", 50);
        Image fadeFondo = RuntimeUI.CrearPanel(canvas.transform, "FadeFondo", new Color(0f, 0f, 0f, 0f));
        RuntimeUI.Estirar(fadeFondo.rectTransform, 0f, 0f, 0f, 0f);

        TextMeshProUGUI textoEscape = RuntimeUI.CrearTituloHorror(
            canvas.transform,
            "TextoCinematica",
            "PARA DETENER AL ENTE...\nDEBES REGRESAR AL LUGAR DEL QUE ACABAS DE ESCAPAR",
            26f,
            new Color(0.95f, 0.25f, 0.25f, 0f)
        );
        RuntimeUI.Anclar(textoEscape.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(900f, 120f));

        // 5. Animación: La cámara se aleja lentamente mientras la bicicleta se interna en la niebla
        float tiempo = 0f;
        Vector3 posInicialBici = transformBicicleta != null ? transformBicicleta.position : new Vector3(4.8f, 0f, 130f);
        Vector3 posCamInicial = camGo.transform.position;
        Vector3 posCamFinal = posCamInicial + new Vector3(-3f, 4f, 18f); // Alejamiento progresivo de cámara

        while (tiempo < duracionCinematica)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionCinematica;

            // Movimiento de la bicicleta alejándose hacia el inicio de la carretera (rumbo a la universidad)
            if (transformBicicleta != null)
            {
                transformBicicleta.position = Vector3.Lerp(posInicialBici, posInicialBici + new Vector3(0f, 0f, -45f), t);
            }

            // Cámara alejándose (Plano general)
            camGo.transform.position = Vector3.Lerp(posCamInicial, posCamFinal, t * 0.65f);
            camGo.transform.LookAt(transformBicicleta != null ? transformBicicleta.position : Vector3.zero);

            // Fade in de texto y pantalla negra progresiva
            if (t > 0.45f)
            {
                float alphaFade = (t - 0.45f) / 0.55f;
                fadeFondo.color = new Color(0f, 0f, 0f, alphaFade);
                textoEscape.color = new Color(0.95f, 0.25f, 0.25f, alphaFade);
            }

            yield return null;
        }

        yield return new WaitForSeconds(1.2f);

        // 6. Completar nivel en el gestor
        if (LevelStateManager.Instance != null)
        {
            LevelStateManager.Instance.CompletarNivel();
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }

    private AudioClip GenerarAudioPedaleo()
    {
        int samplerate = 44100;
        int samples = (int)(samplerate * 5.0f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samplerate;
            // Campana ring-ring inicial
            float ring = 0f;
            if (t < 0.6f)
            {
                ring = Mathf.Sin(2f * Mathf.PI * 1900f * t) * Mathf.Exp(-Mathf.Repeat(t, 0.2f) * 14f) * 0.7f;
            }

            // Pedaleo y cadena rítmica
            float pedaleo = Mathf.Sin(2f * Mathf.PI * 3.5f * t);
            float cadena = (UnityEngine.Random.value * 2f - 1f) * Mathf.Abs(pedaleo) * 0.25f;

            // Rugido lejano en el fondo
            float roar = 0f;
            if (t > 1.8f && t < 4.2f)
            {
                float tR = t - 1.8f;
                roar = Mathf.Sin(2f * Mathf.PI * 85f * tR) * (UnityEngine.Random.value * 2f - 1f) * 0.35f * Mathf.Sin(tR * 1.2f);
            }

            data[i] = Mathf.Clamp(ring + cadena + roar, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SFX_EscapeBicicletaCompleto", samples, 1, samplerate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
