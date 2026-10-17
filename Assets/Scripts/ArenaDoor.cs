using UnityEngine;

public class ArenaDoor : MonoBehaviour
{
    private WaveManager manager;
    private string destination;
    private int side;
    private BoxCollider2D blocker;
    private SpriteRenderer panel;
    private SpriteRenderer lightStrip;
    private float nextHint;

    public void Configure(WaveManager owner, string target, int direction, BoxCollider2D solid, SpriteRenderer sprite, SpriteRenderer light)
    {
        manager = owner; destination = target; side = direction; blocker = solid; panel = sprite; lightStrip = light;
    }

    private void Update()
    {
        if (manager == null) return;
        bool cleared = manager.CurrentCleared;
        bool unlocked = manager.HasRequiredKeys(destination);
        bool open = cleared && unlocked && !GameController.gameOver;
        blocker.enabled = !open;
        panel.enabled = !open;
        lightStrip.color = open ? new Color(0.2f, 0.95f, 0.7f) : cleared ? new Color(1f, 0.75f, 0.2f) : new Color(0.95f, 0.2f, 0.25f);
    }

    private void OnTriggerEnter2D(Collider2D other) => TryPass(other);
    private void OnTriggerStay2D(Collider2D other) => TryPass(other);
    private void TryPass(Collider2D other)
    {
        if (manager == null || !GameController.CanControl || other.GetComponentInParent<PlayerMovement>() == null) return;
        if (manager.CanEnter(destination)) manager.TryEnter(destination, (side + 2) % 4);
        else if (Time.time >= nextHint)
        {
            nextHint = Time.time + 2f;
            manager.Notify(manager.EntryHint(destination), 1.6f);
        }
    }
}
