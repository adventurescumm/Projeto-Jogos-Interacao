using UnityEngine;

public class PlayerDeathFeedback : MonoBehaviour
{
    [SerializeField] private DeathEffect deathEffectPrefab;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private GameObject aimObject;
    private DeathEffect effect;
    private AudioSource fallbackSound;
    public bool Played { get; private set; }
    public bool Finished => Played && (effect == null || effect.Completed) && (fallbackSound == null || !fallbackSound.isPlaying);
    private void LateUpdate()
    {
        if (!GameController.gameOver || GameController.victory || Played) return;
        Played = true;
        if (deathEffectPrefab != null)
        {
            effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            effect.Play(deathSound);
        }
        else fallbackSound = GameAudio.Play(deathSound);
        if (aimObject != null) aimObject.SetActive(false);
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null) { body.linearVelocity = Vector2.zero; body.angularVelocity = 0f; }
        gameObject.SetActive(false);
    }
}
