using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Execute pelo menu depois da importação. Não requer montar uma cena de testes.
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
                Check(room.enemies.Length >= 5, "A sala deve ter pelo menos cinco inimigos: " + room.id);
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
                Check(floorAmmo >= Mathf.CeilToInt(hitsNeeded * 1.5f),
                    "Munição da sala sem margem de 50% de tiros extras: " + room.id);
            }
            Check(rooms.Count == 12, "O circuito desta versão deve ter doze salas.");
            HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
            foreach (ArenaDefinition room in rooms.Values)
                Check(positions.Add(room.mapPosition), "Duas salas ocupam a mesma posição no mapa: " + room.id);
            foreach (ArenaDefinition room in rooms.Values)
            for (int side = 0; side < 4; side++)
            {
                string destination = room.Exit(side);
                if (string.IsNullOrEmpty(destination)) continue;
                Check(rooms.ContainsKey(destination), "Saída inexistente: " + destination);
                Check(rooms[destination].Exit((side + 2) % 4) == room.id, "Porta sem caminho de volta: " + room.id);
                Vector2Int step = side == 0 ? Vector2Int.up : side == 1 ? Vector2Int.right : side == 2 ? Vector2Int.down : Vector2Int.left;
                Check(rooms[destination].mapPosition == room.mapPosition + step, "Porta não corresponde à posição no minimapa: " + room.id);
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
            Check(visited.Count == rooms.Count, "Há salas inacessíveis ou um bloqueio circular de credenciais.");
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
            Debug.Log("King of the Kill: " + checks + " verificações concluídas. Física, visual, áudio e Web ainda exigem Play Mode/Build and Run.");
        }
        catch (Exception error) { Debug.LogException(error); throw; }
        finally { GameController.Init(35f, true); }
    }
    private static void Check(bool condition, string message)
    { checks++; if (!condition) throw new InvalidOperationException(message); }

    private static void ValidatePauseAndAim()
    {
        GameController.Init(35f, true);
        GameController.StartWave(5);
        GameController.UpdateTime(2f);
        float remaining = GameController.waveTimeRemaining, elapsed = GameController.elapsedTime;
        int ammo = GameController.ammo;
        Check(GameController.SetPaused(true), "Não foi possível pausar a partida.");
        GameController.UpdateTime(100f); GameController.TakeDamage();
        Check(GameController.Paused && !GameController.CanControl && Time.timeScale == 0f && AudioListener.pause,
            "A pausa não bloqueou simulação/áudio/controles.");
        Check(GameController.waveTimeRemaining == remaining && GameController.elapsedTime == elapsed,
            "O relógio avançou durante a pausa.");
        Check(!GameController.TrySpendAmmo() && GameController.ammo == ammo && GameController.health == 3,
            "A pausa permitiu dano ou consumo de munição.");
        Check(GameController.SetPaused(false) && GameController.CanControl && Time.timeScale == 1f && !AudioListener.pause,
            "Retornar deixou o jogo ou o áudio pausado.");
        Check(GameController.SuppressPlayerInput && !GameController.TrySpendAmmo(),
            "O clique de Retornar pode atravessar a UI e disparar.");
        GameController.UpdateTime(1f);
        Check(GameController.waveTimeRemaining == remaining - 1f && GameController.enemiesRemaining == 5,
            "Retornar reiniciou o combate ou perdeu o prazo.");
        GameController.Transitioning = true;
        Check(!GameController.SetPaused(true), "Foi possível abrir a pausa durante a transição.");
        GameController.Init(35f, true); GameController.StartWave(5); GameController.SetPaused(true);
        GameController.AddKey("A"); GameController.Init(35f, true);
        Check(!GameController.Paused && !AudioListener.pause && Time.timeScale == 1f && GameController.KeyCount == 0,
            "Reiniciar a partir da pausa reteve estado ou áudio pausado.");
        GameController.Win(); Check(!GameController.SetPaused(true), "A pausa abriu por cima do resultado.");
        Vector2 thirty = Vector2.right, sixty = Vector2.right, oneTwenty = Vector2.right;
        for (int i = 0; i < 6; i++) thirty = PlayerShooter.SmoothDirection(thirty, Vector2.up, 18f, 1f / 30f);
        for (int i = 0; i < 12; i++) sixty = PlayerShooter.SmoothDirection(sixty, Vector2.up, 18f, 1f / 60f);
        for (int i = 0; i < 24; i++) oneTwenty = PlayerShooter.SmoothDirection(oneTwenty, Vector2.up, 18f, 1f / 120f);
        Check(Vector2.Distance(thirty, sixty) < 0.0001f && Vector2.Distance(sixty, oneTwenty) < 0.0001f,
            "A velocidade da mira depende do FPS.");
        Vector2 reverse = PlayerShooter.SmoothDirection(Vector2.right, Vector2.left, 45f, 1f / 60f);
        Check(Mathf.Abs(reverse.magnitude - 1f) < 0.0001f && !float.IsNaN(reverse.x),
            "Virar 180 graus gerou uma direção inválida.");
        Check(PlayerShooter.SmoothDirection(Vector2.up, Vector2.zero, 18f, 1f) == Vector2.up,
            "Soltar o analógico perdeu a última direção.");
        foreach (string name in new[] { "Projectile", "EnemyProjectile" })
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + name + ".prefab");
            Check(prefab.GetComponent<SpriteRenderer>().sprite != null, "Sprite de bala ausente: " + name);
        }
        Check(AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BlenderFoundation/BossApplause.wav") != null,
            "Aplausos não importados.");
    }

    private static void ValidateProgression(Dictionary<string, ArenaDefinition> rooms)
    {
        // A coleta é opcional: sair da sala sem pegar a chave também é um estado válido.
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
            Check(canWin.Contains(state), "Estado sem saída até a final: " + state.room + ", credenciais=" + state.keys);
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
        throw new InvalidOperationException("Credencial não prevista na validação: " + id);
    }

    private static void ValidateState()
    {
        GameController.Init(5f, true);
        Check(!GameController.gameOver && GameController.health == 3 && GameController.ammo == 20, "Estado inicial inválido.");
        GameController.UpdateTime(20f);
        Check(!GameController.gameOver, "Tempo fora de combate causou derrota.");
        GameController.EnterRoom(); GameController.StartWave(1, 5f); GameController.UpdateTime(4f); GameController.RegisterKill();
        GameController.UpdateTime(50f);
        Check(!GameController.gameOver && GameController.score == 10, "Sala concluída/pontuação incorretas.");
        GameController.EnterRoom(); GameController.StartWave(1, 5f); GameController.UpdateTime(1f);
        Check(GameController.waveTimeRemaining == 4f && !GameController.gameOver, "A sala usou o tempo total como limite.");
        GameController.Transitioning = true; GameController.UpdateTime(99f);
        Check(GameController.waveTimeRemaining == 4f && !GameController.TrySpendAmmo(), "Transição não bloqueou o combate.");
        GameController.Transitioning = false; GameController.UpdateTime(5f);
        Check(GameController.Result == MatchResult.Timeout, "O prazo não causou derrota.");
        GameController.Win(); Check(GameController.Result == MatchResult.Timeout, "O resultado foi sobrescrito.");
        GameController.Init(5f, true);
        Check(!GameController.TryHeal(), "Cura foi consumida com vida cheia.");
        GameController.AddAmmo(8); Check(GameController.ammo == 28 && GameController.score == 0, "Munição concedeu pontos.");
        Check(GameController.AddKey("A") && !GameController.AddKey("A"), "Credencial duplicada.");
        Check(!GameController.HasKey("B"), "Credencial B já existe antes da coleta.");
        GameController.AddKey("B"); Check(GameController.KeyCount == 2, "Contagem de credenciais.");
        GameController.StartWave(1);
        FieldInfo immunity = typeof(GameController).GetField("nextDamageTime", BindingFlags.NonPublic | BindingFlags.Static);
        immunity.SetValue(null, -1f); GameController.TakeDamage(); GameController.TakeDamage();
        Check(GameController.health == 2, "Invulnerabilidade não protegeu de impactos repetidos.");
        Check(GameController.TryHeal() && GameController.health == 3, "Cura inválida.");
        for (int i = 0; i < 3; i++) { immunity.SetValue(null, -1f); GameController.TakeDamage(); }
        Check(GameController.health == 0 && GameController.Result == MatchResult.Eliminated, "Três danos não encerraram a partida.");
        GameController.UpdateTime(99f); Check(GameController.Result == MatchResult.Eliminated, "O prazo substituiu o motivo da morte.");
        GameController.Init(5f, true); GameController.Win();
        Check(GameController.victory && !GameController.CanControl, "Vitória não bloqueou controles.");
        GameController.Init(5f, true);
        Check(GameController.KeyCount == 0 && GameController.roomsCleared == 0 && !GameController.gameOver, "Reinício reteve progresso.");
        GameController.EnterRoom(); GameController.StartWave(5, 35f);
        for (int i = 0; i < 4; i++)
        {
            GameController.RegisterKill();
            Check(GameController.enemiesRemaining == 4 - i && GameController.CombatActive,
                "A sala foi liberada antes da quinta eliminação.");
        }
        GameController.RegisterKill();
        Check(GameController.enemiesRemaining == 0 && !GameController.CombatActive && GameController.score == 50,
            "A quinta eliminação não encerrou o combate corretamente.");
        GameController.RegisterKill(); GameController.UpdateTime(99f);
        Check(GameController.enemiesRemaining == 0 && GameController.score == 50 && !GameController.gameOver,
            "Eliminação duplicada ou prazo após a limpeza corrompeu o estado.");
    }
}
