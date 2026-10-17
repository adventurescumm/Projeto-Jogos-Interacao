using UnityEngine; // Disponibiliza os componentes e as cores da Unity.
using TMPro; // Disponibiliza os textos do TextMesh Pro.
using UnityEngine.UI; // Disponibiliza Image e Button.

public class UIManager : MonoBehaviour // Atualiza os elementos da interface.
{
    [SerializeField] private GameObject endGamePanel; // Guarda o painel de encerramento.
    [SerializeField] private TMP_Text broadcastText;
    [SerializeField] private TMP_Text resultText; // Guarda o texto do resultado.
    [SerializeField] private Button restartButton; // Guarda o botão selecionado ao terminar.

    [SerializeField] private TMP_Text scoreText; // Mostra a pontuação.
    [SerializeField] private TMP_Text ammoText; // Mostra a munição.
    [SerializeField] private TMP_Text waveText; // Mostra a onda.
    [SerializeField] private TMP_Text enemiesText; // Mostra os inimigos restantes.
    [SerializeField] private TMP_Text timerText; // Mostra o cronômetro.

    [SerializeField] private TMP_Text finalScoreText; // Recebe o texto da pontuação na tela de derrota.

    [SerializeField] private TMP_Text finalWaveText; // Recebe o texto da onda alcançada.

    [SerializeField] private TMP_Text finalTimeText; // Recebe o texto do tempo total na arena.

    [SerializeField] private TMP_Text tauntText; // Guarda o texto de provocação.
    [SerializeField, TextArea(2, 3)] private string deathMessage = "Você teve uma morte digna... de um animal!"; // Mensagem que você escreverá para morte.
    [SerializeField, TextArea(2, 3)] private string timeoutMessage = "Se você ficar parado que nem um idiota na frente do inimigo, é melhor se matar AGORA!"; // Mensagem que você escreverá para tempo esgotado.
    [SerializeField, Min(0f)] private float defeatDelay = 0.7f; // Deixa a explosão aparecer antes do painel.

    private float defeatDetectedAt = -1f; // Guarda quando a derrota foi percebida.

    [SerializeField] private Image[] lifeIcons = new Image[3]; // Guarda os três indicadores de vida.

    [SerializeField] private Color fullLifeColor = new Color(0.98f, 0.44f, 0.52f); // Cor das vidas disponíveis.
    [SerializeField] private Color emptyLifeColor = new Color(0.2f, 0.25f, 0.33f); // Cor das vidas perdidas.
    [SerializeField] private Color ammoColor = new Color(1f, 0.75f, 0.2f); // Cor normal da munição.

    [SerializeField] private PlayerDeathFeedback playerDeath;
    [SerializeField] private AudioSource arenaMusic;
    private bool resultShown; // Impede abrir o resultado repetidamente.

    private void Awake() // Prepara a interface.
    {
        if (endGamePanel != null) endGamePanel.SetActive(false);
        if (playerDeath == null) playerDeath = FindAnyObjectByType<PlayerDeathFeedback>(); // Esconde o painel final.
        resultShown = false; // Permite apresentar o resultado desta partida.
    }

