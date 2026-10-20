using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Complementa o HUD existente, respeitando a referência de 600 × 480.
public class ArenaHUD : MonoBehaviour
{
    [SerializeField] private Sprite panelSprite;
    private TMP_FontAsset font;
    private TMP_Text roomText, keysText, announcement, bossText;
    private RectTransform bossFill;
    private GameObject bossPanel, mapPanel;
    private readonly Dictionary<string, Image> nodes = new Dictionary<string, Image>();
    private WaveManager manager;
    private bool builtMap;
    private readonly Color amber = new Color(1f, 0.75f, 0.2f);

    private void Start()
    {
        TMP_Text sample = GetComponentInChildren<TMP_Text>(true);
        font = sample != null ? sample.font : TMP_Settings.defaultFontAsset;
        Image room = Panel("SalaECredenciais", transform, new Vector2(0f, 0f), new Vector2(12f, 10f), new Vector2(245f, 43f));
        roomText = Label("Nome", room.transform, new Vector2(10f, 25f), new Vector2(225f, 16f), 12f, amber);
        keysText = Label("Credenciais", room.transform, new Vector2(10f, 5f), new Vector2(225f, 16f), 11f, Color.white);
        Image message = Panel("Anuncio", transform, new Vector2(0.5f, 1f), new Vector2(0f, -102f), new Vector2(520f, 25f));
        message.color = new Color(0.035f, 0.045f, 0.065f, 0.85f);
        announcement = Label("Texto", message.transform, new Vector2(8f, 4f), new Vector2(504f, 18f), 12f, amber);
        announcement.alignment = TextAlignmentOptions.Center;
        bossPanel = Panel("VidaDoBoss", transform, new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(320f, 26f)).gameObject;
        bossText = Label("Titulo", bossPanel.transform, new Vector2(8f, 10f), new Vector2(304f, 15f), 11f, Color.white);
        bossText.alignment = TextAlignmentOptions.Center;
        Image bar = Panel("Vida", bossPanel.transform, Vector2.zero, new Vector2(8f, 5f), new Vector2(304f, 4f));
        bar.sprite = null; bar.color = new Color(0.98f, 0.35f, 0.4f); bossFill = bar.rectTransform;
        bossPanel.SetActive(false);
    }
    private void Update()
    {
        if (roomText == null || announcement == null || bossPanel == null || keysText == null) return;
        if (manager == null) manager = WaveManager.Instance;
        if (manager == null || manager.CurrentArena == null) return;
        if (GameController.gameOver)
        {
            // O minimapa ampliado não deve ficar por cima do painel de resultado.
            roomText.transform.parent.gameObject.SetActive(false);
            announcement.transform.parent.gameObject.SetActive(false);
            if (mapPanel != null) mapPanel.SetActive(false);
            bossPanel.SetActive(false);
            return;
        }
        if (!builtMap) BuildMap();
        roomText.text = manager.CurrentArena.displayName.ToUpperInvariant();
        keysText.text = $"CREDENCIAIS   A [{(GameController.HasKey("A") ? "OK" : "--")}]   B [{(GameController.HasKey("B") ? "OK" : "--")}]";
        announcement.transform.parent.gameObject.SetActive(!GameController.gameOver && Time.time < manager.AnnouncementUntil);
        announcement.text = manager.Announcement;
        foreach (ArenaDefinition arena in manager.Arenas)
        {
            if (!nodes.TryGetValue(arena.id, out Image node) || node == null) continue;
            node.color = arena == manager.CurrentArena ? amber : manager.WasCleared(arena.id) ? new Color(0.2f, 0.65f, 0.5f)
                : !manager.HasRequiredKeys(arena.id) ? new Color(0.5f, 0.24f, 0.16f)
                : manager.WasVisited(arena.id) ? new Color(0.45f, 0.55f, 0.65f) : new Color(0.17f, 0.23f, 0.3f);
        }
        EnemyChaser boss = manager.CurrentBoss;
        bool visible = boss != null && !boss.Dead && !GameController.gameOver;
        bossPanel.SetActive(visible);
        if (visible)
        {
            bossText.text = $"{boss.DisplayName.ToUpperInvariant()}   {boss.Health}/{boss.MaxHealth}";
            bossFill.sizeDelta = new Vector2(304f * boss.HealthRatio, 4f);
        }
    }
    private void BuildMap()
    {
        builtMap = true;
        // Usa os limites do circuito e as portas reais, sem ligar todas as salas ao centro.
        Vector2Int min = manager.Arenas[0].mapPosition, max = min;
        Dictionary<string, Vector2> points = new Dictionary<string, Vector2>();
        foreach (ArenaDefinition arena in manager.Arenas)
        {
            min = Vector2Int.Min(min, arena.mapPosition);
            max = Vector2Int.Max(max, arena.mapPosition);
        }
        float stepX = Mathf.Min(24f, 144f / Mathf.Max(1, max.x - min.x));
        float stepY = Mathf.Min(22f, 66f / Mathf.Max(1, max.y - min.y));
        Vector2 size = new Vector2(Mathf.Max(112f, 36f + (max.x - min.x) * stepX), 42f + (max.y - min.y) * stepY);
        Image map = Panel("Mapa", transform, new Vector2(1f, 0f), new Vector2(-12f, 10f), size);
        mapPanel = map.gameObject;
        Label("Titulo", map.transform, new Vector2(8f, size.y - 17f), new Vector2(size.x - 16f, 12f), 9f, Color.white).text = "CIRCUITO AO VIVO";
        foreach (ArenaDefinition arena in manager.Arenas)
            points.Add(arena.id, new Vector2(18f + (arena.mapPosition.x - min.x) * stepX, 16f + (arena.mapPosition.y - min.y) * stepY));

        HashSet<string> links = new HashSet<string>();
        foreach (ArenaDefinition arena in manager.Arenas)
        for (int side = 0; side < 4; side++)
        {
            string target = arena.Exit(side);
            if (string.IsNullOrEmpty(target) || !points.ContainsKey(target)) continue;
            string link = string.CompareOrdinal(arena.id, target) < 0 ? arena.id + ":" + target : target + ":" + arena.id;
            if (links.Add(link))
            {
                Vector2 start = points[arena.id], end = points[target], delta = end - start;
                Image line = Panel("Ligacao_" + link, map.transform, Vector2.zero, (start + end) * 0.5f, new Vector2(delta.magnitude, 2f));
                line.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                line.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                line.color = new Color(0.4f, 0.45f, 0.5f); line.sprite = null;
            }
        }
        foreach (ArenaDefinition arena in manager.Arenas)
        {
            Image node = Panel(arena.id, map.transform, Vector2.zero, points[arena.id] - new Vector2(10f, 8f), new Vector2(20f, 16f));
            node.sprite = null;
            TMP_Text label = Label("Legenda", node.transform, Vector2.zero, new Vector2(20f, 16f), 9f, Color.white);
            label.alignment = TextAlignmentOptions.Center;
            label.text = !string.IsNullOrEmpty(arena.mapLabel) ? arena.mapLabel
                : arena.bossArena ? "F" : !string.IsNullOrEmpty(arena.rewardKey) ? arena.rewardKey : "·";
            nodes.Add(arena.id, node);
        }
    }
    private Image Panel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor; rect.pivot = anchor;
        rect.anchoredPosition = position; rect.sizeDelta = size;
        Image image = go.GetComponent<Image>(); image.sprite = panelSprite;
        image.type = Image.Type.Sliced; image.color = new Color(0.08f, 0.1f, 0.14f, 0.94f); image.raycastTarget = false;
        return image;
    }
    private TMP_Text Label(string name, Transform parent, Vector2 position, Vector2 size, float fontSize, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = position; rect.sizeDelta = size;
        TMP_Text label = go.GetComponent<TMP_Text>(); label.font = font; label.fontSize = fontSize;
        label.color = color; label.raycastTarget = false; label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }
}
