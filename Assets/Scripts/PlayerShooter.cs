using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform aimPivot;
    [SerializeField] private Camera gameCamera;
    [SerializeField, Min(0.05f)] private float shotInterval = 0.2f;
    [SerializeField] private float spriteAngleOffset;
    [Header("Mira do controle")]
    [SerializeField, Range(0.05f, 0.35f)] private float stickDeadzone = 0.12f;
    [SerializeField, Min(1f)] private float fineAimResponse = 18f;
    [SerializeField, Min(1f)] private float fullAimResponse = 45f;
    private InputAction aimAction, pointerAction, attackAction;
    private Vector2 aimDirection = Vector2.right;
    private bool usingMouse;
    private bool waitForAttackRelease;
    private float nextShotTime;
    private Rigidbody2D rb;

    private void Awake()
    {
        if (projectilePrefab == null) Debug.LogError("PlayerShooter: conecte o prefab Projectile.", this);
        aimAction = InputSystem.actions.FindAction("Player/Aim", true);
        pointerAction = InputSystem.actions.FindAction("Player/Pointer", true);
        attackAction = InputSystem.actions.FindAction("Player/Attack", true);
        rb = GetComponent<Rigidbody2D>();
        if (gameCamera == null) gameCamera = Camera.main;
        aimAction.Enable(); pointerAction.Enable(); attackAction.Enable();
        usingMouse = Mouse.current != null;
    }

    private void Update()
    {
        if (!GameController.CanControl || GameController.SuppressPlayerInput)
        {
            if (GameController.Paused) waitForAttackRelease = true;
            return;
        }
        Gamepad pad = aimAction.activeControl != null ? aimAction.activeControl.device as Gamepad : null;
        if (pad == null) pad = Gamepad.current;
        // Leitura crua: uma única zona morta radial, sem somá-la à do Input System.
        Vector2 stick = pad != null ? pad.rightStick.ReadUnprocessedValue() : aimAction.ReadValue<Vector2>();
        bool stickMoved = stick.sqrMagnitude > stickDeadzone * stickDeadzone;
        bool mouseUsed = Mouse.current != null && (Mouse.current.delta.ReadValue().sqrMagnitude > 1f
            || Mouse.current.leftButton.wasPressedThisFrame);
        if (mouseUsed) usingMouse = true;
        else if (stickMoved) usingMouse = false;

        bool pointerInside = true;
        if (usingMouse && Mouse.current != null && gameCamera != null)
        {
            Vector2 pointer = pointerAction.ReadValue<Vector2>();
            pointerInside = gameCamera.pixelRect.Contains(pointer);
            if (pointerInside)
            {
                Vector3 screen = new Vector3(pointer.x, pointer.y, -gameCamera.transform.position.z);
                Vector2 direction = (Vector2)gameCamera.ScreenToWorldPoint(screen) - rb.position;
                if (direction.sqrMagnitude > 0.001f) aimDirection = direction.normalized;
            }
        }
        else if (stickMoved)
        {
            float strength = Mathf.InverseLerp(stickDeadzone, 1f, stick.magnitude);
            float response = Mathf.Lerp(fineAimResponse, fullAimResponse, strength);
            aimDirection = SmoothDirection(aimDirection, stick, response, Time.deltaTime);
        }
        // Soltar o analógico conserva a última direção: não retorna ao centro.
        if (waitForAttackRelease)
        {
            if (!attackAction.IsPressed()) waitForAttackRelease = false;
            return;
        }
        if (!pointerInside || projectilePrefab == null || !attackAction.IsPressed() || Time.time < nextShotTime) return;
        if (!GameController.TrySpendAmmo()) return;
        Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.Init(aimDirection);
        nextShotTime = Time.time + shotInterval;
    }

    public static Vector2 SmoothDirection(Vector2 current, Vector2 target, float response, float deltaTime)
    {
        if (target.sqrMagnitude < 0.0001f) return current;
        float from = Mathf.Atan2(current.y, current.x) * Mathf.Rad2Deg;
        float to = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
        // Interpola ângulos, evitando vetor zero ao virar 180 graus; independente do FPS.
        float angle = Mathf.LerpAngle(from, to, 1f - Mathf.Exp(-response * Mathf.Max(0f, deltaTime))) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private void FixedUpdate()
    {
        if (!GameController.CanControl) return;
        rb.SetRotation(Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg + spriteAngleOffset);
    }

    private void LateUpdate()
    {
        if (aimPivot == null) return;
        aimPivot.position = transform.position;
        aimPivot.right = aimDirection;
    }
}
