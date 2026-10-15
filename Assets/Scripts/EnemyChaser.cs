using UnityEngine; // Disponibiliza os componentes e vetores da Unity.

[RequireComponent(typeof(Rigidbody2D))] // Exige um corpo físico no inimigo.
public class EnemyChaser : MonoBehaviour // Implementa o inimigo perseguidor.
{
    private Rigidbody2D rb; // Guarda o corpo físico.
    private Transform player; // Guarda o alvo da perseguição.
    private float speed; // Guarda a velocidade desta onda.
    private bool dead; // Impede registrar a morte mais de uma vez.

    private void Awake() // Obtém o componente ao criar o inimigo.
    {
        rb = GetComponent<Rigidbody2D>(); // Localiza o corpo físico.
    }

    public void Init(Transform target, float movementSpeed) // Configura o inimigo recém-criado.
    {
        player = target; // Define o alvo da perseguição.
        speed = movementSpeed; // Define a velocidade recebida da onda.

        EnemyShooter shooter = GetComponent<EnemyShooter>(); // Procura o componente responsável pelos tiros.

        if (shooter != null) shooter.Init(target); // Entrega o mesmo jogador como alvo dos disparos.
    }

    private void FixedUpdate() // Atualiza a perseguição junto da física.
    {
        if (dead || GameController.gameOver || player == null) // Verifica se pode perseguir.
        {
            rb.linearVelocity = Vector2.zero; // Interrompe o movimento.
            return; // Sai antes de calcular a direção.
        }

        Vector2 direction = (Vector2)player.position - rb.position; // Calcula a direção até o jogador.
        rb.linearVelocity = direction.normalized * speed; // Persegue com velocidade constante.
    }

    private void OnCollisionEnter2D(Collision2D collision) // Detecta o início do contato.
    {
        DamagePlayer(collision); // Tenta causar dano.
    }

    private void OnCollisionStay2D(Collision2D collision) // Detecta a permanência do contato.
    {
        DamagePlayer(collision); // Permite outro dano depois do intervalo de proteção.
    }

    private void DamagePlayer(Collision2D collision) // Identifica o objeto em contato.
    {
        if (dead || GameController.gameOver) return; // Impede ataques após a morte ou o encerramento.

        PlayerMovement target = collision.gameObject.GetComponentInParent<PlayerMovement>(); // Procura o componente do jogador.

        if (target != null) GameController.TakeDamage(); // Causa dano somente ao jogador.
    }

    public void Die() // É chamado quando um tiro atinge o inimigo.
    {
        if (dead || GameController.gameOver) return; // Evita mortes duplicadas ou após a derrota.

        dead = true; // Registra a morte imediatamente.
        GameController.RegisterKill(); // Atualiza a quantidade de inimigos e os pontos.
        gameObject.SetActive(false); // Desativa o visual, o corpo e o collider.
        Destroy(gameObject); // Solicita a destruição definitiva.
    }
}