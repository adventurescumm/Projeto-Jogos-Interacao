using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Execute fora do Play Mode. Os testes de combate isolados pulam a apresentação;
// ValidateRoomPreparation verifica a preparação separadamente, nos três modos.
public static class ProjectValidation
{
    private static int checks;
    [MenuItem("King of the Kill/Validar projeto")]
    public static void Validate()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("Saia do Play Mode antes de validar."); return; }
        checks = 0;
        try
        {
            Check(EditorBuildSettings.scenes.Length >= 2, "Faltam cenas no Build Settings.");
            Check(EditorBuildSettings.scenes[0].path == "Assets/Scenes/MainMenu.unity", "O menu deve ser a primeira cena.");
            Check(EditorBuildSettings.scenes[1].path == "Assets/Scenes/Arena01Estudio.unity", "A arena deve ser a segunda cena.");
            Dictionary<string, ArenaDefinition> rooms = new Dictionary<string, ArenaDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:ArenaDefinition"))
            {
                ArenaDefinition room = AssetDatabase.LoadAssetAtPath<ArenaDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                Check(room != null && !string.IsNullOrEmpty(room.id), "Sala sem ID.");
                Check(!rooms.ContainsKey(room.id), "ID de sala duplicado: " + room.id);
                rooms.Add(room.id, room);
                Check(room.timeLimit > 0f, "Sala sem prazo: " + room.id);
                int previousCount = 0;
                foreach (DifficultyLevel level in Enum.GetValues(typeof(DifficultyLevel)))
                {
                    int count = GameDifficulty.EnemyCount(room, level);
                    Check(count > 5 && count > previousCount, "Cada modo deve acrescentar inimigos (mínimo 6): " + room.id);
                    Check(GameDifficulty.RoomTime(room, level) > 0f, "Prazo inválido: " + room.id);
                    int bosses = 0;
                    foreach (EnemySpawn spawn in room.enemies)
                        if (GameDifficulty.Includes(spawn, level) && spawn.prefab.IsBoss) bosses++;
                    Check(bosses == (room.bossArena ? 1 : 0), "Chefe ausente/duplicado: " + room.id);
                    previousCount = count;
                }
                int hitsNeeded = 0, floorAmmo = 0;
                foreach (EnemySpawn spawn in room.enemies)
                {
                    Check(spawn.prefab != null, "Inimigo ausente: " + room.id);
                    hitsNeeded += spawn.prefab.MaxHealth;
                }
                foreach (ItemSpawn spawn in room.items)
                {
                    Check(spawn.prefab != null, "Item ausente: " + room.id);
                    SerializedObject item = new SerializedObject(spawn.prefab);
                    if (item.FindProperty("type").intValue == (int)PickupType.Ammo)
                        floorAmmo += item.FindProperty("ammoAmount").intValue;
                }
                Check(floorAmmo >= Mathf.CeilToInt(hitsNeeded * 1.5f), "Munição sem margem de 50%: " + room.id);
            }
            Check(rooms.Count == 12, "O circuito desta versão deve ter doze salas.");
            HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
            foreach (ArenaDefinition room in rooms.Values)
                Check(positions.Add(room.mapPosition), "Posição de mapa duplicada: " + room.id);
            foreach (ArenaDefinition room in rooms.Values)
            for (int side = 0; side < 4; side++)
            {
                string destination = room.Exit(side);
                if (string.IsNullOrEmpty(destination)) continue;
                Check(rooms.ContainsKey(destination), "Saída inexistente: " + destination);
                Check(rooms[destination].Exit((side + 2) % 4) == room.id, "Porta sem caminho de volta: " + room.id);
                Vector2Int step = side == 0 ? Vector2Int.up : side == 1 ? Vector2Int.right : side == 2 ? Vector2Int.down : Vector2Int.left;
                Check(rooms[destination].mapPosition == room.mapPosition + step, "Porta diverge do minimapa: " + room.id);
            }
            Check(rooms["final"].requiredKeys.Length == 2, "A final deve exigir duas credenciais.");
            HashSet<string> visited = new HashSet<string>();
            HashSet<string> keys = new HashSet<string>();
            bool changed;
            do
            {
                changed = false;
                foreach (ArenaDefinition room in rooms.Values)
                {
                    if (visited.Contains(room.id)) continue;
                    bool reachable = room.id == "estudio";
                    for (int side = 0; side < 4; side++) reachable |= visited.Contains(room.Exit(side) ?? "");
                    bool unlocked = true;
                    foreach (string key in room.requiredKeys) unlocked &= keys.Contains(key);
                    if (!reachable || !unlocked) continue;
                    visited.Add(room.id);
                    if (!string.IsNullOrEmpty(room.rewardKey)) keys.Add(room.rewardKey);
                    changed = true;
                }
            } while (changed);
            Check(visited.Count == rooms.Count, "Há salas inacessíveis ou bloqueio circular de credenciais.");
            ValidateProgression(rooms);
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                    Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0, "Script ausente em " + prefab.name);
                EnemyChaser enemy = prefab.GetComponent<EnemyChaser>();
                if (enemy != null) Check(prefab.layer == 6 && prefab.GetComponent<Rigidbody2D>().gravityScale == 0f, "Física/layer de inimigo: " + prefab.name);
            }
            ValidateState();
            ValidatePauseAndAim();
            ValidateDifficultiesAndRecovery();
            ValidateRoomPreparation();
            Debug.Log("King of the Kill: " + checks + " verificações concluídas. Física, visual, áudio e Web ainda exigem Play Mode/Build and Run.");
        }
        catch (Exception error) { Debug.LogException(error); throw; }
        finally { GameController.Init(35f, true); }
    }
    private static void Check(bool condition, string message)
    { checks++; if (!condition) throw new InvalidOperationException(message); }
    private static void StartCombat(int count, float duration = -1f) => GameController.StartWave(count, duration, false);

    private static void ValidateRoomPreparation()
    {
        foreach (DifficultyLevel level in Enum.GetValues(typeof(DifficultyLevel)))
        {
            GameController.Init(21f, true, level);
            GameController.EnterRoom();
            GameController.StartWave(10, 21f);
            int ammo = GameController.ammo;
            Check(GameController.Preparing && !GameController.CanControl && GameController.Invulnerable, "A preparação não bloqueou os controles.");
            GameController.TakeDamage();
            Check(!GameController.TrySpendAmmo() && GameController.ammo == ammo && GameController.health == 3, "Dano/tiro durante Get Ready.");
            GameController.UpdateTime(0.75f);
            float remaining = GameController.PreparationTimeRemaining;
            Check(Mathf.Abs(remaining - 1.25f) < 0.0001f && GameController.waveTimeRemaining == 21f
                && GameController.elapsedTime == 0f, "A preparação consumiu tempo de combate.");
            Check(GameController.SetPaused(true), "Não foi possível pausar durante Get Ready.");
            GameController.UpdateTime(100f);
            Check(GameController.PreparationTimeRemaining == remaining, "A preparação avançou durante a pausa.");
            GameController.SetPaused(false);
            GameController.Transitioning = true;
            GameController.UpdateTime(100f);
            Check(GameController.PreparationTimeRemaining == remaining, "A preparação avançou durante a transição.");
            GameController.Transitioning = false;
            GameController.UpdateTime(remaining);
            Check(!GameController.Preparing && GameController.CanControl && GameController.waveTimeRemaining == 21f
                && GameController.Invulnerable, "Fim da preparação inválido ou sem proteção de entrada.");
            GameController.UpdateTime(1f);
            Check(GameController.waveTimeRemaining == 20f && GameController.elapsedTime == 1f, "Combate não começou após Get Ready.");
            GameController.EnterRoom();
            Check(!GameController.Preparing && !GameController.CombatActive, "Entrar numa sala limpa recriou a preparação.");
            GameController.StartWave(7, 30f);
            Check(GameController.Preparing && GameController.PreparationTimeRemaining == 2f, "Próxima sala não reiniciou a preparação.");
            GameController.ReconcileEnemies(0);
            Check(!GameController.Preparing && !GameController.CombatActive, "Sala sem inimigos ficou presa na preparação.");
            GameController.StartWave(7, 30f); GameController.SetPaused(true);
            GameController.Init(30f, true, level);
            Check(!GameController.Preparing && !GameController.Paused && Time.timeScale == 1f, "Reinício reteve preparação/pausa.");
            GameController.StartWave(7, 30f); GameController.Win();
            Check(!GameController.Preparing && GameController.victory, "Resultado deixou Get Ready ativo.");
        }
    }

    private static void ValidatePauseAndAim()
    {
        GameController.Init(35f, true);
        StartCombat(5);
        GameController.UpdateTime(2f);
        float remaining = GameController.waveTimeRemaining, elapsed = GameController.elapsedTime;
        int ammo = GameController.ammo;
        Check(GameController.SetPaused(true), "Não foi possível pausar.");
        GameController.UpdateTime(100f); GameController.TakeDamage();
        Check(GameController.Paused && !GameController.CanControl && Time.timeScale == 0f && AudioListener.pause, "Pausa não bloqueou simulação/áudio.");
        Check(GameController.waveTimeRemaining == remaining && GameController.elapsedTime == elapsed, "Relógio avançou durante a pausa.");
        Check(!GameController.TrySpendAmmo() && GameController.ammo == ammo && GameController.health == 3, "Pausa permitiu dano/munição.");
        Check(GameController.SetPaused(false) && GameController.CanControl && Time.timeScale == 1f && !AudioListener.pause, "Retornar manteve jogo pausado.");
        Check(GameController.SuppressPlayerInput && !GameController.TrySpendAmmo(), "Clique de Retornar atravessa UI.");
        GameController.UpdateTime(1f);
        Check(GameController.waveTimeRemaining == remaining - 1f && GameController.enemiesRemaining == 5, "Retornar reiniciou combate/prazo.");
        GameController.Transitioning = true;
        Check(!GameController.SetPaused(true), "Pausa abriu durante transição.");
        GameController.Init(35f, true); StartCombat(5); GameController.SetPaused(true);
        GameController.AddKey("A"); GameController.Init(35f, true);
        Check(!GameController.Paused && !AudioListener.pause && Time.timeScale == 1f && GameController.KeyCount == 0, "Reinício reteve estado da pausa.");
        GameController.Win(); Check(!GameController.SetPaused(true), "Pausa abriu sobre resultado.");
        Vector2 thirty = Vector2.right, sixty = Vector2.right, oneTwenty = Vector2.right;
        for (int i = 0; i < 6; i++) thirty = PlayerShooter.SmoothDirection(thirty, Vector2.up, 18f, 1f / 30f);
        for (int i = 0; i < 12; i++) sixty = PlayerShooter.SmoothDirection(sixty, Vector2.up, 18f, 1f / 60f);
        for (int i = 0; i < 24; i++) oneTwenty = PlayerShooter.SmoothDirection(oneTwenty, Vector2.up, 18f, 1f / 120f);
        Check(Vector2.Distance(thirty, sixty) < 0.0001f && Vector2.Distance(sixty, oneTwenty) < 0.0001f, "Mira depende do FPS.");
        Vector2 reverse = PlayerShooter.SmoothDirection(Vector2.right, Vector2.left, 45f, 1f / 60f);
        Check(Mathf.Abs(reverse.magnitude - 1f) < 0.0001f && !float.IsNaN(reverse.x), "Virada gerou direção inválida.");
        Check(PlayerShooter.SmoothDirection(Vector2.up, Vector2.zero, 18f, 1f) == Vector2.up, "Soltar perdeu direção.");
        foreach (string name in new[] { "Projectile", "EnemyProjectile" })
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + name + ".prefab");
            Check(prefab.GetComponent<SpriteRenderer>().sprite != null, "Sprite de bala ausente: " + name);
        }
        Check(AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BlenderFoundation/BossApplause.wav") != null, "Aplausos não importados.");
    }

    private static void ValidateDifficultiesAndRecovery()
    {
        foreach (DifficultyLevel level in Enum.GetValues(typeof(DifficultyLevel)))
        {
            GameController.Init(30f, true, level);
            Check(GameController.Difficulty == level && GameController.ammo == GameDifficulty.StartingAmmo(level), "Perfil não aplicado.");
            Check(GameController.health == 3, "Dificuldade alterou três vidas.");
            GameController.EnterRoom(); StartCombat(12, 30f);
            GameController.ReconcileEnemies(8);
            Check(GameController.enemiesRemaining == 8 && GameController.score == 0 && GameController.CombatActive, "Reconciliação inválida.");
            GameController.SetPaused(true); GameController.ReconcileEnemies(0);
            Check(GameController.enemiesRemaining == 8, "Reconciliação mudou combate pausado.");
            GameController.SetPaused(false); GameController.ReconcileEnemies(0);
            GameController.UpdateTime(40f);
            Check(!GameController.CombatActive && !GameController.gameOver, "Combate vazio permaneceu ativo.");
        }
        Check(ControllerSettings.Radial(new Vector2(0.1f, 0.1f), 0.16f) == Vector2.zero, "Drift dentro da zona morta.");
        Check(Mathf.Abs(ControllerSettings.Radial(Vector2.one, 0.16f).magnitude - 1f) < 0.0001f, "Diagonal ultrapassa velocidade.");
        Check(ControllerSettings.Radial(new Vector2(0.58f, 0f), 0.16f).x > 0.49f
            && ControllerSettings.Radial(new Vector2(0.58f, 0f), 0.16f).x < 0.51f, "Intensidade analógica inválida.");
    }

    private static void ValidateProgression(Dictionary<string, ArenaDefinition> rooms)
    {
        var start = (room: "estudio", keys: 0);
        var reached = new HashSet<(string room, int keys)> { start };
        var pending = new Queue<(string room, int keys)>();
        var predecessors = new Dictionary<(string room, int keys), List<(string room, int keys)>>();
        pending.Enqueue(start);
        while (pending.Count > 0)
        {
            var state = pending.Dequeue();
            foreach (var next in NextStates(state, rooms))
            {
                if (!predecessors.ContainsKey(next)) predecessors.Add(next, new List<(string room, int keys)>());
                predecessors[next].Add(state);
                if (reached.Add(next)) pending.Enqueue(next);
            }
        }
        var victory = (room: "final", keys: 3);
        Check(reached.Contains(victory), "Não há caminho até a vitória.");
        var canWin = new HashSet<(string room, int keys)> { victory };
        pending.Enqueue(victory);
        while (pending.Count > 0)
        {
            var state = pending.Dequeue();
            if (!predecessors.TryGetValue(state, out var previous)) continue;
            foreach (var before in previous) if (canWin.Add(before)) pending.Enqueue(before);
        }
        foreach (var state in reached)
            Check(canWin.Contains(state), "Estado sem caminho até a final: " + state.room + ", credenciais=" + state.keys);
    }
    private static IEnumerable<(string room, int keys)> NextStates((string room, int keys) state, Dictionary<string, ArenaDefinition> rooms)
    {
        ArenaDefinition room = rooms[state.room];
        if (room.bossArena) yield break;
        int reward = KeyBit(room.rewardKey);
        if (reward != 0 && (state.keys & reward) == 0) yield return (state.room, state.keys | reward);
        for (int side = 0; side < 4; side++)
        {
            string target = room.Exit(side);
            if (string.IsNullOrEmpty(target)) continue;
            int keyMask = 0;
            foreach (string key in rooms[target].requiredKeys) keyMask |= KeyBit(key);
            if ((state.keys & keyMask) == keyMask) yield return (target, state.keys);
        }
    }
    private static int KeyBit(string id)
    {
        if (string.IsNullOrEmpty(id)) return 0;
        if (id == "A") return 1;
        if (id == "B") return 2;
        throw new InvalidOperationException("Credencial não prevista: " + id);
    }

    private static void ValidateState()
    {
        GameController.Init(5f, true, DifficultyLevel.Normal);
        Check(!GameController.gameOver && GameController.health == 3 && GameController.ammo == 24, "Estado inicial inválido.");
        GameController.UpdateTime(20f);
        Check(!GameController.gameOver, "Tempo fora de combate causou derrota.");
        GameController.EnterRoom(); StartCombat(1, 5f); GameController.UpdateTime(4f); GameController.RegisterKill();
        GameController.UpdateTime(50f);
        Check(!GameController.gameOver && GameController.score == 10, "Sala concluída/pontuação incorretas.");
        GameController.EnterRoom(); StartCombat(1, 5f); GameController.UpdateTime(1f);
        Check(GameController.waveTimeRemaining == 4f && !GameController.gameOver, "Sala usou tempo total como limite.");
        GameController.Transitioning = true; GameController.UpdateTime(99f);
        Check(GameController.waveTimeRemaining == 4f && !GameController.TrySpendAmmo(), "Transição não bloqueou combate.");
        GameController.Transitioning = false; GameController.UpdateTime(5f);
        Check(GameController.Result == MatchResult.Timeout, "Prazo não causou derrota.");
        GameController.Win(); Check(GameController.Result == MatchResult.Timeout, "Resultado sobrescrito.");
        GameController.Init(5f, true, DifficultyLevel.Normal);
        Check(!GameController.TryHeal(), "Cura com vida cheia.");
        GameController.AddAmmo(8); Check(GameController.ammo == 32 && GameController.score == 0, "Munição concedeu pontos.");
        Check(GameController.AddKey("A") && !GameController.AddKey("A"), "Credencial duplicada.");
        Check(!GameController.HasKey("B"), "Credencial B já existe.");
        GameController.AddKey("B"); Check(GameController.KeyCount == 2, "Contagem de credenciais.");
        StartCombat(1);
        FieldInfo immunity = typeof(GameController).GetField("nextDamageTime", BindingFlags.NonPublic | BindingFlags.Static);
        immunity.SetValue(null, -1f); GameController.TakeDamage(); GameController.TakeDamage();
        Check(GameController.health == 2, "Invulnerabilidade não protegeu de impactos repetidos.");
        Check(GameController.TryHeal() && GameController.health == 3, "Cura inválida.");
        for (int i = 0; i < 3; i++) { immunity.SetValue(null, -1f); GameController.TakeDamage(); }
        Check(GameController.health == 0 && GameController.Result == MatchResult.Eliminated, "Três danos não encerraram partida.");
        GameController.UpdateTime(99f); Check(GameController.Result == MatchResult.Eliminated, "Prazo substituiu motivo da morte.");
        GameController.Init(5f, true, DifficultyLevel.Normal); GameController.Win();
        Check(GameController.victory && !GameController.CanControl, "Vitória não bloqueou controles.");
        GameController.Init(5f, true, DifficultyLevel.Normal);
        Check(GameController.KeyCount == 0 && GameController.roomsCleared == 0 && !GameController.gameOver, "Reinício reteve progresso.");
        GameController.EnterRoom(); StartCombat(5, 35f);
        for (int i = 0; i < 4; i++)
        {
            GameController.RegisterKill();
            Check(GameController.enemiesRemaining == 4 - i && GameController.CombatActive, "Sala liberada antes da última eliminação.");
        }
        GameController.RegisterKill();
        Check(GameController.enemiesRemaining == 0 && !GameController.CombatActive && GameController.score == 50, "Última eliminação não encerrou combate.");
        GameController.RegisterKill(); GameController.UpdateTime(99f);
        Check(GameController.enemiesRemaining == 0 && GameController.score == 50 && !GameController.gameOver, "Eliminação duplicada/prazo corrompeu estado.");
    }
}
