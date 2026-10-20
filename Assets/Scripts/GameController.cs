using System.Collections.Generic;
using UnityEngine;

public enum MatchResult { Playing, Eliminated, Timeout, Victory }

public static class GameController
{
    public const int MaxHealth = 3;
    public const float RoomPreparationDuration = 2f;
    public static int health { get; private set; }
    public static int ammo { get; private set; }
    public static int score { get; private set; }
    public static int wave { get; private set; }
    public static int enemiesRemaining { get; private set; }
    public static int roomsCleared { get; private set; }
    public static int EncounterId { get; private set; }
    public static float elapsedTime { get; private set; }
    public static float timeLimit { get; private set; }
    public static float waveTimeRemaining { get; private set; }
    public static float PreparationTimeRemaining { get; private set; }
    public static bool Preparing { get; private set; }
    public static bool timeLimitEnabled { get; private set; }
    public static bool CombatActive { get; private set; }
    public static bool Transitioning { get; set; }
    public static bool Paused { get; private set; }
    public static bool Initialized { get; private set; }
    public static DifficultyLevel Difficulty { get; private set; }
    public static MatchResult Result { get; private set; }
    public static bool timeExpired => Result == MatchResult.Timeout;
    public static bool victory => Result == MatchResult.Victory;
    public static bool gameOver => Initialized && Result != MatchResult.Playing;
    public static bool CanControl => Initialized && !gameOver && !Transitioning && !Paused && !Preparing;
    public static bool SuppressPlayerInput => Time.frameCount <= resumeFrame;
    public static bool Invulnerable => Preparing || Time.time < nextDamageTime;
    public static int KeyCount => keys.Count;
    private static float nextDamageTime;
    private static int resumeFrame = -1;
    private static readonly HashSet<string> keys = new HashSet<string>();

    public static void Init(float duration, bool useTimeLimit, DifficultyLevel? difficulty = null)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Paused = Preparing = false;
        PreparationTimeRemaining = 0f;
        resumeFrame = -1;
        Difficulty = difficulty ?? GameDifficulty.Selected;
        health = MaxHealth;
        ammo = GameDifficulty.StartingAmmo(Difficulty);
        score = wave = roomsCleared = enemiesRemaining = EncounterId = 0;
        elapsedTime = 0f;
        timeLimit = Mathf.Max(1f, duration);
        waveTimeRemaining = timeLimit;
        timeLimitEnabled = useTimeLimit;
        CombatActive = Transitioning = false;
        Result = MatchResult.Playing;
        nextDamageTime = Time.time + 0.6f;
        keys.Clear();
        Initialized = true;
    }

    public static bool SetPaused(bool paused)
    {
        if (paused && (!Initialized || gameOver || Transitioning)) return false;
        if (Paused == paused) return true;
        Paused = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
        if (!paused) resumeFrame = Time.frameCount;
        return true;
    }

    public static void EnterRoom()
    {
        EncounterId++;
        CombatActive = Preparing = false;
        PreparationTimeRemaining = 0f;
        enemiesRemaining = 0;
        GrantProtection(0.65f);
    }

    // O parâmetro opcional permite que testes isolados de combate pulem a apresentação.
    // As chamadas normais do WaveManager usam a preparação em todas as dificuldades.
    public static void StartWave(int amount, float duration = -1f, bool prepareRoom = true)
    {
        if (gameOver || amount <= 0) return;
        wave++;
        enemiesRemaining = amount;
        if (duration > 0f) timeLimit = duration;
        waveTimeRemaining = timeLimit;
        CombatActive = true;
        Preparing = prepareRoom;
        PreparationTimeRemaining = prepareRoom ? RoomPreparationDuration : 0f;
    }

    public static void UpdateTime(float deltaTime)
    {
        if (!Initialized || gameOver || Transitioning || Paused || ScreenTransition.IsBusy) return;
        float dt = Mathf.Max(0f, deltaTime);
        if (Preparing)
        {
            // A espera só avança com a sala visível. Não consome nenhum dos relógios.
            PreparationTimeRemaining = Mathf.Max(0f, PreparationTimeRemaining - dt);
            if (PreparationTimeRemaining <= 0f)
            {
                Preparing = false;
                GrantProtection(0.65f);
            }
            return;
        }
        if (!CanControl) return;
        elapsedTime += dt;
        if (!timeLimitEnabled || !CombatActive) return;
        waveTimeRemaining = Mathf.Max(0f, waveTimeRemaining - dt);
        if (waveTimeRemaining <= 0f) Finish(MatchResult.Timeout);
    }

    public static bool TrySpendAmmo()
    {
        if (!CanControl || SuppressPlayerInput || !CombatActive || ammo <= 0) return false;
        ammo--;
        return true;
    }

    public static void AddAmmo(int amount)
    {
        if (!gameOver) ammo += Mathf.Max(0, amount);
    }
    public static bool TryHeal()
    {
        if (gameOver || health >= MaxHealth) return false;
        health++;
        return true;
    }
    public static void GrantProtection(float duration)
    {
        nextDamageTime = Mathf.Max(nextDamageTime, Time.time + duration);
    }
    public static void TakeDamage()
    {
        if (!CanControl || !CombatActive || Invulnerable) return;
        health = Mathf.Max(0, health - 1);
        GrantProtection(GameDifficulty.DamageProtection(Difficulty));
        if (health == 0) Finish(MatchResult.Eliminated);
    }
    public static void RegisterKill(int points = 10)
    {
        if (gameOver || !CombatActive || enemiesRemaining <= 0) return;
        enemiesRemaining--;
        AddScore(points);
        if (enemiesRemaining == 0) CombatActive = false;
    }
    public static void RegisterRoomCleared()
    {
        if (!gameOver) roomsCleared++;
    }
    public static void ReconcileEnemies(int liveCount)
    {
        if (gameOver || Transitioning || Paused) return;
        enemiesRemaining = Mathf.Max(0, liveCount);
        CombatActive = enemiesRemaining > 0;
        if (!CombatActive)
        {
            Preparing = false;
            PreparationTimeRemaining = 0f;
        }
    }
    public static void AddScore(int amount)
    {
        if (!gameOver) score += Mathf.Max(0, amount);
    }
    public static bool HasKey(string key) => string.IsNullOrEmpty(key) || keys.Contains(key);
    public static bool AddKey(string key)
    {
        if (gameOver || string.IsNullOrEmpty(key)) return false;
        return keys.Add(key);
    }
    public static void Win() => Finish(MatchResult.Victory);
    private static void Finish(MatchResult result)
    {
        if (gameOver) return;
        Result = result;
        CombatActive = Preparing = false;
        PreparationTimeRemaining = 0f;
    }
}
