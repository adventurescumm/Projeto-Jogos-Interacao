using UnityEngine;

public static class GameController
{
    private static int collectableCount;
    
    public static int score { get; private set; }
    
    private const int pointsPerCollectable = 10;

    public static float elapsedTime { get; private set; }
    
    public static float timeLimit { get; private set; }

    public static bool timeExpired
    {
        get { return elapsedTime >= timeLimit; }
    }

    public static bool gameOver
    {
        get { return collectableCount <= 0 || timeExpired; }
    }

    public static void Init(float duration)
    {
        collectableCount = GameObject.FindGameObjectsWithTag("Coletavel").Length;
        elapsedTime = 0f;
        score = 0;
        timeLimit = Mathf.Max(1f, duration);
    }

    public static void UpdateTime(float deltaTime)
    {
        if (gameOver) return;
        elapsedTime = Mathf.Min(elapsedTime + deltaTime, timeLimit);
    }

    public static void Collect()
    {
        if (gameOver) return;
        collectableCount--;
        score += pointsPerCollectable;
    }
}
