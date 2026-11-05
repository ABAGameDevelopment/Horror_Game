
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AjustesGraficos : MonoBehaviour
{
    [Header("Desplegables")]
    [SerializeField] private TMP_Dropdown dropdownResolucion;
    [SerializeField] private TMP_Dropdown dropdownCalidad;

    [Header("Interruptores")]
    [SerializeField] private Toggle togglePantallaCompleta;
    [SerializeField] private Toggle toggleVSync;

    private List<Resolution> resolucionesDisponibles = new List<Resolution>();

    private const string RESOLUCION_ANCHO = "Graficos_Ancho";
    private const string RESOLUCION_ALTO = "Graficos_Alto";
    private const string CALIDAD = "Graficos_Calidad";
    private const string PANTALLA_COMPLETA = "Graficos_PantallaCompleta";
    private const string VSYNC = "Graficos_VSync";

    private void Start()
    {
        CargarResoluciones();
        CargarCalidades();
        CargarPreferencias();
        ConfigurarEventos();
    }

    private void CargarResoluciones()
    {
        resolucionesDisponibles.Clear();
        dropdownResolucion.ClearOptions();

        List<string> opciones = new List<string>();

        foreach (Resolution resolucion in Screen.resolutions)
        {
            bool existe = resolucionesDisponibles.Exists(r =>
                r.width == resolucion.width &&
                r.height == resolucion.height
            );

            if (!existe)
            {
                resolucionesDisponibles.Add(resolucion);

                opciones.Add(
                    resolucion.width + " x " + resolucion.height
                );
            }
        }

        dropdownResolucion.AddOptions(opciones);
    }

    private void AplicarResolucion(int indice)
    {
        if (indice < 0 || indice >= resolucionesDisponibles.Count)
            return;

        Resolution resolucion = resolucionesDisponibles[indice];

        Screen.SetResolution(
            resolucion.width,
            resolucion.height,
            Screen.fullScreenMode
        );

        PlayerPrefs.SetInt(RESOLUCION_ANCHO, resolucion.width);
        PlayerPrefs.SetInt(RESOLUCION_ALTO, resolucion.height);
        PlayerPrefs.Save();
    }

    private void CargarCalidades()
    {
        dropdownCalidad.ClearOptions();

        dropdownCalidad.AddOptions(
            new List<string>(QualitySettings.names)
        );
    }

    private void AplicarCalidad(int indice)
    {
        if (indice < 0 || indice >= QualitySettings.names.Length)
            return;

        QualitySettings.SetQualityLevel(indice, true);

        PlayerPrefs.SetInt(CALIDAD, indice);
        PlayerPrefs.Save();
    }

    private void AplicarPantallaCompleta(bool activada)
    {
        Screen.fullScreenMode = activada
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        PlayerPrefs.SetInt(PANTALLA_COMPLETA, activada ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void AplicarVSync(bool activado)
    {
        QualitySettings.vSyncCount = activado ? 1 : 0;

        PlayerPrefs.SetInt(VSYNC, activado ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void CargarPreferencias()
    {
        int ancho = PlayerPrefs.GetInt(
            RESOLUCION_ANCHO,
            Screen.currentResolution.width
        );

        int alto = PlayerPrefs.GetInt(
            RESOLUCION_ALTO,
            Screen.currentResolution.height
        );

        int indiceResolucion = resolucionesDisponibles.FindIndex(r =>
            r.width == ancho && r.height == alto
        );

        if (indiceResolucion < 0)
        {
            indiceResolucion = resolucionesDisponibles.FindIndex(r =>
                r.width == Screen.width && r.height == Screen.height
            );
        }

        if (indiceResolucion < 0 && resolucionesDisponibles.Count > 0)
            indiceResolucion = 0;

        if (indiceResolucion >= 0)
        {
            dropdownResolucion.SetValueWithoutNotify(indiceResolucion);
            AplicarResolucion(indiceResolucion);
        }

        int calidad = PlayerPrefs.GetInt(
            CALIDAD,
            QualitySettings.GetQualityLevel()
        );

        calidad = Mathf.Clamp(
            calidad,
            0,
            QualitySettings.names.Length - 1
        );

        dropdownCalidad.SetValueWithoutNotify(calidad);
        AplicarCalidad(calidad);

        bool pantallaCompleta = PlayerPrefs.GetInt(
            PANTALLA_COMPLETA,
            Screen.fullScreen ? 1 : 0
        ) == 1;

        togglePantallaCompleta.SetIsOnWithoutNotify(pantallaCompleta);
        AplicarPantallaCompleta(pantallaCompleta);

        bool vsync = PlayerPrefs.GetInt(
            VSYNC,
            QualitySettings.vSyncCount > 0 ? 1 : 0
        ) == 1;

        toggleVSync.SetIsOnWithoutNotify(vsync);
        AplicarVSync(vsync);
    }

    private void ConfigurarEventos()
    {
        dropdownResolucion.onValueChanged.AddListener(AplicarResolucion);
        dropdownCalidad.onValueChanged.AddListener(AplicarCalidad);

        togglePantallaCompleta.onValueChanged.AddListener(
            AplicarPantallaCompleta
        );

        toggleVSync.onValueChanged.AddListener(AplicarVSync);
    }

    private void OnDestroy()
    {
        if (dropdownResolucion != null)
            dropdownResolucion.onValueChanged.RemoveListener(AplicarResolucion);

        if (dropdownCalidad != null)
            dropdownCalidad.onValueChanged.RemoveListener(AplicarCalidad);

        if (togglePantallaCompleta != null)
            togglePantallaCompleta.onValueChanged.RemoveListener(AplicarPantallaCompleta);

        if (toggleVSync != null)
            toggleVSync.onValueChanged.RemoveListener(AplicarVSync);
    }
}
