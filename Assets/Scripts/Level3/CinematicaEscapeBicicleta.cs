using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CinematicaEscapeBicicleta : MonoBehaviour
{
    [Header("Referencias de Cinemática")]
    [SerializeField] private Transform transformBicicleta;
    [SerializeField] private float duracionCinematica = 5.8f;

    public void IniciarEscapeCinematico(Transform biciTransform)
    {
        transformBicicleta = biciTransform;
        StartCoroutine(SecuenciaEscape());
    }

    private IEnumerator SecuenciaEscape()
    {
        // 1. Desactivar y neutralizar a cualquier monstruo para que NO ataque ni persiga al jugador durante el escape
        MonstruoIA[] todosMonstruos = UnityEngine.Object.FindObjectsByType<MonstruoIA>(FindObjectsSortMode.None);
        foreach (var m in todosMonstruos)
        {
            if (m != null)
            {
                m.enabled = false;
                UnityEngine.AI.NavMeshAgent nav = m.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (nav != null && nav.isOnNavMesh)
                {
                    nav.isStopped = true;
                    nav.velocity = Vector3.zero;
                }
                AudioSource a = m.GetComponent<AudioSource>();
                if (a != null) a.Stop();
            }
        }

        // 2. Cancelar y bloquear cualquier secuencia de muerte o jumpscare
        MuerteJugador[] muertes = UnityEngine.Object.FindObjectsByType<MuerteJugador>(FindObjectsSortMode.None);
        foreach (var mj in muertes)
        {
            if (mj != null)
            {
                mj.CancelarYBloquearMuerte();
            }
        }

        // 3. Montar al jugador en la bicicleta y ocultar su cápsula del suelo
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador != null)
        {
            PlayerController pc = jugador.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;

            CharacterController cc = jugador.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Camera playerCam = jugador.GetComponentInChildren<Camera>();
            if (playerCam != null) playerCam.enabled = false;

            // Ocultar la cápsula, gafas y renderers del jugador para que no se quede parado en la carretera
            Renderer[] playerRends = jugador.GetComponentsInChildren<Renderer>();
            foreach (var r in playerRends) r.enabled = false;

            Light[] playerLights = jugador.GetComponentsInChildren<Light>();
            foreach (var l in playerLights) l.enabled = false;

            Collider[] playerCols = jugador.GetComponentsInChildren<Collider>();
            foreach (var col in playerCols) col.enabled = false;

            // Emparentar el transform del jugador a la bicicleta para que viaje con ella
            if (transformBicicleta != null)
            {
                jugador.transform.SetParent(transformBicicleta);
                jugador.transform.localPosition = Vector3.zero;
            }
        }

        // 4. Crear el modelo 3D del ciclista montado en la bicicleta
        CrearCiclistaMontado(transformBicicleta);

        // 5. Crear Cámara Cinemática de Plano General
        GameObject camGo = new GameObject("Camara_Cinematica_Escape");
        Camera cineCam = camGo.AddComponent<Camera>();
        cineCam.fieldOfView = 50f;

        Vector3 posInicialBici = transformBicicleta != null ? transformBicicleta.position : new Vector3(5.2f, 0f, 127f);
        // Colocar cámara detrás y ligeramente al lado de la bicicleta
        camGo.transform.position = posInicialBici + new Vector3(-1.8f, 2.4f, 7.5f);
        camGo.transform.LookAt(posInicialBici + Vector3.up * 1f);

        // 6. Audio de escape: timbre, cadena y lamento distante
        AudioSource audioSource = camGo.AddComponent<AudioSource>();
        audioSource.PlayOneShot(GenerarAudioPedaleo(), 0.95f);

        // 7. Canvas cinematográfico con mensaje dramático
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

        // 8. Buscar ruedas de la bicicleta para hacerlas girar
        List<Transform> ruedas = new List<Transform>();
        if (transformBicicleta != null)
        {
            Transform[] allParts = transformBicicleta.GetComponentsInChildren<Transform>();
            foreach (var p in allParts)
            {
                if (p != null && (p.name.ToLower().Contains("wheel") || p.name.ToLower().Contains("rueda")))
                {
                    ruedas.Add(p);
                }
            }
        }

        // 9. Animación: La bicicleta avanza por la carretera rumbo a la universidad y la cámara la sigue
        float tiempo = 0f;
        Vector3 dirEscape = transformBicicleta != null ? transformBicicleta.forward : new Vector3(0f, 0f, -1f);
        dirEscape.y = 0f;
        if (dirEscape.sqrMagnitude < 0.01f) dirEscape = new Vector3(0f, 0f, -1f);
        dirEscape.Normalize();

        Vector3 posCamInicial = camGo.transform.position;
        Vector3 posCamFinal = posCamInicial + new Vector3(-2.5f, 3.5f, 18f); // Elevación y alejamiento progresivo
        Vector3 posFinalBici = posInicialBici + dirEscape * 60f;

        while (tiempo < duracionCinematica)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracionCinematica);
            // Curva suave de aceleración inicial
            float easeT = t * t * (3f - 2f * t);

            // Movimiento de la bicicleta alejándose hacia el inicio de la carretera
            if (transformBicicleta != null)
            {
                transformBicicleta.position = Vector3.Lerp(posInicialBici, posFinalBici, easeT);

                // Girar ruedas
                foreach (var r in ruedas)
                {
                    if (r != null) r.Rotate(Vector3.right * 720f * Time.deltaTime, Space.Self);
                }
            }

            // Cámara cinematográfica siguiendo el trayecto
            camGo.transform.position = Vector3.Lerp(posCamInicial, posCamFinal, t * 0.7f);
            if (transformBicicleta != null)
            {
                camGo.transform.LookAt(transformBicicleta.position + Vector3.up * 1f);
            }

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

        // 10. Completar nivel en el gestor
        if (LevelStateManager.Instance != null)
        {
            LevelStateManager.Instance.CompletarNivel();
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }

    private void CrearCiclistaMontado(Transform bici)
    {
        if (bici == null) return;

        // Si ya existe un ciclista montado previo, no duplicar
        if (bici.Find("Ciclista_Escape") != null) return;

        GameObject ciclista = new GameObject("Ciclista_Escape");
        ciclista.transform.SetParent(bici, false);
        ciclista.transform.localPosition = Vector3.zero;
        ciclista.transform.localRotation = Quaternion.identity;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material matRopa = new Material(shader);
        matRopa.color = new Color(0.12f, 0.15f, 0.20f); // Chaqueta azul oscura

        Material matPantalon = new Material(shader);
        matPantalon.color = new Color(0.08f, 0.08f, 0.09f); // Pantalón oscuro

        Material matPiel = new Material(shader);
        matPiel.color = new Color(0.82f, 0.68f, 0.55f); // Tono de piel

        // 1. Torso inclinado hacia el manubrio
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "Torso";
        torso.transform.SetParent(ciclista.transform, false);
        torso.transform.localPosition = new Vector3(0f, 0.95f, -0.05f);
        torso.transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
        torso.transform.localScale = new Vector3(0.36f, 0.48f, 0.24f);
        Destroy(torso.GetComponent<Collider>());
        torso.GetComponent<MeshRenderer>().material = matRopa;

        // 2. Cabeza
        GameObject cabeza = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cabeza.name = "Cabeza";
        cabeza.transform.SetParent(ciclista.transform, false);
        cabeza.transform.localPosition = new Vector3(0f, 1.28f, 0.10f);
        cabeza.transform.localScale = new Vector3(0.24f, 0.25f, 0.24f);
        Destroy(cabeza.GetComponent<Collider>());
        cabeza.GetComponent<MeshRenderer>().material = matPiel;

        // 3. Gorro / Cabello
        GameObject pelo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pelo.name = "Gorro";
        pelo.transform.SetParent(cabeza.transform, false);
        pelo.transform.localPosition = new Vector3(0f, 0.08f, -0.04f);
        pelo.transform.localScale = new Vector3(1.05f, 0.95f, 1.05f);
        Destroy(pelo.GetComponent<Collider>());
        pelo.GetComponent<MeshRenderer>().material = matPantalon;

        // 4. Brazos hacia el manubrio
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject brazo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            brazo.name = i == -1 ? "Brazo_Izq" : "Brazo_Der";
            brazo.transform.SetParent(ciclista.transform, false);
            brazo.transform.localPosition = new Vector3(i * 0.20f, 0.88f, 0.22f);
            brazo.transform.localRotation = Quaternion.Euler(60f, i * 15f, 0f);
            brazo.transform.localScale = new Vector3(0.08f, 0.22f, 0.08f);
            Destroy(brazo.GetComponent<Collider>());
            brazo.GetComponent<MeshRenderer>().material = matRopa;
        }

        // 5. Piernas hacia los pedales
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject pierna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pierna.name = i == -1 ? "Pierna_Izq" : "Pierna_Der";
            pierna.transform.SetParent(ciclista.transform, false);
            pierna.transform.localPosition = new Vector3(i * 0.16f, 0.55f, 0.05f);
            pierna.transform.localRotation = Quaternion.Euler(28f, 0f, 0f);
            pierna.transform.localScale = new Vector3(0.10f, 0.28f, 0.10f);
            Destroy(pierna.GetComponent<Collider>());
            pierna.GetComponent<MeshRenderer>().material = matPantalon;
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
