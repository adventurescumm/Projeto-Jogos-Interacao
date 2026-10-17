using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Uma única sobreposição permanece viva durante a troca de cena.
public class ScreenTransition : MonoBehaviour
{
    private static ScreenTransition instance;
    private Image shade;
    public static bool IsBusy => instance != null && instance.busy;
    private bool busy;
    private Action onFinished;
    public static bool Run(Action change, float fade = 0.18f, float hold = 0.05f, Action completed = null)
    {
        if (IsBusy) return false;
        if (instance == null)
        {
            GameObject root = new GameObject("Transicao", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(root);
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            instance = root.AddComponent<ScreenTransition>();
        }
        if (instance.shade == null)
        {
            GameObject panel = new GameObject("Escurecimento", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(instance.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            instance.shade = panel.GetComponent<Image>();
            instance.shade.color = new Color(0.025f, 0.035f, 0.055f, 0f);
        }
        instance.gameObject.SetActive(true);
        instance.enabled = true;
        instance.StartCoroutine(instance.Change(change, Mathf.Max(0.01f, fade), Mathf.Max(0f, hold), completed));
        return true;
    }
    private IEnumerator Change(Action change, float fade, float hold, Action completed)
    {
        busy = true;
        onFinished = completed;
        shade.raycastTarget = true;
        try
        {
            yield return Fade(0f, 1f, fade);
            if (hold > 0f) yield return new WaitForSecondsRealtime(hold);
            // Um erro de troca não deve deixar a sobreposição bloqueando a UI.
            try { change?.Invoke(); }
            catch (Exception error) { Debug.LogException(error); }
            yield return null;
            yield return Fade(1f, 0f, fade);
        }
        finally { Release(); }
    }
    private void Release()
    {
        busy = false;
        if (shade != null)
        {
            Color color = shade.color; color.a = 0f; shade.color = color;
            shade.raycastTarget = false;
        }
        // Retira a referência antes de chamar: cancelamento e destruição podem coincidir.
        Action callback = onFinished;
        onFinished = null;
        try { callback?.Invoke(); }
        catch (Exception error) { Debug.LogException(error); }
    }
    private IEnumerator Fade(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (shade == null) yield break; // A hierarquia pode ser removida pelo Editor durante a prévia.
            elapsed += Time.unscaledDeltaTime;
            Color color = shade.color; color.a = Mathf.Lerp(start, end, elapsed / duration); shade.color = color;
            yield return null;
        }
    }
    private void OnDisable() { StopAllCoroutines(); Release(); }
    private void OnDestroy() { Release(); if (instance == this) instance = null; }
}
