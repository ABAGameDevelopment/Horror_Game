using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum EstadoNivel
{
    Jugando,
    Pausado,
    Completado,
    GameOver
}

public class LevelStateManager : MonoBehaviour
{
    public static LevelStateManager Instance { get; private set; }

    [SerializeField] private string nombreNivel = "Nivel2";
    [SerializeField] private string siguienteEscena = "Level3";
    [SerializeField] private int indiceSiguienteNivel = 3;

    private float tiempoJugado;
    private int puntaje;
    private EstadoNivel estado = EstadoNivel.Jugando;
    private PlayerHealth saludJugador;

    public event Action<EstadoNivel> EstadoCambiado;
    public event Action<int> PuntajeCambiado;

    public EstadoNivel Estado => estado;
    public bool JuegoActivo => estado == EstadoNivel.Jugando;
    public bool EstaPausado => estado == EstadoNivel.Pausado;
    public float TiempoJugado => tiempoJugado;
    public int Puntaje => puntaje;
    public string NombreNivel => nombreNivel;
    public string SiguienteEscena => siguienteEscena;
    public int IndiceSiguienteNivel => indiceSiguienteNivel;
    public bool NuevoRecord { get; private set; }
    public string MotivoGameOver { get; private set; } = string.Empty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        saludJugador = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
        if (saludJugador != null)
            saludJugador.Murio += AlMorirJugador;

        EstablecerControlJugador(true);
        BloquearCursor();
        EstadoCambiado?.Invoke(estado);
    }

    private void OnDestroy()
    {
        if (saludJugador != null)
            saludJugador.Murio -= AlMorirJugador;

        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (estado == EstadoNivel.Jugando)
            tiempoJugado += Time.deltaTime;

        bool pausa = false;
        if (Keyboard.current != null)
            pausa |= Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame;
        if (Gamepad.current != null)
            pausa |= Gamepad.current.startButton.wasPressedThisFrame;

        if (pausa)
            AlternarPausa();
    }

    public void AlternarPausa()
    {
        if (estado == EstadoNivel.Jugando)
            Pausar();
        else if (estado == EstadoNivel.Pausado)
            Reanudar();
    }

    public void Pausar()
    {
        if (estado != EstadoNivel.Jugando)
            return;

        estado = EstadoNivel.Pausado;
        Time.timeScale = 0f;
        EstablecerControlJugador(false);
        DesbloquearCursor();
        EstadoCambiado?.Invoke(estado);
    }

    public void Reanudar()
    {
        if (estado != EstadoNivel.Pausado)
            return;

        estado = EstadoNivel.Jugando;
        Time.timeScale = 1f;
        EstablecerControlJugador(true);
        BloquearCursor();
        EstadoCambiado?.Invoke(estado);
    }

    public void AgregarPuntaje(int cantidad)
    {
        if (cantidad == 0)
            return;

        puntaje = Mathf.Max(0, puntaje + cantidad);
        PuntajeCambiado?.Invoke(puntaje);
    }

    public void CompletarNivel()
    {
        if (estado == EstadoNivel.Completado || estado == EstadoNivel.GameOver)
            return;

        estado = EstadoNivel.Completado;
        Time.timeScale = 0f;
        EstablecerControlJugador(false);
        DesbloquearCursor();
        NuevoRecord = SaveManager.RegistrarNivelCompletado(nombreNivel, puntaje, tiempoJugado, indiceSiguienteNivel);
        EstadoCambiado?.Invoke(estado);
    }

    public void GameOver(string motivo)
    {
        if (estado == EstadoNivel.Completado || estado == EstadoNivel.GameOver)
            return;

        estado = EstadoNivel.GameOver;
        MotivoGameOver = motivo;
        Time.timeScale = 0f;
        EstablecerControlJugador(false);
        DesbloquearCursor();
        SaveManager.RegistrarGameOver(nombreNivel);
        EstadoCambiado?.Invoke(estado);
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.IrAlMenu();
            return;
        }

        SceneManager.LoadScene("MainMenu");
    }

    private void AlMorirJugador()
    {
        GameOver("Has caído en la oscuridad.");
    }

    private void EstablecerControlJugador(bool activo)
    {
        PlayerController[] jugadores = UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude);
        foreach (PlayerController jugador in jugadores)
            jugador.enabled = activo;
    }

    private static void BloquearCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void DesbloquearCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
