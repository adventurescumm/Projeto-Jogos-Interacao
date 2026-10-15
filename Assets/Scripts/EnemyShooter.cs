using UnityEngine; // Disponibiliza os componentes e recursos da Unity.

[RequireComponent(typeof(SpriteRenderer))] // Exige o componente visual no mesmo objeto.
public class EnemyShooter : MonoBehaviour // Controla a preparação e o disparo do inimigo.
{
    [SerializeField] private EnemyProjectile projectilePrefab; // Prefab do tiro inimigo.
    [SerializeField] private float shootingRange = 8f; // Distância máxima para tentar atirar.

    [SerializeField, Min(0.1f)] private float minPause = 2f; // Menor pausa antes de preparar outro tiro.
    [SerializeField, Min(0.1f)] private float maxPause = 3f; // Maior pausa antes de preparar outro tiro.
    [SerializeField, Min(0.05f)] private float warningDuration = 0.35f; // Duração do aviso antes do disparo.

    [SerializeField] private Color warningColor = Color.yellow; // Cor mostrada durante a preparação.
    [SerializeField] private LayerMask obstacleLayers; // Layers que bloqueiam a visão até o jogador.

    private Transform player; // Guarda o jogador recebido durante a criação do inimigo.
    private SpriteRenderer enemySprite; // Guarda o componente visual do inimigo.
    private Color originalColor; // Guarda a cor que deverá ser restaurada.

    private bool preparingShot; // Indica se o inimigo está preparando um tiro.
    private float fireTime; // Instante em que a preparação terminará.
    private float nextPreparationTime; // Instante em que uma nova preparação será permitida.

    private void Awake() // Obtém as referências ao criar o componente.
    {
        enemySprite = GetComponent<SpriteRenderer>(); // Localiza o componente visual.
        originalColor = enemySprite.color; // Guarda a cor inicial.
    }

    public void Init(Transform target) // Recebe o alvo durante a criação do inimigo.
    {
        player = target; // Define quem será atacado.
        preparingShot = false; // Começa sem nenhum tiro em preparação.
        nextPreparationTime = Time.time + Random.Range(0.4f, 1f); // Varia o atraso inicial entre os inimigos.
    }

    private void Update() // Controla a preparação e os disparos a cada quadro.
    {
        if (GameController.gameOver || player == null) // Verifica se o combate pode continuar.
        {
            enemySprite.color = originalColor; // Retira o aviso visual.
            preparingShot = false; // Cancela a preparação.
            return; // Impede disparos.
        }

        if (preparingShot) // Verifica se existe um tiro sendo preparado.
        {
            if (Time.time < fireTime) return; // Aguarda o término do aviso.

            preparingShot = false; // Encerra a preparação.
            enemySprite.color = originalColor; // Restaura a aparência normal.

            float pause = Random.Range(minPause, Mathf.Max(minPause, maxPause)); // Sorteia uma pausa válida.
            nextPreparationTime = Time.time + pause; // Agenda quando poderá preparar outro tiro.

            if (!CanShoot()) return; // Cancela o disparo se o jogador saiu do alcance ou ficou atrás de uma parede.

            Vector2 direction = player.position - transform.position; // Mira na posição atual do jogador.
            EnemyProjectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity); // Cria o tiro sem torná-lo filho do inimigo.
            projectile.Init(direction); // Entrega a direção que o projétil seguirá.

            return; // Encerra esta atualização após o disparo.
        }

        if (Time.time < nextPreparationTime || !CanShoot()) return; // Aguarda a pausa, o alcance e a visão livre.

        preparingShot = true; // Inicia a preparação.
        fireTime = Time.time + warningDuration; // Define quando o tiro será lançado.
        enemySprite.color = warningColor; // Mostra o aviso visual.
    }

    private bool CanShoot() // Verifica alcance e obstáculos.
    {
        Vector2 difference = player.position - transform.position; // Calcula a separação até o jogador.
        float distanceSquared = difference.sqrMagnitude; // Obtém a distância ao quadrado.

        if (distanceSquared < 0.001f) return false; // Evita um disparo sem direção válida.
        if (distanceSquared > shootingRange * shootingRange) return false; // Impede tiros fora do alcance.

        RaycastHit2D obstacle = Physics2D.Linecast(transform.position, player.position, obstacleLayers); // Procura uma parede entre os dois.
        return obstacle.collider == null; // Permite atirar somente quando o caminho está livre.
    }

    private void OnDisable() // Executa quando o inimigo é desativado.
    {
        if (enemySprite != null) enemySprite.color = originalColor; // Restaura a cor caso estivesse preparando um tiro.

        preparingShot = false; // Cancela qualquer preparação pendente.
    }
}