    private void LateUpdate() // Atualiza a interface depois dos métodos Update.
    {
        scoreText.text = $"PONTOS {GameController.score:000000}"; // Mostra a pontuação com pelo menos seis dígitos.
        ammoText.text = $"MUNIÇÃO {GameController.ammo:000}"; // Mostra a munição com pelo menos três dígitos.
        waveText.text = $"SALA {GameController.wave:00}"; // Mostra o número da onda.
        enemiesText.text = $"INIMIGOS {GameController.enemiesRemaining:00}"; // Mostra quantos inimigos faltam.

        ammoText.color = GameController.ammo > 0 ? ammoColor : Color.red; // Destaca em vermelho quando não há munição.

        for (int i = 0; i < lifeIcons.Length; i++) // Percorre os indicadores de vida.
        {
            bool hasLife = i < GameController.health; // Verifica se essa posição representa uma vida disponível.
            if (lifeIcons[i] != null) lifeIcons[i].color = hasLife ? fullLifeColor : emptyLifeColor;
        }

        float displayedTime = GameController.timeLimitEnabled
            ? GameController.waveTimeRemaining // Mostra o prazo restante da onda quando há limite.
            : GameController.elapsedTime; // Mostra a duração da partida quando o limite está desativado.

        int totalSeconds = GameController.timeLimitEnabled // Escolhe como arredondar o tempo.
            ? Mathf.CeilToInt(displayedTime) // Arredonda para cima na contagem regressiva.
            : Mathf.FloorToInt(displayedTime); // Mostra segundos completos na contagem crescente.

        int minutes = totalSeconds / 60; // Calcula os minutos.
        int seconds = totalSeconds % 60; // Calcula os segundos restantes.

        timerText.text = $"{minutes:00}:{seconds:00}"; // Atualiza o cronômetro.

        bool timeWarning = GameController.timeLimitEnabled
                           && GameController.enemiesRemaining > 0
                           && GameController.waveTimeRemaining <= 5f; // Ativa o alerta nos últimos cinco segundos de combate.

        timerText.color = timeWarning
            ? new Color32(251, 113, 133, 255) // Usa FB7185 quando o prazo está acabando.
            : new Color32(248, 250, 252, 255); // Restaura F8FAFC no início da próxima onda.

        if (!GameController.gameOver || resultShown) return; // Aguarda o encerramento e impede repetir sua apresentação.

        if (arenaMusic != null && arenaMusic.isPlaying) arenaMusic.Stop();
        if (defeatDetectedAt < 0f) defeatDetectedAt = Time.unscaledTime; // Registra o primeiro instante da derrota.

        if (Time.unscaledTime - defeatDetectedAt < defeatDelay) return; // Aguarda a apresentação da explosão.

        if (!GameController.victory && playerDeath != null && !playerDeath.Finished) return;
        if (GameController.victory && WaveManager.Instance != null && !WaveManager.Instance.BossFeedbackFinished) return;
        tauntText.text = GameController.victory ? "A COROA É SUA. ATÉ A PRÓXIMA TRANSMISSÃO." : GameController.health <= 0 // Identifica o motivo da eliminação.
            ? deathMessage // Mostra a provocação por morte.
            : timeoutMessage; // Mostra a provocação por tempo esgotado.

        resultText.text = GameController.victory ? "REI DA ARENA" : GameController.health <= 0
            ? "ELIMINADO" // Mostra este título quando a vida termina.
            : "TEMPO ESGOTADO"; // Mostra este título quando o prazo termina.

        if (broadcastText != null) broadcastText.text = GameController.victory ? "CAMPEÃO" : "FORA DO AR";
        finalScoreText.text = $"{GameController.score:000000}"; // Mostra a pontuação com pelo menos seis dígitos.

        finalWaveText.text = $"{GameController.roomsCleared:00}"; // Mostra a onda alcançada com pelo menos dois dígitos.

        int finalTotalSeconds = Mathf.Max(0, Mathf.FloorToInt(GameController.elapsedTime)); // Converte a duração em segundos inteiros não negativos.

        int finalMinutes = finalTotalSeconds / 60; // Calcula os minutos completos.

        int finalSeconds = finalTotalSeconds % 60; // Calcula os segundos restantes.

        finalTimeText.text = $"{finalMinutes:00}:{finalSeconds:00}"; // Formata o tempo como 02:35.

        endGamePanel.transform.SetAsLastSibling(); // Coloca a tela de derrota acima dos outros elementos do Canvas.

        endGamePanel.SetActive(true); // Exibe o fundo escurecido e a placa de resultado.

        restartButton.Select(); // Seleciona Reiniciar para navegação por teclado ou controle.

        resultShown = true; // Registra que a tela de resultado já foi apresentada.
    }
}
