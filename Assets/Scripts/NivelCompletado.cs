
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NivelCompletado : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image panelOscuro;
    [SerializeField] private GameObject panelNivelCompletado;

    [Header("Transicion")]
    [SerializeField] private float retrasoAntesDeOscurecer = 0.5f;
    [SerializeField] private float duracionDesvanecimiento = 2f;

    [Header("Escenas")]
    [SerializeField] private string escenaMenuPrincipal = "MenuPrincipal";
    [SerializeField] private string siguienteNivel = "Nivel2";

    private bool finalIniciado;

    private void Start()
    {
        if (panelOscuro != null)
        {
            Color color = panelOscuro.color;
            color.a = 0f;
            panelOscuro.color = color;
            panelOscuro.raycastTarget = false;
        }

        if (panelNivelCompletado != null)
            panelNivelCompletado.SetActive(false);
    }

    public void IniciarFinal()
    {
        if (finalIniciado)
            return;

        finalIniciado = true;
        StartCoroutine(TransicionFinal());
    }

    private IEnumerator TransicionFinal()
    {
        yield return new WaitForSeconds(retrasoAntesDeOscurecer);

        if (panelOscuro == null)
        {
            Debug.LogError("No se ha asignado PanelOscuro en NivelCompletado.");
            yield break;
        }

        panelOscuro.gameObject.SetActive(true);
        panelOscuro.raycastTarget = true;

        Color color = panelOscuro.color;
        color.a = 0f;
        panelOscuro.color = color;

        float tiempo = 0f;

        while (tiempo < duracionDesvanecimiento)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(tiempo / duracionDesvanecimiento);

            color.a = Mathf.SmoothStep(0f, 1f, progreso);
            panelOscuro.color = color;

            yield return null;
        }

        color.a = 1f;
        panelOscuro.color = color;

        if (panelNivelCompletado != null)
            panelNivelCompletado.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenuPrincipal);
    }

    public void Continuar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(siguienteNivel);
    }
}
