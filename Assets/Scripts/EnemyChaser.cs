using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class EnemyChaser : MonoBehaviour
{
    [SerializeField] private float spriteAngleOffset;
    [SerializeField] private DeathEffect deathEffectPrefab;
    [SerializeField] private AudioClip deathSound;
    [SerializeField, Min(1)] private int maxHealth = 1;
    [SerializeField, Min(0)] private int killPoints = 10;
    [SerializeField, Min(0f)] private float speedMultiplier = 1.15f;
    [SerializeField] private bool stationary;
    [SerializeField] private bool isBoss;
    [SerializeField, Min(1f)] private float bossExplosionScale = 3f;
    [SerializeField, Min(1f)] private float bossExplosionDuration = 3f;
    [SerializeField] private string displayName = "Soldado";
    private Rigidbody2D rb;
    private SpriteRenderer image;
    private Color baseColor;
    private Transform player;
    private float speed;
    private float hitUntil;
    private bool warning;
    private Color warningColor;
    private bool dead;
    private int health;
    public int MaxHealth => maxHealth;
    public int Health => health;
    public float HealthRatio => maxHealth > 0 ? (float)health / maxHealth : 0f;
    public bool IsBoss => isBoss;
    public string DisplayName => displayName;
    public bool Dead => dead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        image = GetComponent<SpriteRenderer>();
        baseColor = image.color;
        rb.gravityScale = 0f;
        rb.angularVelocity = 0f;
        rb.constraints = stationary ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;
        Collider2D body = GetComponent<Collider2D>();
        if (body != null) body.sharedMaterial = GamePhysics.NoFriction;
        health = Mathf.Max(1, maxHealth);
    }

    public void Init(Transform target, float movementSpeed)
    {
        player = target;
        speed = speedMultiplier * movementSpeed;
        health = Mathf.Max(1, maxHealth);
        dead = false;
        EnemyShooter shooter = GetComponent<EnemyShooter>();
        if (shooter != null && shooter.enabled) shooter.Init(target);
    }

    private void FixedUpdate()
    {
        rb.angularVelocity = 0f;
        if (dead || !GameController.CanControl || !GameController.CombatActive || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 facing = (Vector2)player.position - rb.position;
        Vector2 travel = facing.normalized;
        if (WaveManager.Instance != null)
            travel = WaveManager.Instance.DirectionToPlayer(rb.position);
        rb.linearVelocity = stationary ? Vector2.zero : travel * speed;
        if (facing.sqrMagnitude > 0.001f)
            rb.SetRotation(Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg + spriteAngleOffset);
    }

    private void LateUpdate()
    {
        if (image != null) image.color = Time.time < hitUntil ? Color.white : warning ? warningColor : baseColor;
    }

    public void SetWarning(bool active, Color color)
    {
        warning = active;
        warningColor = color;
    }

    private void OnCollisionEnter2D(Collision2D collision) => DamagePlayer(collision);
    private void OnCollisionStay2D(Collision2D collision) => DamagePlayer(collision);
    private void DamagePlayer(Collision2D collision)
    {
        if (!dead && collision.gameObject.GetComponentInParent<PlayerMovement>() != null)
            GameController.TakeDamage();
    }

    public void TakeDamage(int amount = 1)
    {
        if (dead || !GameController.CanControl || !GameController.CombatActive || amount <= 0) return;
        health = Mathf.Max(0, health - amount);
        hitUntil = Time.time + 0.09f;
        if (health == 0) Die();
    }

    public void Die()
    {
        if (dead || GameController.gameOver) return;
        dead = true;
        DeathEffect effect = null;
        if (deathEffectPrefab != null)
        {
            effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            if (isBoss) effect.transform.localScale *= bossExplosionScale;
            effect.Play(deathSound, isBoss ? bossExplosionDuration : 1f);
        }
        else GameAudio.Play(deathSound);
        if (isBoss && WaveManager.Instance != null) WaveManager.Instance.BossDefeated(effect);
        GameController.RegisterKill(killPoints);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
