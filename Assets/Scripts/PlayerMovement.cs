using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    AudioSource audio;
    public float speed;
    private InputAction moveAction;
    private Vector2 movement;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audio = GetComponent<AudioSource>();
        moveAction = InputSystem.actions.FindAction("Player/Move", true);
    }

    // Update is called once per frame
    private void Update() // Lê a entrada a cada quadro.
    {
        if (GameController.gameOver) // Verifica se a partida terminou.
        {
            movement = Vector2.zero; // Apaga qualquer direção armazenada.
            return; // Sai antes de ler novos comandos.
        }

        // Lê os dois eixos da ação: X para horizontal e Y para vertical.
        movement = moveAction.ReadValue<Vector2>();
    }
    
    void FixedUpdate()
    {
        if (GameController.gameOver) // Verifica se a partida terminou.
        {
            rb.linearVelocity = Vector2.zero; // Zera a velocidade de deslocamento.
            rb.angularVelocity = 0f; // Zera uma possível velocidade de rotação.
            return; // Sai antes de ler os comandos e chamar MovePosition.
        }
        
        Vector2 direction = Vector2.ClampMagnitude(movement, 1f);
        
        Vector2 displacement = direction * speed * Time.fixedDeltaTime;
        
        rb.MovePosition(rb.position + displacement);
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameController.gameOver) // Verifica se a partida já terminou.
        {
            return; // Impede tocar o som, contar a coleta e destruir o item.
        }
        
        if (other.CompareTag("Coletavel"))
        {
            audio.Play();
            GameController.Collect();
            Destroy(other.gameObject);
        }
    }
}
