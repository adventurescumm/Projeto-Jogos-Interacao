using UnityEngine;

// Preferências locais; a mira do mouse continua direta.
public static class ControllerSettings
{
    public static float Deadzone => Mathf.Clamp(PlayerPrefs.GetFloat("KOTK.StickDeadzone", 0.16f), 0.08f, 0.30f);
    public static int AimStyle => Mathf.Clamp(PlayerPrefs.GetInt("KOTK.AimStyle", 1), 0, 2);
    public static bool AimAssist => PlayerPrefs.GetInt("KOTK.AimAssist", 1) != 0;
    public static string StyleName => AimStyle == 0 ? "DIRETA" : AimStyle == 1 ? "LEVE" : "SUAVE";
    public static void CycleDeadzone()
    {
        float next = Deadzone + 0.04f;
        PlayerPrefs.SetFloat("KOTK.StickDeadzone", next > 0.245f ? 0.12f : next);
        PlayerPrefs.Save();
    }
    public static void CycleStyle() { PlayerPrefs.SetInt("KOTK.AimStyle", (AimStyle + 1) % 3); PlayerPrefs.Save(); }
    public static void ToggleAssist() { PlayerPrefs.SetInt("KOTK.AimAssist", AimAssist ? 0 : 1); PlayerPrefs.Save(); }
    public static Vector2 Radial(Vector2 raw, float deadzone)
    {
        float magnitude = Mathf.Min(raw.magnitude, 1f);
        return magnitude <= deadzone ? Vector2.zero : raw.normalized * Mathf.InverseLerp(deadzone, 1f, magnitude);
    }
    public static float Response(float strength)
    {
        return AimStyle == 2 ? Mathf.Lerp(18f, 45f, strength) : Mathf.Lerp(35f, 70f, strength);
    }
}
