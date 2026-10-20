using UnityEngine; // Disponibiliza os componentes, vetores e consultas de física.

public class EnemyProjectile : MonoBehaviour // Controla um tiro disparado pelo inimigo.
{
    [SerializeField] private float speed = 3.5f; // Define a velocidade do projétil.
    [SerializeField] private float radius = 0.08f; // Define o raio da detecção de impacto.
    [SerializeField] private float lifetime = 4f; // Define o tempo máximo de existência.
    [SerializeField] private LayerMask hitLayers; // Define quais layers podem ser atingidas.
    [SerializeField] private float spriteAngleOffset = -90f;

    private Vector2 direction; // Guarda a direção escolhida no momento do disparo.
    private int createdInWave; // Guarda a onda em que o tiro foi criado.

    public void Init(Vector2 shotDirection) // Configura o projétil recém-criado.
    {
        direction = shotDirection.normalized; // Garante a mesma velocidade em qualquer direção.
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spriteAngleOffset);
        createdInWave = GameController.EncounterId; // Registra a onda atual.
        Destroy(gameObject, lifetime); // Agenda a remoção caso nenhum impacto aconteça.
    }

    private void FixedUpdate() // Atualiza o tiro nos passos da física.
    {
        if (GameController.Paused) return;
        if (!GameController.CanControl || !GameController.CombatActive || createdInWave != GameController.EncounterId) // Verifica encerramento ou troca de onda.
        {
            Remove(); // Remove tiros que não pertencem mais ao combate atual.
            return; // Impede novos impactos.
        }

        float distance = speed * GameDifficulty.ProjectileSpeed(GameController.Difficulty) * Time.fixedDeltaTime;

        RaycastHit2D hit = Physics2D.CircleCast( // Verifica todo o caminho até a próxima posição.
            transform.position, // Usa a posição atual como origem.
            radius, // Define a espessura da detecção.
            direction, // Usa a direção fixada no disparo.
            distance, // Limita o teste ao deslocamento deste passo.
            hitLayers); // Considera somente as layers selecionadas.

        if (hit.collider != null) // Verifica se encontrou um objeto.
        {
            PlayerMovement player = hit.collider.GetComponentInParent<PlayerMovement>(); // Procura o jogador no objeto atingido.

            if (player != null) GameController.TakeDamage(); // Tenta retirar uma vida do jogador.

            Remove(); // Consome o tiro ao atingir o jogador ou uma parede.
            return; // Impede continuar avançando depois do impacto.
        }

        transform.position += (Vector3)(direction * distance); // Avança pelo caminho livre.
    }

    private void Remove() // Encerra o funcionamento do projétil.
    {
        gameObject.SetActive(false); // Interrompe suas atualizações imediatamente.
        Destroy(gameObject); // Solicita a destruição definitiva.
    }
}
