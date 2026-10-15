using UnityEngine; // Disponibiliza vetores e consultas de física.

public class Projectile : MonoBehaviour // Controla um projétil.
{
    [SerializeField] private float speed = 12f; // Velocidade do tiro.
    [SerializeField] private float radius = 0.05f; // Raio usado para detectar impactos.
    [SerializeField] private float lifetime = 3f; // Tempo máximo de existência.
    [SerializeField] private LayerMask hitLayers; // Layers que podem bloquear o projétil.

    private Vector2 direction; // Direção de deslocamento.
    private int createdInWave; // Onda em que o projétil foi criado.

    public void Init(Vector2 aimDirection) // Configura o tiro após sua criação.
    {
        direction = aimDirection.normalized; // Mantém a mesma velocidade em todas as direções.
        createdInWave = GameController.wave; // Registra a onda de origem.
        Destroy(gameObject, lifetime); // Agenda a destruição se nenhum impacto acontecer.
    }

    private void FixedUpdate() // Atualiza o projétil em intervalos da física.
    {
        if (GameController.gameOver || createdInWave != GameController.wave) // Detecta encerramento ou troca de onda.
        {
            Remove(); // Remove o projétil antigo.
            return; // Impede que ele atinja inimigos da nova onda.
        }

        float distance = speed * Time.fixedDeltaTime; // Calcula o deslocamento deste passo.

        RaycastHit2D hit = Physics2D.CircleCast( // Verifica um círculo ao longo do caminho.
            transform.position, // Usa a posição atual como origem.
            radius, // Define a espessura da detecção.
            direction, // Define a direção do teste.
            distance, // Limita o teste ao deslocamento deste passo.
            hitLayers); // Considera somente as layers configuradas.

        if (hit.collider != null) // Verifica se encontrou um objeto.
        {
            EnemyChaser enemy = hit.collider.GetComponentInParent<EnemyChaser>(); // Procura um inimigo no objeto atingido.

            if (enemy != null) enemy.Die(); // Elimina o inimigo, se houver.

            Remove(); // Consome o tiro também quando atingir uma parede.
            return; // Impede continuar avançando.
        }

        transform.position += (Vector3)(direction * distance); // Avança quando o caminho está livre.
    }

    private void Remove() // Encerra o funcionamento do tiro.
    {
        gameObject.SetActive(false); // Impede novas atualizações imediatamente.
        Destroy(gameObject); // Solicita a destruição.
    }
}