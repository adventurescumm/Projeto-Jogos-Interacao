using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Executa antes do combate para que Start congele o mesmo frame da entrada.
[DefaultExecutionOrder(-500)]
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Sprite panelSprite;
    [SerializeField] private AudioClip confirmSound;
    private GameObject overlay, optionsPanel, confirmationPanel;
    private Button returnButton, restartButton, cancelButton, confirmButton;
    private TMP_FontAsset font;
    private bool confirming, restarting, ownsPause;
    private int panelOpenedFrame;
    private readonly Color amber = new Color32(247, 185, 49, 255);

    private void Start()
    {
        TMP_Text sample = GetComponentInChildren<TMP_Text>(true);
        font = sample != null ? sample.font : TMP_Settings.defaultFontAsset;
        Build();
    }

    private void Update()
    {
        if (overlay == null || restarting) return;
        bool toggle = (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            || (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame);
        bool cancel = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
        if (!GameController.Paused)
        {
            if (toggle && !ScreenTransition.IsBusy && GameController.SetPaused(true))
            {
                ownsPause = true;
                overlay.SetActive(true);
                ShowOptions();
                Feedback();
            }
            return;
        }
        if (!ownsPause) return;
        if (toggle || cancel)
        {
            if (confirming) { ShowOptions(); Feedback(); }
            else Resume();
        }
    }

    public void Resume()
    {
        if (!ownsPause || restarting) return;
        Feedback();
        ClearSelection();
        overlay.SetActive(false);
        ownsPause = false;
        GameController.SetPaused(false);
    }

    private void ShowOptions()
    {
        confirming = false;
        ClearSelection();
        confirmationPanel.SetActive(false);
        optionsPanel.SetActive(true);
        panelOpenedFrame = Time.frameCount;
        returnButton.Select();
    }

    private void AskRestart()
    {
        if (!ownsPause || restarting || Time.frameCount <= panelOpenedFrame) return;
        Feedback();
        confirming = true;
        ClearSelection();
        optionsPanel.SetActive(false);
        confirmationPanel.SetActive(true);
        panelOpenedFrame = Time.frameCount;
        cancelButton.Select(); // A escolha inicial preserva a partida.
    }

    private void ConfirmRestart()
    {
        if (!ownsPause || !confirming || restarting || Time.frameCount <= panelOpenedFrame) return;
        Feedback();
        restarting = true;
        GameController.Transitioning = true;
        // O fade usa tempo real. A arena continua congelada até a nova cena inicializar.
        if (!ScreenTransition.Run(() => SceneManager.LoadScene("Arena01Estudio"), 0.25f, 0.08f, () =>
        {
            if (this == null) return;
            restarting = false;
            GameController.Transitioning = false;
            ShowOptions(); // Também permite recuperar o menu se o carregamento falhar.
        }))
        {
            restarting = false;
            GameController.Transitioning = false;
        }
    }

    private void Feedback() => GameAudio.Play(confirmSound, 0.5f, true);
    private void ClearSelection()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    private void Build()
    {
        // Canvas próprio: mantém a ordem da pausa sem alterar o menu nem o texto da história.
        overlay = new GameObject("Pausa", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20000;
        CanvasScaler scaler = overlay.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(640f, 480f);
        scaler.matchWidthOrHeight = 0.5f;
        Image shade = Panel("Fundo", overlay.transform, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.05f, 0.86f));
        shade.sprite = null;
        shade.rectTransform.anchorMin = Vector2.zero;
        shade.rectTransform.anchorMax = Vector2.one;
        shade.rectTransform.offsetMin = shade.rectTransform.offsetMax = Vector2.zero;
        optionsPanel = Panel("Opcoes", overlay.transform, Vector2.zero, new Vector2(392f, 300f), new Color32(17, 25, 35, 255)).gameObject;
        Label(optionsPanel.transform, "TRANSMISSÃO SUSPENSA", 111f, 14f, amber);
        Label(optionsPanel.transform, "PAUSA", 67f, 32f, Color.white);
        returnButton = MakeButton(optionsPanel.transform, "RETORNAR", 8f, Resume);
        restartButton = MakeButton(optionsPanel.transform, "REINICIAR", -50f, AskRestart);
        Label(optionsPanel.transform, "Start / Esc: retornar    A / Enter: escolher", -115f, 11f, Color.white);
        Link(returnButton, restartButton);
        confirmationPanel = Panel("Confirmacao", overlay.transform, Vector2.zero, new Vector2(392f, 300f), new Color32(17, 25, 35, 255)).gameObject;
        Label(confirmationPanel.transform, "REINICIAR A PARTIDA?", 94f, 23f, amber);
        Label(confirmationPanel.transform, "Quer mesmo reiniciar, seu covarde?", 42f, 15f, Color.white);
        cancelButton = MakeButton(confirmationPanel.transform, "CANCELAR", -23f, () => { Feedback(); ShowOptions(); });
        confirmButton = MakeButton(confirmationPanel.transform, "SIM, REINICIAR", -81f, ConfirmRestart);
        Link(cancelButton, confirmButton);
        overlay.SetActive(false);
    }

    private Image Panel(string objectName, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        image.sprite = panelSprite; image.type = Image.Type.Sliced; image.color = color;
        return image;
    }

    private TMP_Text Label(Transform parent, string text, float y, float size, Color color)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y); rect.sizeDelta = new Vector2(368f, 48f);
        TMP_Text label = go.GetComponent<TMP_Text>();
        label.font = font; label.fontSize = size; label.text = text;
        label.alignment = TextAlignmentOptions.Center; label.color = color; label.raycastTarget = false;
        return label;
    }

    private Button MakeButton(Transform parent, string text, float y, UnityEngine.Events.UnityAction action)
    {
        Image image = Panel(text, parent, new Vector2(0f, y), new Vector2(300f, 46f), Color.white);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color32(42, 52, 67, 255);
        colors.highlightedColor = new Color32(121, 83, 16, 255);
        colors.selectedColor = new Color32(121, 83, 16, 255);
        colors.pressedColor = amber; colors.fadeDuration = 0.08f;
        button.colors = colors;
        TMP_Text label = Label(image.transform, text, 0f, 19f, Color.white);
        label.rectTransform.sizeDelta = new Vector2(288f, 44f);
        button.onClick.AddListener(action);
        return button;
    }

    private static void Link(Button first, Button second)
    {
        Navigation nav = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = second, selectOnDown = second };
        first.navigation = nav;
        nav.selectOnUp = nav.selectOnDown = first; second.navigation = nav;
    }

    private void OnDisable()
    {
        if (ownsPause)
        {
            ownsPause = false;
            GameController.SetPaused(false);
            GameController.Transitioning = false;
        }
        if (overlay != null) overlay.SetActive(false);
    }
    private void OnDestroy()
    {
        if (overlay != null) Destroy(overlay);
    }
}
