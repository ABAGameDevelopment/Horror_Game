using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float saludMaxima = 100f;
    [SerializeField] private float saludInicial = 100f;

    private float salud;

    public event Action<float, float> SaludCambiada;
    public event Action Murio;

    public float Salud => salud;
    public float SaludMaxima => saludMaxima;
    public float SaludNormalizada => saludMaxima <= 0f ? 0f : Mathf.Clamp01(salud / saludMaxima);
    public bool EstaMuerto => salud <= 0f;

    private void Awake()
    {
        saludMaxima = Mathf.Max(1f, saludMaxima);
        salud = Mathf.Clamp(saludInicial <= 0f ? saludMaxima : saludInicial, 0f, saludMaxima);
    }

    private void Start()
    {
        SaludCambiada?.Invoke(salud, saludMaxima);
    }

    public void RecibirDanio(float cantidad)
    {
        if (cantidad <= 0f || EstaMuerto)
            return;

        salud = Mathf.Max(0f, salud - cantidad);
        SaludCambiada?.Invoke(salud, saludMaxima);

        if (salud <= 0f)
            Murio?.Invoke();
    }

    public void Curar(float cantidad)
    {
        if (cantidad <= 0f || EstaMuerto)
            return;

        salud = Mathf.Min(saludMaxima, salud + cantidad);
        SaludCambiada?.Invoke(salud, saludMaxima);
    }

    public void Restablecer()
    {
        salud = saludMaxima;
        SaludCambiada?.Invoke(salud, saludMaxima);
    }
}
