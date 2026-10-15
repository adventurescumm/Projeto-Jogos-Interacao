using UnityEngine; // Disponibiliza os componentes e as cores da Unity.
using TMPro; // Disponibiliza os textos do TextMesh Pro.
using UnityEngine.UI; // Disponibiliza Image e Button.

public class UIManager : MonoBehaviour // Atualiza os elementos da interface.
{
    [SerializeField] private GameObject endGamePanel; // Guarda o painel de encerramento.
    [SerializeField] private TMP_Text resultText; // Guarda o texto do resultado.
    [SerializeField] private Button restartButton; // Guarda o botão selecionado ao terminar.

    [SerializeField] private TMP_Text scoreText; // Mostra a pontuação.
    [SerializeField] private TMP_Text ammoText; // Mostra a munição.
    [SerializeField] private TMP_Text waveText; // Mostra a onda.
    [SerializeField] private TMP_Text enemiesText; // Mostra os inimigos restantes.
    [SerializeField] private TMP_Text timerText; // Mostra o cronômetro.

    [SerializeField] private Image[] lifeIcons = new Image[3]; // Guarda os três indicadores de vida.

    [SerializeField] private Color fullLifeColor = new Color(0.98f, 0.44f, 0.52f); // Cor das vidas disponíveis.
    [SerializeField] private Color emptyLifeColor = new Color(0.2f, 0.25f, 0.33f); // Cor das vidas perdidas.
    [SerializeField] private Color ammoColor = new Color(1f, 0.75f, 0.2f); // Cor normal da munição.

    private bool resultShown; // Impede abrir o resultado repetidamente.

    private void Awake() // Prepara a interface.
    {
        endGamePanel.SetActive(false); // Esconde o painel final.
        resultShown = false; // Permite apresentar o resultado desta partida.
    }

    private void LateUpdate() // Atualiza a interface depois dos métodos Update.
    {
        scoreText.text = $"PONTOS {GameController.score:000000}"; // Mostra a pontuação com pelo menos seis dígitos.
        ammoText.text = $"MUNIÇÃO {GameController.ammo:000}"; // Mostra a munição com pelo menos três dígitos.
        waveText.text = $"ONDA {GameController.wave:00}"; // Mostra o número da onda.
        enemiesText.text = $"INIMIGOS {GameController.enemiesRemaining:00}"; // Mostra quantos inimigos faltam.

        ammoText.color = GameController.ammo > 0 ? ammoColor : Color.red; // Destaca em vermelho quando não há munição.

        for (int i = 0; i < lifeIcons.Length; i++) // Percorre os indicadores de vida.
        {
            bool hasLife = i < GameController.health; // Verifica se essa posição representa uma vida disponível.
            lifeIcons[i].color = hasLife ? fullLifeColor : emptyLifeColor; // Acende ou apaga visualmente o indicador.
        }

        float displayedTime = GameController.elapsedTime; // Começa considerando o tempo decorrido.

        if (GameController.timeLimitEnabled) // Verifica se existe contagem regressiva.
        {
            displayedTime = Mathf.Max(0f, GameController.timeLimit - GameController.elapsedTime); // Calcula o tempo restante.
        }

        int totalSeconds = GameController.timeLimitEnabled // Escolhe como arredondar o tempo.
            ? Mathf.CeilToInt(displayedTime) // Arredonda para cima na contagem regressiva.
            : Mathf.FloorToInt(displayedTime); // Mostra segundos completos na contagem crescente.

        int minutes = totalSeconds / 60; // Calcula os minutos.
        int seconds = totalSeconds % 60; // Calcula os segundos restantes.

        timerText.text = $"{minutes:00}:{seconds:00}"; // Atualiza o cronômetro.

        if (!GameController.gameOver || resultShown) return; // Aguarda a derrota e impede repetir o resultado.

        string result = GameController.health <= 0 ? "Você perdeu!" : "Tempo esgotado!"; // Identifica o motivo do encerramento.

        resultText.text = // Monta a mensagem final.
            $"{result}\nPontos: {GameController.score}\n" + // Mostra o motivo e a pontuação.
            $"Onda: {GameController.wave}\nTempo: {GameController.elapsedTime:F1} s"; // Mostra onda e duração.

        endGamePanel.transform.SetAsLastSibling(); // Coloca o painel acima dos outros filhos do Canvas.
        endGamePanel.SetActive(true); // Mostra o painel final.
        restartButton.Select(); // Seleciona Reiniciar para uso pelo controle.
        resultShown = true; // Registra que o resultado já foi apresentado.
    }
}