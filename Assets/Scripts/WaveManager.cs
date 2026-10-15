using System.Collections.Generic; // Disponibiliza listas.
using UnityEngine; // Disponibiliza os recursos da Unity.

public class WaveManager : MonoBehaviour // Controla a partida e a criação das ondas.
{
    [SerializeField] private Transform player; // Jogador usado como alvo e referência de distância.
    [SerializeField] private EnemyChaser enemyPrefab; // Prefab do inimigo.
    [SerializeField] private Pickup ammoPrefab; // Prefab de munição.
    [SerializeField] private Pickup healthPrefab; // Prefab de vida.
    [SerializeField] private Transform[] spawnPoints; // Marcadores de surgimento.
    [SerializeField] private LayerMask occupiedLayers; // Layers que bloqueiam um ponto.

    [SerializeField, Min(1)] private int enemiesPerWave = 3; // Quantidade de inimigos por onda.
    [SerializeField] private float initialSpeed = 2f; // Velocidade inicial dos inimigos.
    [SerializeField] private float speedIncrease = 0.15f; // Aumento de velocidade por onda.
    [SerializeField] private float maximumSpeed = 3.5f; // Limite de velocidade.
    [SerializeField] private float playerClearance = 2f; // Distância mínima até o jogador.
    [SerializeField] private float spawnRadius = 0.6f; // Espaço que precisa estar livre no marcador.

    [SerializeField] private bool useTimeLimit = false; // Permite habilitar a derrota por tempo.
    [SerializeField] private float duration = 60f; // Mantém o prazo de sessenta segundos como opção.

    private readonly List<Pickup> items = new List<Pickup>(); // Guarda os itens criados.

    private void Awake() // Inicializa os dados antes dos métodos Start.
    {
        GameController.Init(duration, useTimeLimit); // Reinicia vida, munição, pontos e cronômetro.
    }

    private void Start() // Executa após a inicialização dos componentes.
    {
        StartNextWave(); // Cria a primeira onda.
    }

    private void Update() // Acompanha o estado da partida.
    {
        if (GameController.gameOver) return; // Impede novas ondas depois da derrota.

        GameController.UpdateTime(Time.deltaTime); // Atualiza o cronômetro.

        if (GameController.gameOver) return; // Interrompe se o prazo acabou nesta atualização.

        if (GameController.enemiesRemaining == 0) // Verifica se todos os inimigos morreram.
        {
            StartNextWave(); // Cria a próxima onda.
        }

        EnsureAmmoAvailable(); // Evita esgotar todos os tiros e todos os pacotes da arena.
    }

    private void StartNextWave() // Prepara uma nova onda.
    {
        ClearItems(); // Remove itens que sobraram da onda anterior.

        List<Transform> available = GetAvailablePoints(); // Obtém os marcadores livres.
        int required = enemiesPerWave + 3; // Reserva locais para os inimigos, duas munições e uma vida.

        if (available.Count < required) // Verifica se existem locais suficientes.
        {
            enabled = false; // Evita repetir o erro a cada quadro.
            throw new System.InvalidOperationException("Adicione mais pontos livres e afastados do jogador."); // Informa o problema.
        }

        GameController.StartWave(enemiesPerWave); // Registra o número da onda e a quantidade de inimigos.

        float speed = initialSpeed + (GameController.wave - 1) * speedIncrease; // Calcula a progressão.
        speed = Mathf.Min(speed, maximumSpeed); // Aplica o limite de velocidade.

        for (int i = 0; i < enemiesPerWave; i++) // Cria os inimigos desta onda.
        {
            Vector3 position = TakePosition(available); // Sorteia uma posição ainda não utilizada.
            EnemyChaser enemy = Instantiate(enemyPrefab, position, Quaternion.identity, transform); // Cria o inimigo.
            enemy.Init(player, speed); // Define seu alvo e sua velocidade.
        }

        for (int i = 0; i < 2; i++) // Cria dois pacotes de munição.
        {
            SpawnItem(ammoPrefab, TakePosition(available)); // Usa uma posição diferente para cada pacote.
        }

        SpawnItem(healthPrefab, TakePosition(available)); // Cria um item de vida.
    }

    private List<Transform> GetAvailablePoints() // Encontra os marcadores adequados.
    {
        List<Transform> available = new List<Transform>(); // Prepara a lista de candidatos.

        foreach (Transform point in spawnPoints) // Examina os marcadores configurados.
        {
            if (point == null || available.Contains(point)) continue; // Ignora referências vazias ou repetidas.

            float distance = Vector2.Distance(player.position, point.position); // Mede a distância até o jogador.

            if (distance < playerClearance) continue; // Descarta pontos próximos do jogador.
            if (Physics2D.OverlapCircle(point.position, spawnRadius, occupiedLayers) != null) continue; // Descarta pontos ocupados.

            available.Add(point); // Inclui o marcador no sorteio.
        }

        return available; // Devolve os candidatos.
    }

    private Vector3 TakePosition(List<Transform> available) // Sorteia uma posição sem repeti-la.
    {
        int index = Random.Range(0, available.Count); // Escolhe um índice válido.
        Vector3 position = available[index].position; // Guarda a posição correspondente.
        available.RemoveAt(index); // Retira o marcador deste sorteio.
        return position; // Entrega a posição escolhida.
    }

    private void SpawnItem(Pickup prefab, Vector3 position) // Cria um item e registra sua referência.
    {
        Pickup item = Instantiate(prefab, position, Quaternion.identity, transform); // Cria uma cópia do prefab.
        items.Add(item); // Guarda o item para consultas e limpeza.
    }

    private void ClearItems() // Remove os itens restantes.
    {
        foreach (Pickup item in items) // Examina os itens registrados.
        {
            if (item == null) continue; // Ignora os que já foram destruídos.

            item.gameObject.SetActive(false); // Desativa o collider imediatamente.
            Destroy(item.gameObject); // Solicita a destruição.
        }

        items.Clear(); // Esvazia o registro.
    }

    private void EnsureAmmoAvailable() // Repõe um pacote quando todos os recursos de munição acabam.
    {
        if (GameController.ammo > 0) return; // Não repõe enquanto o jogador tem tiros.

        items.RemoveAll(item => item == null); // Remove referências de itens já destruídos.

        foreach (Pickup item in items) // Examina os itens restantes.
        {
            if (item.IsAmmo && item.gameObject.activeSelf) return; // Aguarda a coleta dos pacotes que ainda existem.
        }

        List<Transform> available = GetAvailablePoints(); // Procura locais livres.

        if (available.Count == 0) return; // Tenta novamente depois, caso nenhum local esteja livre.

        SpawnItem(ammoPrefab, TakePosition(available)); // Cria um pacote de reposição.
    }
}