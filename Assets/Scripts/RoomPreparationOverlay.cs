using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// É criado automaticamente nas cenas que têm WaveManager; não exige ligação no Inspector.
public class RoomPreparationOverlay : MonoBehaviour
{
    private GameObject panel;
    private TMP_Text label;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= Attach;
        SceneManager.sceneLoaded += Attach;
        Attach(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    private static void Attach(Scene scene, LoadSceneMode mode)
    {
        WaveManager manager = Object.FindAnyObjectByType<WaveManager>();
        if (manager == null || manager.gameObject.scene != scene) return;
        if (Object.FindAnyObjectByType<RoomPreparationOverlay>() != null) return;
        GameObject root = new GameObject("PreparacaoDaSala", typeof(RectTransform), typeof(Canvas));
        SceneManager.MoveGameObjectToScene(root, scene);
        UIManager ui = Object.FindAnyObjectByType<UIManager>();
        if (ui != null && ui.gameObject.scene == scene)
        {
            // Usa a mesma escala do HUD, inclusive em tela cheia.
            root.transform.SetParent(ui.transform, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        root.AddComponent<RoomPreparationOverlay>();
    }
    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas.isRootCanvas)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(600f, 480f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000; // Abaixo da pausa; oculto durante transições.
        gameObject.layer = LayerMask.NameToLayer("UI");
        UIManager ui = Object.FindAnyObjectByType<UIManager>();
        TMP_Text sample = ui != null ? ui.GetComponentInChildren<TMP_Text>(true) : null;
        TMP_FontAsset font = sample != null ? sample.font : TMP_Settings.defaultFontAsset;

        panel = new GameObject("Aviso", typeof(RectTransform), typeof(Image));
        panel.layer = gameObject.layer;
        panel.transform.SetParent(transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -70f);
        rect.sizeDelta = new Vector2(360f, 30f);
        Image background = panel.GetComponent<Image>();
        background.color = new Color32(17, 25, 35, 255);
        background.raycastTarget = false;

        GameObject text = new GameObject("GetReady", typeof(RectTransform), typeof(TextMeshProUGUI));
        text.layer = gameObject.layer;
        text.transform.SetParent(panel.transform, false);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        label = text.GetComponent<TMP_Text>();
        label.font = font;
        label.text = "Get Ready!";
        label.fontSize = 24f;
        label.color = new Color32(247, 185, 49, 255);
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        panel.SetActive(false);
    }
    private void LateUpdate()
    {
        bool visible = GameController.Preparing && !GameController.gameOver
            && !GameController.Transitioning && !ScreenTransition.IsBusy;
        panel.SetActive(visible);
        if (!visible) return;
        float elapsed = GameController.RoomPreparationDuration - GameController.PreparationTimeRemaining;
        // O piscar também fica congelado quando o jogador pausa.
        label.alpha = Mathf.FloorToInt(elapsed / 0.25f) % 2 == 0 ? 1f : 0f;
    }
}
