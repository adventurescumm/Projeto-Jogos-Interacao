using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerDamageFeedback : MonoBehaviour
{
    private SpriteRenderer image;
    private Color normal;
    private void Awake() { image = GetComponent<SpriteRenderer>(); normal = image.color; }
    private void LateUpdate()
    {
        Color color = normal;
        if (GameController.Invulnerable && !GameController.gameOver)
            color.a = Mathf.FloorToInt(Time.time * 14f) % 2 == 0 ? 0.28f : 1f;
        image.color = color;
    }
    private void OnDisable() { if (image != null) image.color = normal; }
}
