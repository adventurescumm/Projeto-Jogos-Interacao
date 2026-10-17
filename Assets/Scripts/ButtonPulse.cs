using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonPulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CanvasGroup glow;
    [SerializeField] private CanvasGroup menuGroup;
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private AudioClip confirmSound;
    [SerializeField] private bool pulseAll = false;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.08f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.65f;
    [SerializeField, Min(0.1f)] private float pulseSpeed = 1.3f;
    [SerializeField, Min(0.01f)] private float flashDuration = 0.24f;
    [SerializeField] private UnityEvent onConfirmed = new UnityEvent();
    private Button button;
    private Image glowImage;
    private readonly Color normalColor = new Color(1f, 0.75f, 0.2f, 0.65f);
    private bool pointerInside, confirming, ready;

    private void Awake() => Prepare();
    private void Prepare()
    {
        if (ready) return;
        ready = true; button = GetComponent<Button>();
        Canvas canvas = GetComponentInParent<Canvas>();
        if (menuGroup == null && canvas != null)
            menuGroup = canvas.GetComponent<CanvasGroup>() ?? canvas.gameObject.AddComponent<CanvasGroup>();
        // Cada botão controla exclusivamente seu próprio brilho, sem referências cruzadas.
        if (glow != null && glow.transform.IsChildOf(transform)) glow.gameObject.SetActive(false);
        GameObject child = new GameObject("BrilhoInterativo", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        child.transform.SetParent(transform, false);
        child.transform.SetAsFirstSibling();
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(3f, 3f); rect.offsetMax = new Vector2(-3f, -3f);
        glow = child.GetComponent<CanvasGroup>(); glow.blocksRaycasts = false; glow.alpha = 0f;
        glowImage = child.GetComponent<Image>(); glowImage.color = normalColor; glowImage.raycastTarget = false;
    }
    public void Configure(AudioClip clip, AudioSource source)
    {
        Prepare(); confirmSound = clip; uiAudio = source;
        minAlpha = 0.08f; maxAlpha = 0.65f; flashDuration = 0.24f;
        bool wired = false;
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            if (button.onClick.GetPersistentTarget(i) == this && button.onClick.GetPersistentMethodName(i) == nameof(Confirmar)) wired = true;
        if (!wired)
        {
            onConfirmed = button.onClick;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(Confirmar);
        }
    }
    private void Update()
    {
        if (confirming || !ready || button == null || glow == null) return;
        bool selected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
        bool active = button.IsInteractable() && (pulseAll || pointerInside || selected);
        float wave = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * pulseSpeed) + 1f) * 0.5f;
        glow.alpha = active ? Mathf.Lerp(minAlpha, maxAlpha, wave) : 0f;
    }
    public void Confirmar()
    {
        Prepare();
        if (!isActiveAndEnabled || confirming || ScreenTransition.IsBusy || button == null || glow == null || glowImage == null || !button.IsInteractable()) return;
        confirming = true;
        if (menuGroup != null) menuGroup.interactable = false;
        StartCoroutine(Confirmacao());
    }
    private IEnumerator Confirmacao()
    {
        AudioSource feedback = GameAudio.Play(confirmSound, 0.6f, true);
        glowImage.color = Color.white;
        for (int i = 0; i < 2; i++)
        {
            if (glow != null) glow.alpha = 1f;
            yield return new WaitForSecondsRealtime(flashDuration / 4f);
            if (glow != null) glow.alpha = 0f;
            yield return new WaitForSecondsRealtime(flashDuration / 4f);
        }
        if (glowImage != null) glowImage.color = normalColor;
        if (glow != null) glow.alpha = minAlpha;
        while (feedback != null && feedback.isPlaying) yield return null;
        if (menuGroup != null) menuGroup.interactable = true;
        confirming = false;
        onConfirmed.Invoke();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        if (button.IsInteractable()) button.Select();
    }
    public void OnPointerExit(PointerEventData eventData) { pointerInside = false; }
    private void OnDisable()
    {
        StopAllCoroutines();
        if (confirming && menuGroup != null) menuGroup.interactable = true;
        confirming = pointerInside = false;
        if (glowImage != null) glowImage.color = normalColor;
        if (glow != null && ready) glow.alpha = 0f;
    }
}
