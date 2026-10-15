using UnityEngine; // Disponibiliza os componentes da Unity.
using TMPro; // Disponibiliza os textos do TextMesh Pro.
using UnityEngine.UI; // Disponibiliza Button.

public class UIManager : MonoBehaviour // Apresenta os dados da partida.
{
    public GameObject endGamePanel; // Mantém a referência ao painel existente.

    [SerializeField] private TMP_Text timerText; // Texto do cronômetro.
    [SerializeField] private TMP_Text resultText; // Texto do resultado final.
    [SerializeField] private TMP_Text scoreText; // Texto da pontuação.
    [SerializeField] private TMP_Text statusText; // Novo texto de vida, munição e onda.
    [SerializeField] private Button restartButton; // Botão selecionado no encerramento.

    private bool resultShown; // Impede apresentar o resultado repetidamente.

    private void Awake() // Prepara a interface.
    {
        endGamePanel.SetActive(false); // Esconde o painel de encerramento.
        resultShown = false; // Permite apresentar o resultado desta partida.
    }

    private void LateUpdate() // Atualiza a interface depois dos métodos Update.
    {
        scoreText.text = $"Pontos: {GameController.score}"; // Mostra os pontos por eliminações.

        statusText.text = // Monta as duas linhas de informações.
            $"Vida: {GameController.health}/{GameController.MaxHealth} | Munição: {GameController.ammo}\n" + // Mostra os recursos.
            $"Onda: {GameController.wave} | Inimigos: {GameController.enemiesRemaining}"; // Mostra a progressão.

        float displayedTime = GameController.elapsedTime; // Começa considerando o tempo decorrido.

        if (GameController.timeLimitEnabled) // Verifica se deve mostrar contagem regressiva.
        {
            displayedTime = Mathf.Max(0f, GameController.timeLimit - GameController.elapsedTime); // Calcula o tempo restante.
        }

        int totalSeconds = GameController.timeLimitEnabled // Escolhe o arredondamento adequado.
            ? Mathf.CeilToInt(displayedTime) // Arredonda para cima na contagem regressiva.
            : Mathf.FloorToInt(displayedTime); // Mostra segundos completos na contagem crescente.

        int minutes = totalSeconds / 60; // Calcula os minutos.
        int seconds = totalSeconds % 60; // Calcula os segundos restantes.

        timerText.text = $"{minutes:00}:{seconds:00}"; // Preserva a apresentação em minutos e segundos.

        if (!GameController.gameOver || resultShown) return; // Aguarda o encerramento e evita repetições.

        string result = GameController.health <= 0 ? "Você perdeu!" : "Tempo esgotado!"; // Identifica o motivo da derrota.

        resultText.text = // Monta a mensagem final.
            $"{result}\nPontos: {GameController.score}\n" + // Mostra o motivo e a pontuação.
            $"Onda: {GameController.wave}\nTempo: {GameController.elapsedTime:F1} s"; // Mostra onda e duração.

        endGamePanel.SetActive(true); // Abre o painel.
        restartButton.Select(); // Seleciona Reiniciar para navegação pelo controle.
        resultShown = true; // Registra que o resultado já foi mostrado.
    }
}