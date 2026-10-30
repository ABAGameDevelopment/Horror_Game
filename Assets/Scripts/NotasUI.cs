
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class NotasUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject panelNota;
    [SerializeField] private Image imagenNota;
    [SerializeField] private TMP_Text textoContinuar;

    public bool MostrandoNota { get; private set; }

    private void Start()
    {
        panelNota.SetActive(false);
        MostrandoNota = false;
    }

    public void MostrarNota(Sprite imagen)
    {
        if (imagen == null)
        {
            Debug.LogError("No se asigno una imagen a la nota.");
            return;
        }

        // Colocar la imagen correspondiente.
        imagenNota.sprite = imagen;

        // Ajustar la imagen sin deformarla.
        imagenNota.preserveAspect = true;

        // Mostrar el boton correspondiente al control.
        ActualizarTextoContinuar();

        // Mostrar la interfaz.
        panelNota.SetActive(true);
        MostrandoNota = true;

        // Desbloquear el cursor si es necesario no se requiere
        // para una interfaz que se controla con E/X.
    }

    private void ActualizarTextoContinuar()
    {
        bool esXbox = playerInput != null &&
            !string.IsNullOrEmpty(playerInput.currentControlScheme) &&
            (playerInput.currentControlScheme.ToLower().Contains("gamepad") ||
             playerInput.currentControlScheme.ToLower().Contains("xbox"));

        string boton = esXbox ? "X" : "E";

        textoContinuar.text =
            $"Presiona {boton} para continuar";
    }

    public void CerrarNota()
    {
        panelNota.SetActive(false);
        MostrandoNota = false;
    }
}
