using UnityEngine;

public enum DifficultyLevel { Easy, Normal, Hard }

// Uma seleção para a próxima partida; GameController guarda uma cópia ao iniciar.
public static class GameDifficulty
{
    private const string Preference = "KOTK.Difficulty";
    public static DifficultyLevel Selected => (DifficultyLevel)Mathf.Clamp(PlayerPrefs.GetInt(Preference, 1), 0, 2);
    public static void Select(DifficultyLevel level)
    {
        PlayerPrefs.SetInt(Preference, Mathf.Clamp((int)level, 0, 2));
        PlayerPrefs.Save();
    }
    public static string Name(DifficultyLevel level) => level == DifficultyLevel.Easy ? "FÁCIL"
        : level == DifficultyLevel.Hard ? "DIFÍCIL" : "NORMAL";
    public static float Movement(DifficultyLevel level) => level == DifficultyLevel.Easy ? 0.9f : level == DifficultyLevel.Hard ? 1.15f : 1f;
    public static float ProjectileSpeed(DifficultyLevel level) => level == DifficultyLevel.Easy ? 0.85f : level == DifficultyLevel.Hard ? 1.1f : 1f;
    public static float ShootingPause(DifficultyLevel level) => level == DifficultyLevel.Easy ? 1.2f : level == DifficultyLevel.Hard ? 0.85f : 1f;
    public static float TimeMultiplier(DifficultyLevel level) => level == DifficultyLevel.Easy ? 1.3f : level == DifficultyLevel.Hard ? 0.9f : 1f;
    public static int StartingAmmo(DifficultyLevel level) => level == DifficultyLevel.Easy ? 28 : level == DifficultyLevel.Hard ? 20 : 24;
    public static float DamageProtection(DifficultyLevel level) => level == DifficultyLevel.Easy ? 1.2f : 1f;
    public static float RoomTime(ArenaDefinition room, DifficultyLevel level) => Mathf.Ceil(room.timeLimit * TimeMultiplier(level));
    public static bool Includes(EnemySpawn spawn, DifficultyLevel level) => spawn.prefab != null && (int)spawn.minimumDifficulty <= (int)level;
    public static int EnemyCount(ArenaDefinition room, DifficultyLevel level)
    {
        int count = 0;
        foreach (EnemySpawn spawn in room.enemies) if (Includes(spawn, level)) count++;
        return count;
    }
    public static string Description(DifficultyLevel level)
    {
        if (level == DifficultyLevel.Easy)
            return "6–9 inimigos por sala\n30% mais tempo\nInimigos 10% mais lentos\nProjéteis 15% mais lentos\nPausas entre rajadas 20% maiores\n28 tiros iniciais · proteção de 1,2 s";
        if (level == DifficultyLevel.Hard)
            return "10–15 inimigos por sala\n10% menos tempo, arredondado para cima\nInimigos 15% mais rápidos\nProjéteis 10% mais rápidos\nPausas entre rajadas 15% menores\n20 tiros iniciais · proteção de 1 s";
        return "8–12 inimigos por sala\nPrazos próprios para cada arena\nVelocidade e tiros padrão\n24 tiros iniciais · proteção de 1 s\n\nMais inimigos que a versão anterior";
    }
}
