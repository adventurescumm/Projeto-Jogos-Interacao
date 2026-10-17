using System.Collections.Generic;
using UnityEngine;

// Mantém cada sala em memória: revisitar não recria inimigos, chaves ou tesouros.
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    [SerializeField] private Transform player;
    [SerializeField] private ArenaDefinition[] arenas;
    [SerializeField] private string startingArena = "estudio";
    [SerializeField] private Pickup ammoPrefab;
    [SerializeField] private Pickup keyPrefab;
    [SerializeField] private Sprite floorSprite, wallSprite, doorSprite, obstacleSprite;
    [SerializeField] private Material spriteMaterial;
    [SerializeField] private bool useTimeLimit = true;
    [SerializeField] private float duration = 35f;
    [SerializeField] private AudioClip roomClearSound;
    [SerializeField] private AudioClip bossApplause;
    private DeathEffect bossExplosion;
    private AudioSource applauseSource;
    private bool bossFeedbackPlayed;
    public bool BossFeedbackFinished => (bossExplosion == null || bossExplosion.Completed)
        && (applauseSource == null || !applauseSource.isPlaying);
    private sealed class Room
    {
        public ArenaDefinition definition;
        public GameObject root;
        public bool entered, cleared;
        public Pickup reward;
        public readonly List<Pickup> items = new List<Pickup>();
    }
    private readonly Dictionary<string, Room> rooms = new Dictionary<string, Room>();
    private Room current;
    private ArenaNavigation navigation;
    private float doorCooldown, ammoRetry;
    private float blockedSupplySince = -1f;
    public ArenaDefinition CurrentArena => current == null ? null : current.definition;
    public ArenaDefinition[] Arenas => arenas;
    public bool CurrentCleared => current != null && current.cleared;
    public EnemyChaser CurrentBoss { get; private set; }
    public string Announcement { get; private set; }
    public float AnnouncementUntil { get; private set; }

    private void Awake()
    {
        Instance = this;
        GameController.Init(duration, useTimeLimit);
    }
    private void Start()
    {
        if (player == null || arenas == null || arenas.Length == 0)
        { Debug.LogError("WaveManager: conecte Player e Arenas.", this); enabled = false; return; }
        foreach (ArenaDefinition definition in arenas)
        {
            if (definition == null || string.IsNullOrEmpty(definition.id) || rooms.ContainsKey(definition.id))
            { Debug.LogError("Arena ausente ou ID duplicado.", this); enabled = false; return; }
            Room room = new Room { definition = definition, root = new GameObject("Arena_" + definition.id) };
            room.root.transform.SetParent(transform);
            room.root.SetActive(false);
            rooms.Add(definition.id, room);
            BuildRoom(room);
        }
        if (!rooms.ContainsKey(startingArena)) { Debug.LogError("Arena inicial não encontrada.", this); enabled = false; return; }
        Enter(startingArena, -1);
    }
    private void Update()
    {
        if (GameController.gameOver && !GameController.victory && applauseSource != null) applauseSource.Stop();
        if (current == null || GameController.gameOver || GameController.Paused) return;
        GameController.UpdateTime(Time.deltaTime);
        if (GameController.gameOver || GameController.Transitioning) return;
        navigation.Refresh(player.position);
        if (!current.cleared && current.entered && GameController.enemiesRemaining == 0) ClearRoom();
        if (current.cleared && !GameController.gameOver) EnsureRewardKey();
        if (GameController.CombatActive) EnsureAmmo();
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    public Vector2 DirectionToPlayer(Vector2 from) => navigation == null ? Vector2.zero : navigation.Direction(from);
    public bool WasVisited(string id) => rooms.TryGetValue(id, out Room room) && room.entered;
    public bool WasCleared(string id) => rooms.TryGetValue(id, out Room room) && room.cleared;
    public bool HasRequiredKeys(string id)
    {
        if (!rooms.TryGetValue(id, out Room room)) return false;
        foreach (string key in room.definition.requiredKeys) if (!GameController.HasKey(key)) return false;
        return true;
    }
    public bool CanEnter(string id) => GameController.CanControl && CurrentCleared && rooms.ContainsKey(id) && HasRequiredKeys(id);
    public string EntryHint(string id)
    {
        if (!CurrentCleared) return "ELIMINE OS INIMIGOS PARA ABRIR AS PORTAS";
        if (!rooms.TryGetValue(id, out Room room)) return "DESTINO INDISPONÍVEL";
        List<string> missing = new List<string>();
        foreach (string key in room.definition.requiredKeys)
            if (!GameController.HasKey(key)) missing.Add(key);
        return "ACESSO BLOQUEADO · REQUER CREDENCIAL " + string.Join(" + ", missing);
    }
    public void TryEnter(string id, int arrivalSide)
    {
        if (Time.time < doorCooldown || !CanEnter(id) || ScreenTransition.IsBusy) return;
        GameController.Transitioning = true;
        if (!ScreenTransition.Run(() => Enter(id, arrivalSide), 0.2f, 0.08f, () =>
        {
            GameController.Transitioning = false;
            GameController.GrantProtection(0.65f);
        })) GameController.Transitioning = false;
    }
    private void Enter(string id, int side)
    {
        if (current != null) current.root.SetActive(false);
        current = rooms[id];
        current.root.SetActive(true);
        GameController.EnterRoom();
        CurrentBoss = null;
        Vector2 arrival = side == 0 ? new Vector2(0f, 3f) : side == 1 ? new Vector2(6f, 0f)
            : side == 2 ? new Vector2(0f, -3f) : side == 3 ? new Vector2(-6f, 0f) : new Vector2(0f, -2.5f);
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        player.position = arrival;
        if (body != null) { body.position = arrival; body.linearVelocity = Vector2.zero; body.angularVelocity = 0f; }
        Physics2D.SyncTransforms();
        navigation = new ArenaNavigation(current.definition.obstacles, current.definition.bossArena ? 0.55f : 0.4f);
        navigation.Refresh(arrival);
        doorCooldown = Time.time + 0.8f;
        ammoRetry = 0f;
        blockedSupplySince = -1f;
        if (!current.entered)
        {
            current.entered = true;
            int count = 0;
            foreach (EnemySpawn spawn in current.definition.enemies) if (spawn.prefab != null) count++;
            GameController.StartWave(count, current.definition.timeLimit);
            float difficulty = 1f + Mathf.Min(GameController.roomsCleared * 0.05f, 0.25f);
            foreach (EnemySpawn spawn in current.definition.enemies)
            {
                if (spawn.prefab == null) continue;
                EnemyChaser enemy = Instantiate(spawn.prefab, spawn.position, Quaternion.identity, current.root.transform);
                enemy.Init(player, difficulty);
                if (enemy.IsBoss) CurrentBoss = enemy;
            }
            foreach (ItemSpawn spawn in current.definition.items) SpawnItem(spawn.prefab, spawn.position);
        }
        Notify(current.cleared ? current.definition.displayName + " · ÁREA LIBERADA" : current.definition.announcement, 2.8f);
    }
    private void ClearRoom()
    {
        current.cleared = true;
        GameController.RegisterRoomCleared();
        if (current.definition.bossArena)
        {
            GameController.AddScore(500);
            GameController.Win();
            return;
        }
        GameAudio.Play(roomClearSound, 0.45f);
        if (!string.IsNullOrEmpty(current.definition.rewardKey)) EnsureRewardKey();
        else Notify("SALA LIBERADA · ESCOLHA A PRÓXIMA PORTA", 3f);
    }
    public void BossDefeated(DeathEffect effect)
    {
        if (bossFeedbackPlayed) return;
        bossFeedbackPlayed = true;
        bossExplosion = effect;
        applauseSource = GameAudio.Play(bossApplause, 0.7f);
        Notify("EXECUTOR ELIMINADO!", 3f);
    }
    private void EnsureRewardKey()
    {
        string id = current.definition.rewardKey;
        if (string.IsNullOrEmpty(id) || GameController.HasKey(id)) return;
        if (current.reward != null && current.reward.IsAvailable && navigation.IsReachable(current.reward.transform.position)) return;
        // Uma chave esquecida permanece na sala. Uma chave perdida é recolocada.
        if (current.reward != null)
        {
            current.reward.gameObject.SetActive(false);
            Destroy(current.reward.gameObject);
        }
        if (keyPrefab != null && navigation.TryFindPickupPosition(Vector2.zero, out Vector2 position))
        {
            current.reward = SpawnItem(keyPrefab, position);
            current.reward.SetKey(id);
            SpriteRenderer visual = current.reward.GetComponent<SpriteRenderer>();
            if (visual != null) visual.color = id == "A" ? new Color(1f, 0.75f, 0.2f) : new Color(0.3f, 0.85f, 1f);
            Notify("SALA LIBERADA · RECOLHA A CREDENCIAL " + id, 3.5f);
        }
        else
        {
            // Falha de configuração/espaço não pode tornar a final inacessível.
            GameController.AddKey(id);
            Debug.LogWarning("Credencial " + id + " entregue diretamente: não foi possível colocar o item.", this);
            Notify("CREDENCIAL " + id + " ENTREGUE PELA PRODUÇÃO", 3f);
        }
    }
    private Pickup SpawnItem(Pickup prefab, Vector2 position)
    {
        if (prefab == null) return null;
        Pickup item = Instantiate(prefab, position, Quaternion.identity, current.root.transform);
        current.items.Add(item);
        return item;
    }
    private void EnsureAmmo()
    {
        if (GameController.ammo > 2) { blockedSupplySince = -1f; return; }
        if (Time.time < ammoRetry) return;
        ammoRetry = Time.time + 1f;
        current.items.RemoveAll(item => item == null);
        foreach (Pickup item in current.items)
        {
            if (!item.IsAmmo || !item.IsAvailable || !navigation.IsReachable(item.transform.position)) continue;
            blockedSupplySince = -1f;
            return;
        }
        if (ammoPrefab != null && navigation.TryFindPickupPosition(player.position, out Vector2 position))
        {
            SpawnItem(ammoPrefab, position);
            blockedSupplySince = -1f;
            Notify("REPOSIÇÃO DE MUNIÇÃO DISPONÍVEL", 1.5f);
            return;
        }
        // Último recurso: zero tiros, nenhum pacote alcançável e nenhuma posição livre.
        // O prazo da sala continua correndo; a reserva não concede pontos.
        if (GameController.ammo != 0) { blockedSupplySince = -1f; return; }
        if (blockedSupplySince < 0f) blockedSupplySince = Time.time;
        if (Time.time - blockedSupplySince < 2f) return;
        GameController.AddAmmo(3);
        blockedSupplySince = -1f;
        Debug.LogWarning("Reserva de emergência: reposição física de munição indisponível.", this);
        Notify("RESERVA DE EMERGÊNCIA · +3 TIROS", 2f);
    }
    public void Notify(string message, float seconds = 2f)
    { Announcement = message; AnnouncementUntil = Time.time + seconds; }

    private SpriteRenderer Visual(string name, Sprite sprite, Vector2 position, Vector2 size, Color color, Transform parent, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;
        if (spriteMaterial != null) renderer.sharedMaterial = spriteMaterial;
        if (sprite != null)
            go.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1f);
        return renderer;
    }
    private BoxCollider2D Solid(SpriteRenderer visual)
    {
        visual.gameObject.layer = 7;
        BoxCollider2D collider = visual.gameObject.AddComponent<BoxCollider2D>();
        collider.size = visual.sprite == null ? Vector2.one : (Vector2)visual.sprite.bounds.size;
        collider.sharedMaterial = GamePhysics.NoFriction;
        return collider;
    }
    private void BuildRoom(Room room)
    {
        Transform root = room.root.transform;
        ArenaDefinition def = room.definition;
        for (int x = 0; x < 14; x++)
        for (int y = 0; y < 8; y++)
        {
            Color tint = def.floorColor * ((x + y) % 2 == 0 ? 1f : 0.92f); tint.a = 1f;
            Visual("Piso", floorSprite, new Vector2(x - 6.5f, y - 3.5f), Vector2.one, tint, root, -20);
        }
        // Faixas externas, painéis metálicos e obstáculos usam sprites Kenney existentes.
        for (int side = 0; side < 4; side++)
        {
            bool horizontal = side % 2 == 0;
            float edge = horizontal ? (side == 0 ? 4.15f : -4.15f) : (side == 1 ? 7.15f : -7.15f);
            float span = horizontal ? 14.6f : 8f;
            string target = def.Exit(side);
            if (string.IsNullOrEmpty(target))
                Solid(Visual("Parede", wallSprite, horizontal ? new Vector2(0f, edge) : new Vector2(edge, 0f), horizontal ? new Vector2(span, 0.3f) : new Vector2(0.3f, span), def.accentColor * 0.7f, root, 2));
            else
            {
                float segment = (span - 1.8f) * 0.5f;
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    float center = sign * (0.9f + segment * 0.5f);
                    Solid(Visual("Parede", wallSprite, horizontal ? new Vector2(center, edge) : new Vector2(edge, center), horizontal ? new Vector2(segment, 0.3f) : new Vector2(0.3f, segment), def.accentColor * 0.7f, root, 2));
                }
                Vector2 p = horizontal ? new Vector2(0f, edge) : new Vector2(edge, 0f);
                SpriteRenderer panel = Visual("Porta_" + target, doorSprite, p, horizontal ? new Vector2(1.8f, 0.3f) : new Vector2(0.3f, 1.8f), Color.white, root, 3);
                BoxCollider2D blocker = Solid(panel);
                Vector2 inward = -p.normalized;
                SpriteRenderer lamp = Visual("Sinal_porta", floorSprite, p + inward * 0.28f, horizontal ? new Vector2(1.8f, 0.08f) : new Vector2(0.08f, 1.8f), def.accentColor, root, 3);
                GameObject sensor = new GameObject("Passagem_" + target);
                sensor.layer = 2;
                sensor.transform.SetParent(root, false);
                sensor.transform.localPosition = p + inward * 0.32f;
                BoxCollider2D trigger = sensor.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = horizontal ? new Vector2(1.6f, 0.8f) : new Vector2(0.8f, 1.6f);
                sensor.AddComponent<ArenaDoor>().Configure(this, target, side, blocker, panel, lamp);
            }
        }
        foreach (ArenaObstacle obstacle in def.obstacles)
            Solid(Visual("Cobertura", obstacleSprite, obstacle.position, obstacle.size, new Color(0.55f, 0.65f, 0.72f), root, 1));
    }
}
