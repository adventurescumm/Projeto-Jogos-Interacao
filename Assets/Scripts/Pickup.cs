using UnityEngine; // Disponibiliza os componentes da Unity.

public enum PickupType // Define os tipos de item.
{
    Ammo, // Item de munição.
    Health // Item de vida.
}

public class Pickup : MonoBehaviour // Controla a coleta de um item.
{
    [SerializeField] private PickupType type; // Tipo escolhido no Inspector.
    [SerializeField] private int ammoAmount = 6; // Quantidade entregue por um pacote de munição.

    public bool IsAmmo => type == PickupType.Ammo; // Permite identificar os pacotes de munição.

    private bool collected; // Impede coletar o mesmo objeto mais de uma vez.

    private void OnTriggerEnter2D(Collider2D other) // Detecta a entrada de outro collider.
    {
        if (collected || GameController.gameOver) return; // Ignora itens consumidos e partidas encerradas.

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>(); // Procura o componente do jogador.

        if (player == null) return; // Ignora inimigos e outros objetos.

        if (type == PickupType.Ammo) // Verifica se deve entregar munição.
        {
            GameController.AddAmmo(ammoAmount); // Acrescenta os tiros disponíveis.
        }
        else // Trata o item como recuperação de vida.
        {
            if (!GameController.TryHeal()) return; // Preserva o item quando o jogador está com vida cheia.
        }

        collected = true; // Registra o consumo imediatamente.

        AudioSource sound = player.GetComponent<AudioSource>(); // Obtém o som de coleta já existente no jogador.

        if (sound != null) sound.Play(); // Reproduz o áudio quando houver um componente.

        gameObject.SetActive(false); // Desativa o visual e o collider.
        Destroy(gameObject); // Solicita a destruição definitiva.
    }
}