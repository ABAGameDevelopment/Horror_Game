
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

        Time.timeScale = 0;

        // Colocar la imagen correspondiente.
        imagenNota.sprite = imagen;

        // Ajustar la imagen sin deformarla.
        imagenNota.preserveAspect = true;

        // Mostrar el boton correspondiente al control.
        ActualizarTextoContinuar();

        // Mostrar la interfaz.
        panelNota.SetActive(true);
        MostrandoNota = true;
    }

    private void ActualizarTextoContinuar()
    {
        // Elegir el texto segun el control utilizado.
        bool usaGamepad =
            Gamepad.current != null &&
            (Keyboard.current == null ||
             Gamepad.current.lastUpdateTime >
             Keyboard.current.lastUpdateTime);

        string boton = usaGamepad ? "X" : "E";

        textoContinuar.text =
            $"Presiona {boton} para continuar";
    }

    public void CerrarNota()
    {
        panelNota.SetActive(false);
        MostrandoNota = false;
        Time.timeScale = 1;
    }
}
