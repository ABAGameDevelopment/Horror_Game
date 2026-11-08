using UnityEngine;
using UnityEngine.InputSystem;

public enum TipoItemLevel2
{
    Bateria,
    Aceite,
    Gasolina,
    Llaves
}

public class Level2ItemPickup : MonoBehaviour
{
    [Header("Configuracion del Item")]
    [SerializeField] private TipoItemLevel2 tipoItem;
    [SerializeField] private string nombreItem = "Item";
    [SerializeField] private float distanciaRecogida = 2.8f;

    [Header("Efecto Visual")]
    [SerializeField] private bool rotarEnSitio = true;
    [SerializeField] private float velocidadRotacion = 45f;

    private Transform playerTransform;
    private bool recogido = false;
    private bool jugadorCerca = false;

    public TipoItemLevel2 Tipo => tipoItem;
    public string Nombre => nombreItem;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null) playerTransform = player.transform;
    }

    private bool TeclaInteraccionPresionada()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame ||
                Keyboard.current.fKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame)
            {
                return true;
            }
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    private void Update()
    {
        if (recogido) return;

        if (LevelStateManager.Instance != null && !LevelStateManager.Instance.JuegoActivo) return;

        if (rotarEnSitio)
        {
            transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.World);
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player != null) playerTransform = player.transform;
            return;
        }

        float distancia = Vector3.Distance(transform.position, playerTransform.position);
        jugadorCerca = (distancia <= distanciaRecogida);

        if (jugadorCerca)
        {
            if (CarEscapeController.Instance != null)
            {
                CarEscapeController.Instance.SetPromptExterno("[E] Recoger " + nombreItem);
            }

            // Permitir recoger con la tecla E/F o al pasar muy cerca
            if (TeclaInteraccionPresionada() || distancia < 1.4f)
            {
                Recoger();
            }
        }
    }

    private void Recoger()
    {
        if (recogido) return;
        recogido = true;

        if (CarEscapeController.Instance != null)
        {
            CarEscapeController.Instance.RegistrarItemRecogido(tipoItem);
        }

        Debug.Log("<color=yellow>[Nivel 2]</color> ¡Item recogido!: " + nombreItem);
        gameObject.SetActive(false);
    }
}
