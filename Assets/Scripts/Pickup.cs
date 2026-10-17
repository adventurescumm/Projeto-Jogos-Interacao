using UnityEngine;

public enum PickupType { Ammo, Health, Treasure, Key }

public class Pickup : MonoBehaviour
{
    [SerializeField] private PickupType type;
    [SerializeField, Min(1)] private int ammoAmount = 8;
    [SerializeField, Min(0)] private int points = 50;
    [SerializeField] private string keyId;
    [SerializeField] private AudioClip pickupSound;
    private bool collected;
    public bool IsAmmo => type == PickupType.Ammo;
    public bool IsAvailable => !collected && isActiveAndEnabled;
    public void SetKey(string id) { type = PickupType.Key; keyId = id; }
    private void OnTriggerEnter2D(Collider2D other) => Collect(other);
    private void OnTriggerStay2D(Collider2D other) => Collect(other);

    private void Collect(Collider2D other)
    {
        if (collected || !GameController.CanControl || other.GetComponentInParent<PlayerMovement>() == null) return;
        switch (type)
        {
            case PickupType.Ammo: GameController.AddAmmo(ammoAmount); break;
            case PickupType.Health: if (!GameController.TryHeal()) return; break;
            case PickupType.Treasure: GameController.AddScore(points); break;
            case PickupType.Key:
                if (!GameController.AddKey(keyId)) return;
                GameController.AddScore(points);
                if (WaveManager.Instance != null) WaveManager.Instance.Notify("CREDENCIAL " + keyId + " ADQUIRIDA", 2f);
                break;
        }
        collected = true;
        GameAudio.Play(pickupSound);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
