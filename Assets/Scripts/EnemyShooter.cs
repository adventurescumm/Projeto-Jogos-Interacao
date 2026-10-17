using UnityEngine;

[RequireComponent(typeof(EnemyChaser))]
public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float shootingRange = 14f;
    [SerializeField, Min(0.1f)] private float minPause = 1.7f;
    [SerializeField, Min(0.1f)] private float maxPause = 2.5f;
    [SerializeField, Min(0.05f)] private float warningDuration = 0.5f;
    [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField, Min(1)] private int shotsPerBurst = 4;
    [SerializeField, Min(0.05f)] private float burstInterval = 0.16f;
    [SerializeField, Range(1, 5)] private int projectilesPerShot = 1;
    [SerializeField, Range(0f, 45f)] private float spreadAngle = 16f;
    [SerializeField] private bool enrageAtHalfHealth;
    private Transform player;
    private EnemyChaser owner;
    private int phase; // 0 = pausa, 1 = aviso, 2 = rajada.
    private int shotsLeft;
    private float nextAction;
    private Vector2 lockedAim;

    private void Awake() => owner = GetComponent<EnemyChaser>();
    public void Init(Transform target)
    {
        player = target;
        phase = 0;
        nextAction = Time.time + Random.Range(0.8f, 1.5f);
    }

    private void Update()
    {
        if (GameController.Paused) return; // Conserva aviso, mira travada e tiros restantes da rajada.
        if (!GameController.CanControl || !GameController.CombatActive || player == null || owner.Dead)
        {
            owner.SetWarning(false, warningColor);
            phase = 0;
            return;
        }
        if (Time.time < nextAction) return;
        bool enraged = enrageAtHalfHealth && owner.HealthRatio <= 0.5f;
        if (phase == 0)
        {
            if (!CanShoot()) return;
            phase = 1;
            owner.SetWarning(true, warningColor);
            nextAction = Time.time + warningDuration;
        }
        else if (phase == 1)
        {
            owner.SetWarning(false, warningColor);
            if (!CanShoot()) { BeginPause(enraged); return; }
            // A mira fica fixa durante a rajada: o jogador pode sair da linha de tiro.
            lockedAim = ((Vector2)player.position - (Vector2)transform.position).normalized;
            shotsLeft = shotsPerBurst + (enraged ? 1 : 0);
            phase = 2;
            Fire(enraged);
        }
        else Fire(enraged);
    }

    private void Fire(bool enraged)
    {
        for (int i = 0; i < projectilesPerShot; i++)
        {
            float angle = (i - (projectilesPerShot - 1) * 0.5f) * spreadAngle;
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * (Vector3)lockedAim;
            EnemyProjectile shot = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            shot.Init(direction);
        }
        shotsLeft--;
        if (shotsLeft <= 0) BeginPause(enraged);
        else nextAction = Time.time + burstInterval;
    }

    private void BeginPause(bool enraged)
    {
        phase = 0;
        nextAction = Time.time + Random.Range(minPause, Mathf.Max(minPause, maxPause)) * (enraged ? 0.75f : 1f);
    }

    private bool CanShoot()
    {
        if (projectilePrefab == null) return false;
        Vector2 difference = player.position - transform.position;
        return difference.sqrMagnitude > 0.01f && difference.sqrMagnitude <= shootingRange * shootingRange
            && Physics2D.Linecast(transform.position, player.position, obstacleLayers).collider == null;
    }

    private void OnDisable()
    {
        if (owner != null) owner.SetWarning(false, warningColor);
        phase = 0;
    }
}
