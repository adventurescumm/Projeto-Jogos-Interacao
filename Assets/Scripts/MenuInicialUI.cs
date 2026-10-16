using UnityEngine; // Permite acessar GameObjects, Canvas e MonoBehaviour.
using UnityEngine.UI; // Permite acessar Button e ScrollRect.

public class MenuInicialUI : MonoBehaviour // Controla as telas do menu.
{
    [SerializeField] private GameObject painelPrincipal; // Tela inicial.
    [SerializeField] private GameObject painelHistoria; // Tela da história.
    [SerializeField] private GameObject painelInstrucoes; // Tela de instruções.

    [SerializeField] private Button botaoJogar; // Seleção inicial do menu.
    [SerializeField] private Button botaoHistoria; // Botão que abre a história.
    [SerializeField] private Button botaoInstrucoes; // Botão que abre as instruções.

    [SerializeField] private Button voltarHistoria; // Botão de retorno da história.
    [SerializeField] private Button voltarInstrucoes; // Botão de retorno das instruções.

    [SerializeField] private ScrollRect rolagemHistoria; // Área rolável da história.

    private Button botaoDeRetorno; // Guarda qual botão selecionar ao voltar.

    private void Start() // Executa quando a cena inicia.
    {
        botaoDeRetorno = botaoJogar; // Define a seleção de retorno inicial.
        Mostrar(painelPrincipal, botaoJogar); // Abre a tela inicial.
    }

    public void AbrirHistoria() // Será chamado pelo botão História.
    {
        botaoDeRetorno = botaoHistoria; // Lembra de onde o jogador veio.
        Mostrar(painelHistoria, voltarHistoria); // Exibe a história e seleciona Voltar.

        Canvas.ForceUpdateCanvases(); // Atualiza as dimensões do texto e da interface.
        rolagemHistoria.StopMovement(); // Interrompe qualquer rolagem anterior.
        rolagemHistoria.verticalNormalizedPosition = 1f; // Volta ao começo do texto.
    }

    public void AbrirInstrucoes() // Será chamado pelo botão Instruções.
    {
        botaoDeRetorno = botaoInstrucoes; // Lembra de onde o jogador veio.
        Mostrar(painelInstrucoes, voltarInstrucoes); // Exibe as instruções.
    }

    public void Voltar() // Será chamado pelos dois botões Voltar.
    {
        Mostrar(painelPrincipal, botaoDeRetorno); // Retorna ao menu e restaura a seleção.
    }

    private void Mostrar(GameObject painel, Button primeiroBotao) // Centraliza a troca de tela.
    {
        painelPrincipal.SetActive(painel == painelPrincipal); // Ativa apenas se for a tela escolhida.
        painelHistoria.SetActive(painel == painelHistoria); // Faz a mesma verificação para a história.
        painelInstrucoes.SetActive(painel == painelInstrucoes); // Faz a verificação para as instruções.

        primeiroBotao.Select(); // Seleciona o botão para navegação por teclado ou controle.
    }
}