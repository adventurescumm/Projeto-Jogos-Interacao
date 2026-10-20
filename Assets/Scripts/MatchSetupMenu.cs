using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Monta as opções com a fonte e o sprite que já pertencem ao menu.
public class MatchSetupMenu : MonoBehaviour
{
    private GameObject main, difficultyPanel, controllerPanel;
    private Button play, controls, backDifficulty, backControls;
    private readonly Button[] levels = new Button[3];
    private readonly Button[] settings = new Button[3];
    private TMP_Text description;
    private TMP_FontAsset font;
    private Sprite sprite;
    private MenuActions actions;
    private CanvasGroup group;
    private ButtonPulse playPulse;
    private Transform content;
    private int openedFrame;
    private bool starting;
    public bool IsOpen => difficultyPanel != null && (difficultyPanel.activeSelf || controllerPanel.activeSelf);
    private static readonly Color Amber = new Color32(247, 185, 49, 255);

    public void Build(GameObject mainPanel, Button playButton, MenuActions menuActions)
    {
        main = mainPanel; play = playButton; actions = menuActions;
        content = main.transform.parent;
        group = GetComponent<CanvasGroup>();
        playPulse = play.GetComponent<ButtonPulse>() ?? play.gameObject.AddComponent<ButtonPulse>();
        playPulse.ConfirmationStarted += actions.PrepararNarracao;
        font = play.GetComponentInChildren<TMP_Text>(true).font;
        sprite = play.targetGraphic is Image image ? image.sprite : null;
        controls = MakeButton(main.transform, "CONTROLE", 200, 394, 240, OpenControls);
        difficultyPanel = MakePanel("PanelDificuldade");
        Label(difficultyPanel.transform, "ESCOLHA A DIFICULDADE", 48, 43, 544, 44, 26, Amber);
        description = Label(difficultyPanel.transform, "", 236, 110, 350, 200, 17, Color.white);
        for (int i = 0; i < 3; i++)
        {
            DifficultyLevel level = (DifficultyLevel)i;
            levels[i] = MakeButton(difficultyPanel.transform, GameDifficulty.Name(level), 48, 112 + 64 * i, 164, () => StartMatch(level));
            EventTrigger trigger = levels[i].gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry select = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            // Navegar/posicionar o cursor só mostra a descrição; clicar/A inicia.
            select.callback.AddListener(_ => PreviewDifficulty(level));
            trigger.triggers.Add(select);
        }
        Label(difficultyPanel.transform, "Todos os modos: 3 vidas, chaves recuperáveis e reserva de munição. O tempo continua correndo durante a reposição.", 48, 320, 544, 49, 14, Color.white);
        backDifficulty = MakeButton(difficultyPanel.transform, "VOLTAR", 192, 382, 256, Close);
        Label(difficultyPanel.transform, "Direcional: ver opções    Clique / A: jogar    B / Esc: voltar", 48, 438, 544, 24, 12, Color.white);
        LinkVertical(new[] { levels[0], levels[1], levels[2], backDifficulty });
        foreach (Button level in levels)
        {
            Navigation nav = level.navigation; nav.selectOnRight = backDifficulty; level.navigation = nav;
        }
        Navigation backNav = backDifficulty.navigation; backNav.selectOnLeft = levels[(int)GameDifficulty.Selected]; backDifficulty.navigation = backNav;

        controllerPanel = MakePanel("PanelControle");
        Label(controllerPanel.transform, "AJUSTES DO CONTROLE", 48, 43, 544, 44, 26, Amber);
        settings[0] = MakeButton(controllerPanel.transform, "", 48, 104, 544, () => { ControllerSettings.CycleDeadzone(); RefreshSettings(); });
        settings[1] = MakeButton(controllerPanel.transform, "", 48, 164, 544, () => { ControllerSettings.CycleStyle(); RefreshSettings(); });
        settings[2] = MakeButton(controllerPanel.transform, "", 48, 224, 544, () => { ControllerSettings.ToggleAssist(); RefreshSettings(); });
        Label(controllerPanel.transform, "Zona morta: aumente se a mira se mover sozinha.\nDireta: resposta imediata. Leve / Suave: suavização crescente.\nAssistência: pequena correção perto de um inimigo visível.\nRT ou RB: atirar · Start: pausar · B: voltar", 48, 284, 544, 90, 14, Color.white);
        backControls = MakeButton(controllerPanel.transform, "VOLTAR", 192, 390, 256, Close);
        LinkVertical(new[] { settings[0], settings[1], settings[2], backControls });
        Button[] mainButtons = main.GetComponentsInChildren<Button>(true);
        System.Array.Sort(mainButtons, (a, b) => ((RectTransform)b.transform).anchoredPosition.y.CompareTo(((RectTransform)a.transform).anchoredPosition.y));
        LinkVertical(mainButtons);
        RefreshSettings();
        difficultyPanel.SetActive(false); controllerPanel.SetActive(false);
    }
    private void Update()
    {
        if (!IsOpen || starting || ScreenTransition.IsBusy || Time.frameCount <= openedFrame || (group != null && !group.interactable)) return;
        bool cancel = (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            || (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame);
        if (cancel) Close();
    }
    public void OpenDifficulty()
    {
        if (starting || IsOpen) return;
        PreviewDifficulty(GameDifficulty.Selected);
        Open(difficultyPanel, levels[(int)GameDifficulty.Selected]);
    }
    private void OpenControls() => Open(controllerPanel, settings[0]);
    private void Open(GameObject panel, Button selected)
    {
        if (ScreenTransition.IsBusy) return;
        ScreenTransition.Run(() =>
        {
            main.SetActive(false); panel.SetActive(true);
            openedFrame = Time.frameCount;
            selected.Select();
        });
    }
    private void PreviewDifficulty(DifficultyLevel level)
    {
        if (starting) return;
        description.text = GameDifficulty.Description(level);
        for (int i = 0; i < levels.Length; i++)
            if (levels[i] != null) levels[i].GetComponentInChildren<TMP_Text>().text = GameDifficulty.Name((DifficultyLevel)i) + (i == (int)level ? " •" : "");
        Navigation navigation = backDifficulty.navigation;
        navigation.selectOnLeft = levels[(int)level];
        backDifficulty.navigation = navigation;
    }
    private void Close()
    {
        if (starting || ScreenTransition.IsBusy) return;
        bool wasControls = controllerPanel.activeSelf;
        ScreenTransition.Run(() =>
        {
            difficultyPanel.SetActive(false); controllerPanel.SetActive(false); main.SetActive(true);
            (wasControls ? controls : play).Select();
        });
    }
    private void StartMatch(DifficultyLevel level)
    {
        if (starting || ScreenTransition.IsBusy) return;
        starting = true;
        // Só a confirmação grava a escolha e inicia o jogo; focar um botão não inicia.
        GameDifficulty.Select(level);
        // O narrador já tocou antes desta tela. Segue direto para o fade.
        actions.ComecarPartida();
    }
    private void OnDestroy()
    {
        if (playPulse != null && actions != null)
            playPulse.ConfirmationStarted -= actions.PrepararNarracao;
    }
    private void RefreshSettings()
    {
        settings[0].GetComponentInChildren<TMP_Text>().text = "ZONA MORTA: " + Mathf.RoundToInt(ControllerSettings.Deadzone * 100f) + "%";
        settings[1].GetComponentInChildren<TMP_Text>().text = "RESPOSTA DA MIRA: " + ControllerSettings.StyleName;
        settings[2].GetComponentInChildren<TMP_Text>().text = "ASSISTÊNCIA DE MIRA: " + (ControllerSettings.AimAssist ? "LIGADA" : "DESLIGADA");
    }
    private GameObject MakePanel(string name)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(content, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return panel;
    }
    private static RectTransform Rect(GameObject go, Transform parent, float x, float y, float w, float h)
    {
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
        return rect;
    }
    private TMP_Text Label(Transform parent, string text, float x, float y, float w, float h, float size, Color color)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        Rect(go, parent, x, y, w, h);
        TMP_Text label = go.GetComponent<TMP_Text>();
        label.font = font; label.text = text; label.fontSize = size; label.color = color;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.alignment = TextAlignmentOptions.TopLeft; label.raycastTarget = false;
        return label;
    }
    private Button MakeButton(Transform parent, string text, float x, float y, float w, UnityAction action)
    {
        GameObject go = new GameObject(text.Length > 0 ? text : "Ajuste", typeof(RectTransform), typeof(Image), typeof(Button));
        Rect(go, parent, x, y, w, 44f);
        Image image = go.GetComponent<Image>(); image.sprite = sprite; image.type = Image.Type.Sliced;
        Button button = go.GetComponent<Button>(); button.targetGraphic = image;
        ColorBlock colors = play.colors; button.colors = colors;
        TMP_Text label = Label(go.transform, text, 8, 0, w - 16, 44, w < 200 ? 17 : 19, Color.black);
        label.alignment = TextAlignmentOptions.Center;
        button.onClick.AddListener(action);
        return button;
    }
    private static void LinkVertical(Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[(i + buttons.Length - 1) % buttons.Length],
                selectOnDown = buttons[(i + 1) % buttons.Length]
            };
        }
    }
}
