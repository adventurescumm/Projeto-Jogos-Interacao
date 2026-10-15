using UnityEngine; // Disponibiliza os componentes e vetores da Unity.
using UnityEngine.InputSystem; // Disponibiliza InputAction e InputSystem.

[RequireComponent(typeof(Rigidbody2D))] // Exige um corpo físico no objeto.
public class PlayerMovement : MonoBehaviour // Controla o deslocamento do jogador.
{
    public float speed = 5f; // Mantém o campo de velocidade usado no seu projeto.

    private Rigidbody2D rb; // Guarda o corpo físico.
    private InputAction moveAction; // Guarda a ação de movimento.
    private Vector2 movement; // Guarda a entrada lida.

    private void Awake() // Obtém as referências antes das atualizações.
    {
        rb = GetComponent<Rigidbody2D>(); // Localiza o corpo físico.
        moveAction = InputSystem.actions.FindAction("Player/Move", true); // Localiza a ação obrigatória.
    }

    private void Update() // Lê a entrada a cada quadro.
    {
        movement = moveAction.ReadValue<Vector2>(); // Lê os controles associados a Move.
    }

    private void FixedUpdate() // Aplica o movimento junto da física.
    {
        if (GameController.gameOver) // Verifica se a partida terminou.
        {
            rb.linearVelocity = Vector2.zero; // Interrompe o deslocamento.
            return; // Não processa novos movimentos.
        }

        Vector2 direction = Vector2.ClampMagnitude(movement, 1f); // Limita a diagonal e preserva a intensidade do analógico.
        rb.linearVelocity = direction * speed; // Define a velocidade do corpo.
    }
}