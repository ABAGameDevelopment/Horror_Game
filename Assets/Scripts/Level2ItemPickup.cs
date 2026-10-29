using UnityEngine;
using TMPro;

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
    [SerializeField] private float distanciaRecogida = 2.5f;

    [Header("Efecto Visual")]
    [SerializeField] private bool rotarEnSitio = true;
    [SerializeField] private float velocidadRotacion = 45f;

    private Transform playerTransform;
    private bool recogido = false;

    public TipoItemLevel2 Tipo => tipoItem;
    public string Nombre => nombreItem;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (recogido) return;

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
        if (distancia <= distanciaRecogida)
        {
            // Permitir recoger con la tecla E o al acercarse
            if (Input.GetKeyDown(KeyCode.E) || distancia < 1.3f)
            {
                Recoger();
            }
        }
    }

    private void Recoger()
    {
        if (recogido) return;
        recogido = true;

        CarEscapeController car = FindObjectOfType<CarEscapeController>();
        if (car != null)
        {
            car.RegistrarItemRecogido(tipoItem);
        }

        Debug.Log("<color=yellow>[Nivel 2]</color> ¡Item recogido!: " + nombreItem);
        gameObject.SetActive(false);
    }
}
