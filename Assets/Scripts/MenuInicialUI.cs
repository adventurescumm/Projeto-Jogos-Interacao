using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuInicialUI : MonoBehaviour
{
    [SerializeField] private GameObject painelPrincipal;
    [SerializeField] private GameObject painelHistoria;
    [SerializeField] private GameObject painelInstrucoes;
    [SerializeField] private Button botaoJogar;
    [SerializeField] private Button botaoHistoria;
    [SerializeField] private Button botaoInstrucoes;
    [SerializeField] private Button voltarHistoria;
    [SerializeField] private Button voltarInstrucoes;
    [SerializeField] private ScrollRect rolagemHistoria;
    private Button botaoDeRetorno;
    private MatchSetupMenu setup;

    private void Awake()
    {
        setup = gameObject.AddComponent<MatchSetupMenu>();
        setup.Build(painelPrincipal, botaoJogar, GetComponent<MenuActions>());
        RemoverRolagemHistoria();
    }
    private void Start()
    {
        botaoDeRetorno = botaoJogar;
        Mostrar(painelPrincipal, botaoJogar);
    }
    private void Update()
    {
        if (setup.IsOpen || ScreenTransition.IsBusy || painelPrincipal.activeSelf) return;
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null && !group.interactable) return;
        Gamepad pad = Gamepad.current;
        if ((pad != null && pad.buttonEast.wasPressedThisFrame)
            || (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)) Voltar();
    }
    public void AbrirHistoria()
    {
        botaoDeRetorno = botaoHistoria;
        ScreenTransition.Run(() => Mostrar(painelHistoria, voltarHistoria));
    }
    public void AbrirInstrucoes()
    {
        botaoDeRetorno = botaoInstrucoes;
        ScreenTransition.Run(() => Mostrar(painelInstrucoes, voltarInstrucoes));
    }
    public void Voltar() => ScreenTransition.Run(() => Mostrar(painelPrincipal, botaoDeRetorno));

    private void RemoverRolagemHistoria()
    {
        if (painelHistoria != null)
            foreach (Scrollbar bar in painelHistoria.GetComponentsInChildren<Scrollbar>(true))
                bar.gameObject.SetActive(false);
        if (rolagemHistoria == null) return;
        rolagemHistoria.StopMovement();
        rolagemHistoria.enabled = false;
        rolagemHistoria.horizontal = rolagemHistoria.vertical = false;
        if (rolagemHistoria.horizontalScrollbar != null) rolagemHistoria.horizontalScrollbar.gameObject.SetActive(false);
        if (rolagemHistoria.verticalScrollbar != null) rolagemHistoria.verticalScrollbar.gameObject.SetActive(false);
        rolagemHistoria.horizontalScrollbar = rolagemHistoria.verticalScrollbar = null;
        // Libera também a faixa que antes era reservada para a barra.
        RectTransform viewport = rolagemHistoria.viewport;
        if (viewport != null)
        {
            viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
        }
        AlinharHistoriaNoTopo();
    }
    private void AlinharHistoriaNoTopo()
    {
        if (rolagemHistoria == null || rolagemHistoria.content == null) return;
        RectTransform content = rolagemHistoria.content;
        // Não modifica o texto nem o tamanho de fonte da história.
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);
    }
    private void Mostrar(GameObject painel, Button primeiroBotao)
    {
        painelPrincipal.SetActive(painel == painelPrincipal);
        painelHistoria.SetActive(painel == painelHistoria);
        painelInstrucoes.SetActive(painel == painelInstrucoes);
        if (painel == painelHistoria)
        {
            Canvas.ForceUpdateCanvases();
            AlinharHistoriaNoTopo();
        }
        primeiroBotao.Select();
    }
}
