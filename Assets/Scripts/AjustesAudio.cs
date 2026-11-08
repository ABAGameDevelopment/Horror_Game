
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AjustesAudio : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderEfectos;

    [Header("Parametros del Audio Mixer")]
    [SerializeField] private string parametroMusica = "MusicaVolume";
    [SerializeField] private string parametroEfectos = "EfectosVolume";

    private const string CLAVE_MUSICA = "VolumenMusica";
    private const string CLAVE_EFECTOS = "VolumenEfectos";

    private void Start()
    {
        // Recuperar los valores guardados.
        float volumenMusica = PlayerPrefs.GetFloat(CLAVE_MUSICA, 75f);
        float volumenEfectos = PlayerPrefs.GetFloat(CLAVE_EFECTOS, 75f);

        // Configurar los limites de los sliders.
        sliderMusica.minValue = 1f;
        sliderMusica.maxValue = 100f;

        sliderEfectos.minValue = 1f;
        sliderEfectos.maxValue = 100f;

        // Mostrar los valores guardados.
        sliderMusica.value = volumenMusica;
        sliderEfectos.value = volumenEfectos;

        // Aplicar los valores al Audio Mixer.
        ActualizarMusica(volumenMusica);
        ActualizarEfectos(volumenEfectos);

        // Escuchar los cambios de los sliders.
        sliderMusica.onValueChanged.AddListener(ActualizarMusica);
        sliderEfectos.onValueChanged.AddListener(ActualizarEfectos);
    }

    public void ActualizarMusica(float valor)
    {
        float volumen = Mathf.Log10(valor / 100f) * 20f;

        audioMixer.SetFloat(parametroMusica, volumen);

        PlayerPrefs.SetFloat(CLAVE_MUSICA, valor);
        PlayerPrefs.Save();
    }

    public void ActualizarEfectos(float valor)
    {
        float volumen = Mathf.Log10(valor / 100f) * 20f;

        audioMixer.SetFloat(parametroEfectos, volumen);

        PlayerPrefs.SetFloat(CLAVE_EFECTOS, valor);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (sliderMusica != null)
            sliderMusica.onValueChanged.RemoveListener(ActualizarMusica);

        if (sliderEfectos != null)
            sliderEfectos.onValueChanged.RemoveListener(ActualizarEfectos);
    }
}
