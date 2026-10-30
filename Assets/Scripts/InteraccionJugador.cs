
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InteraccionJugador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private TMP_Text textoInteraccion;
    [SerializeField] private NotasUI notasUI;

    private PuertaInteractuable puertaCercana;
    private PuertaConLlave puertaBloqueadaCercana;
    private LlaveInteractuable llaveCercana;
    private NotaInteractuable notaCercana;

    private bool tieneLlaveAuditorio;
    private bool tieneLlaveSalida;
    private Coroutine mensajeCoroutine;

    public bool TieneLlave => tieneLlaveAuditorio || tieneLlaveSalida;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (textoInteraccion != null)
            textoInteraccion.gameObject.SetActive(false);
    }

    public void OnInteract(InputValue value)
    {

        if (!value.isPressed)
            return;



        // CERRAR NOTA
        if (notasUI != null && notasUI.MostrandoNota)
        {

            notasUI.CerrarNota();
            return;
        }

        // RECOGER LLAVE
        if (llaveCercana != null)
        {
            llaveCercana.Recoger(this);
            return;
        }

        // LEER NOTA
        if (notaCercana != null)
        {
            notaCercana.Interactuar();

            notaCercana = null;

            if (textoInteraccion != null)
                textoInteraccion.gameObject.SetActive(false);

            return;
        }

        // ABRIR PUERTA CON LLAVE
        if (puertaBloqueadaCercana != null)
        {
            puertaBloqueadaCercana.Interactuar(this);
            return;
        }

        // ABRIR PUERTA NORMAL
        if (puertaCercana != null)
            puertaCercana.Interactuar();
    }

    public void RecogerLlave(LlaveInteractuable.TipoLlave tipo)
    {
        if (tipo == LlaveInteractuable.TipoLlave.Auditorio)
        {
            tieneLlaveAuditorio = true;
            Debug.Log("El jugador tiene la llave del auditorio.");
        }
        else if (tipo == LlaveInteractuable.TipoLlave.Salida)
        {
            tieneLlaveSalida = true;
            Debug.Log("El jugador tiene la llave de salida.");
        }
    }

    public bool TieneLlaveDe(LlaveInteractuable.TipoLlave tipo)
    {
        if (tipo == LlaveInteractuable.TipoLlave.Auditorio)
            return tieneLlaveAuditorio;

        if (tipo == LlaveInteractuable.TipoLlave.Salida)
            return tieneLlaveSalida;

        return false;
    }

    public void MostrarLlave(LlaveInteractuable llave)
    {
        llaveCercana = llave;
        ActualizarTexto("recoger la llave");
    }

    public void OcultarLlave(LlaveInteractuable llave)
    {
        if (llaveCercana != llave)
            return;

        llaveCercana = null;
        ActualizarTextoActual();
    }

    public void MostrarPuerta(PuertaInteractuable puerta)
    {
        puertaCercana = puerta;
        ActualizarTexto("abrir");
    }

    public void OcultarPuerta(PuertaInteractuable puerta)
    {
        if (puertaCercana != puerta)
            return;

        puertaCercana = null;
        ActualizarTextoActual();
    }

    public void MostrarPuertaConLlave(PuertaConLlave puerta)
    {
        puertaBloqueadaCercana = puerta;
        ActualizarTexto("abrir la puerta");
    }

    public void OcultarPuertaConLlave(PuertaConLlave puerta)
    {
        if (puertaBloqueadaCercana != puerta)
            return;

        puertaBloqueadaCercana = null;
        ActualizarTextoActual();
    }

    public void MostrarMensajeTemporal(string mensaje)
    {
        if (textoInteraccion == null)
            return;

        if (mensajeCoroutine != null)
            StopCoroutine(mensajeCoroutine);

        mensajeCoroutine = StartCoroutine(
            MostrarMensajeCoroutine(mensaje)
        );
    }

    private IEnumerator MostrarMensajeCoroutine(string mensaje)
    {
        textoInteraccion.text = mensaje;
        textoInteraccion.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        mensajeCoroutine = null;
        ActualizarTextoActual();
    }

    private void ActualizarTextoActual()
    {
        if (textoInteraccion == null)
            return;

        if (llaveCercana != null)
            ActualizarTexto("recoger la llave");
        else if (notaCercana != null)
            ActualizarTexto("leer la nota");
        else if (puertaBloqueadaCercana != null)
            ActualizarTexto("abrir la puerta");
        else if (puertaCercana != null)
            ActualizarTexto("abrir");
        else
            textoInteraccion.gameObject.SetActive(false);
    }

    private void ActualizarTexto(string accion)
    {
        if (textoInteraccion == null)
            return;

        bool esXbox = playerInput != null &&
            !string.IsNullOrEmpty(playerInput.currentControlScheme) &&
            (playerInput.currentControlScheme.ToLower().Contains("gamepad") ||
             playerInput.currentControlScheme.ToLower().Contains("xbox"));

        string boton = esXbox ? "X" : "E";

        string mensaje = accion == "abrir"
            ? $"Presiona {boton} para abrir"
            : $"Presiona {boton} para {accion}";

        if (mensajeCoroutine != null)
        {
            StopCoroutine(mensajeCoroutine);
            mensajeCoroutine = null;
        }

        textoInteraccion.text = mensaje;
        textoInteraccion.gameObject.SetActive(true);
    }

    public void MostrarNota(NotaInteractuable nota)
    {
        notaCercana = nota;
        ActualizarTexto("leer la nota");
    }

    public void OcultarNota(NotaInteractuable nota)
    {
        if (notaCercana != nota)
            return;

        notaCercana = null;
        ActualizarTextoActual();
    }
}
