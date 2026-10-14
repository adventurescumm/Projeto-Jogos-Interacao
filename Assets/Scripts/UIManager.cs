using UnityEngine; // Disponibiliza MonoBehaviour, GameObject e Time.
using TMPro; // Disponibiliza os componentes de texto do TextMesh Pro.
using UnityEngine.UI;

public class UIManager : MonoBehaviour // Permite anexar o script a um objeto da cena.
{
    public GameObject endGamePanel; // Referência ao painel de encerramento existente.

    [SerializeField] // Mostra o campo privado abaixo no Inspector.
    private TMP_Text timerText; // Texto que mostra o cronômetro durante a partida.

    [SerializeField] // Permite associar este campo pelo Inspector.
    private TMP_Text resultText; // Texto que mostrará o resultado e o tempo final.
    
    [SerializeField]
    private TMP_Text scoreText;
    
    [SerializeField]
    private Button restartButton;

    [SerializeField] // Permite ajustar a duração pelo Inspector.
    private float duration = 60f; // Limite inicial de 60 segundos.

    private bool resultShown; // Começa falso; registra se já mostramos o resultado.

    private void Awake() // Inicializa este componente quando a cena é carregada.
    {
        GameController.Init(duration); // Reinicia coletáveis, cronômetro e limite.
        endGamePanel.SetActive(false); // Esconde o painel no início da partida.
        scoreText.text = $"Pontos: {GameController.score}";
    }

    private void Update() // A Unity chama este método a cada quadro.
    {
        if (resultShown) // Verifica se o resultado já foi apresentado.
        {
            return; // Encerra esta execução para preservar a tela final.
        }

        GameController.UpdateTime(Time.deltaTime); // Atualiza o cronômetro.
        
        scoreText.text = $"Pontos: {GameController.score}";

        // Subtrai o tempo gasto do limite e impede que o resultado fique negativo.
        float remainingTime = Mathf.Max(0f, GameController.timeLimit - GameController.elapsedTime);

        // Arredonda para cima: enquanto ainda existir uma fração de segundo, mostra 1.
        int totalSeconds = Mathf.CeilToInt(remainingTime);

        // Divide por 60; como os valores são inteiros, guarda apenas os minutos completos.
        int minutes = totalSeconds / 60;

        // O operador % obtém o resto da divisão: os segundos que sobraram.
        int seconds = totalSeconds % 60;

        // Mostra minutos e segundos com pelo menos dois dígitos, incluindo zeros à esquerda.
        timerText.text = $"{minutes:00}:{seconds:00}";

        if (!GameController.gameOver) // O ! significa "não": a partida ainda continua?
        {
            return; // Sai sem abrir o painel de encerramento.
        }

        // Escolhe a mensagem: condição ? valor se verdadeira : valor se falsa.
        string result = GameController.timeExpired ? "Tempo esgotado!" : "Você venceu!";

        // Exibe o resultado e o tempo congelado; \n cria uma quebra de linha.
        resultText.text = $"{result}\nTempo final: {GameController.elapsedTime:F2} s";

        endGamePanel.SetActive(true); // Mostra o painel com o resultado.
        restartButton.Select();
        resultShown = true; // Impede que esse encerramento seja repetido.
    }
}