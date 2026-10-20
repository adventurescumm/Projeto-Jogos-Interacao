using System;
using UnityEngine;

[CreateAssetMenu(menuName = "King of the Kill/Arena", fileName = "Arena")]
public class ArenaDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string announcement;
    public Vector2Int mapPosition;
    [Tooltip("Sigla curta exibida no minimapa, por exemplo OF, A ou $.")]
    public string mapLabel;
    public Color floorColor = new Color(0.25f, 0.29f, 0.34f);
    public Color accentColor = new Color(1f, 0.75f, 0.2f);
    [Min(5f)] public float timeLimit = 35f;
    public string north, east, south, west;
    public string[] requiredKeys = new string[0];
    public string rewardKey;
    public bool bossArena;
    public EnemySpawn[] enemies = new EnemySpawn[0];
    public ItemSpawn[] items = new ItemSpawn[0];
    public ArenaObstacle[] obstacles = new ArenaObstacle[0];

    public string Exit(int side)
    {
        switch (side) { case 0: return north; case 1: return east; case 2: return south; default: return west; }
    }
}

[Serializable]
public struct EnemySpawn
{
    public EnemyChaser prefab;
    public Vector2 position;
    [Tooltip("Fácil: aparece em todos os modos; Normal/Difícil: reforços desses modos.")]
    public DifficultyLevel minimumDifficulty;
}

[Serializable]
public struct ItemSpawn { public Pickup prefab; public Vector2 position; }

[Serializable]
public struct ArenaObstacle { public Vector2 position; public Vector2 size; }
