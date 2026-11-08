using UnityEngine;

/// <summary>
/// Coloca y anima la cámara del fondo del menú principal.
/// La cámara rota suavemente por el pasillo de la universidad
/// para dar sensación de ambiente inmersivo al menú.
/// </summary>
public class MenuBackgroundCamera : MonoBehaviour
{
    [Header("Rotación lenta de cámara")]
    [SerializeField] private float velocidadRotacion  = 3.5f;   // grados por segundo
    [SerializeField] private float amplitudOscilacion = 12f;    // rango izq-der
    [SerializeField] private float velocidadCabeceo   = 0.4f;   // lento

    [Header("Posición en la escena (se sobrescribe en Inspector)")]
    [SerializeField] private Vector3 posicionInicial = new Vector3(345.18f, 1.6f, 272.41f);
    [SerializeField] private Vector3 rotacionInicial = new Vector3(0f, -15.28f, 0f);

    [Header("Profundidad de campo simulada")]
    [SerializeField] private float velocidadBrillo    = 0.6f;
    [SerializeField] private float amplitudBrillo     = 0.12f;

    private Camera cam;
    private float tiempoBase;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        cam = GetComponent<Camera>();
        transform.position = posicionInicial;
        transform.eulerAngles = rotacionInicial;
        tiempoBase = Time.time;
    }

    // -----------------------------------------------------------------------
    private void Update()
    {
        float t = (Time.time - tiempoBase);

        // Oscilación horizontal lenta
        float yaw   = rotacionInicial.y + Mathf.Sin(t * velocidadRotacion * Mathf.Deg2Rad * 10f) * amplitudOscilacion;
        float pitch = rotacionInicial.x + Mathf.Sin(t * velocidadCabeceo) * 2.5f;

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        // Variación suave del brillo (simula parpadeo leve)
        if (cam != null)
        {
            // Solo afecta si usamos la propiedad de URP
            float brillo = 1f + Mathf.Sin(t * velocidadBrillo) * amplitudBrillo;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f + (brillo - 1f) * 2f, Time.deltaTime * 0.5f);
        }
    }
}
