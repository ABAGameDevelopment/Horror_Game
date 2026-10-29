using UnityEngine;

/// <summary>
/// Asegura que el GameManager exista en la escena activa.
/// Ponlo en cualquier escena como componente de un GameObject vacío.
/// Si ya existe un GameManager (de escenas anteriores) no hace nada.
/// </summary>
public class GameManagerBootstrap : MonoBehaviour
{
    [SerializeField] private GameManager prefabGameManager;

    private void Awake()
    {
        if (GameManager.Instance == null && prefabGameManager != null)
            Instantiate(prefabGameManager);
    }
}
