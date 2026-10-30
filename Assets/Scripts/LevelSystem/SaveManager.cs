using UnityEngine;

public static class SaveManager
{
    private const string ClaveNivelDesbloqueado = "NivelDesbloqueado";
    private const string Prefijo = "Progreso_";

    public static int NivelDesbloqueado => PlayerPrefs.GetInt(ClaveNivelDesbloqueado, 1);

    public static int MejorPuntaje(string nivel)
    {
        return PlayerPrefs.GetInt(Prefijo + "MejorPuntaje_" + nivel, 0);
    }

    public static float MejorTiempo(string nivel)
    {
        return PlayerPrefs.GetFloat(Prefijo + "MejorTiempo_" + nivel, 0f);
    }

    public static int Intentos(string nivel)
    {
        return PlayerPrefs.GetInt(Prefijo + "Intentos_" + nivel, 0);
    }

    public static bool NivelCompletado(string nivel)
    {
        return PlayerPrefs.GetInt(Prefijo + "Completado_" + nivel, 0) == 1;
    }

    public static bool RegistrarNivelCompletado(string nivel, int puntaje, float tiempo, int siguienteNivel)
    {
        bool nuevoRecord = puntaje > MejorPuntaje(nivel);

        if (nuevoRecord)
            PlayerPrefs.SetInt(Prefijo + "MejorPuntaje_" + nivel, puntaje);

        float mejorTiempo = MejorTiempo(nivel);
        if (mejorTiempo <= 0f || (tiempo > 0f && tiempo < mejorTiempo))
            PlayerPrefs.SetFloat(Prefijo + "MejorTiempo_" + nivel, tiempo);

        PlayerPrefs.SetInt(Prefijo + "Completado_" + nivel, 1);

        if (siguienteNivel > NivelDesbloqueado)
            PlayerPrefs.SetInt(ClaveNivelDesbloqueado, siguienteNivel);

        PlayerPrefs.SetInt("PartidaExiste", 1);
        PlayerPrefs.Save();

        return nuevoRecord;
    }

    public static void RegistrarGameOver(string nivel)
    {
        PlayerPrefs.SetInt(Prefijo + "Intentos_" + nivel, Intentos(nivel) + 1);
        PlayerPrefs.Save();
    }

    public static void ReiniciarProgreso()
    {
        PlayerPrefs.DeleteKey(ClaveNivelDesbloqueado);
        PlayerPrefs.Save();
    }
}
