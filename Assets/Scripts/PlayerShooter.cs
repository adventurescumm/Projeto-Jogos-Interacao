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
    private InputAction aimAction, pointerAction, attackAction;
    private Vector2 manualAim = Vector2.right, aimDirection = Vector2.right;
    private bool usingMouse, stickEngaged, triggerHeld, waitForAttackRelease, pauseRequested;
    private float nextShotTime, lastPadUse, previousStrength, snapbackUntil;
    private Rigidbody2D rb;
    private Gamepad activePad;

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
    private void OnEnable() => InputSystem.onDeviceChange += DeviceChanged;
    private void OnDisable() => InputSystem.onDeviceChange -= DeviceChanged;
    private void DeviceChanged(InputDevice device, InputDeviceChange change)
    {
        if (usingMouse || device != activePad) return;
        if (change != InputDeviceChange.Disconnected && change != InputDeviceChange.Removed) return;
        waitForAttackRelease = true;
        triggerHeld = stickEngaged = false;
        pauseRequested = !GameController.SetPaused(true);
    }
    private void OnApplicationFocus(bool focused)
    {
        if (!focused && GameController.Initialized && !GameController.gameOver)
        { waitForAttackRelease = true; pauseRequested = !GameController.SetPaused(true); }
    }
    private void Update()
    {
        // Se a desconexão ocorreu durante o fade, pausa assim que a sala entrar.
        if (pauseRequested && GameController.CanControl)
            pauseRequested = !GameController.SetPaused(true);
        if (!GameController.CanControl || GameController.SuppressPlayerInput)
        {
            if (GameController.Paused) waitForAttackRelease = true;
            return;
        }
        Gamepad pad = aimAction.activeControl != null ? aimAction.activeControl.device as Gamepad : null;
        if (pad == null || !pad.added) pad = Gamepad.current;
        activePad = pad;
        Vector2 stick = pad != null ? pad.rightStick.ReadUnprocessedValue() : aimAction.ReadValue<Vector2>();
        float strength = Mathf.Clamp01(stick.magnitude);
        float deadzone = ControllerSettings.Deadzone;
        bool wasEngaged = stickEngaged;
        stickEngaged = strength > (wasEngaged ? deadzone * 0.75f : deadzone);
        // Evita um giro involuntário quando a mola do analógico passa pelo centro.
        if (previousStrength > 0.75f && strength < 0.55f && Vector2.Angle(manualAim, stick) > 100f)
            snapbackUntil = Time.unscaledTime + 0.08f;
        if (Time.unscaledTime < snapbackUntil && strength < 0.55f) stickEngaged = false;
        previousStrength = strength;
        float trigger = pad != null ? pad.rightTrigger.ReadValue() : 0f;
        triggerHeld = trigger > (triggerHeld ? 0.10f : 0.15f);
        bool padAttack = triggerHeld || (pad != null && (pad.rightShoulder.isPressed || pad.buttonWest.isPressed));
        bool padUsed = stickEngaged || padAttack || (pad != null && pad.leftStick.ReadUnprocessedValue().sqrMagnitude > 0.09f);
        bool mouseClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool mouseMotion = Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 4f;
        if (mouseClick) usingMouse = true;
        else if (padUsed) { usingMouse = false; lastPadUse = Time.unscaledTime; }
        else if (mouseMotion && Time.unscaledTime - lastPadUse > 0.25f) usingMouse = true;

        bool pointerInside = true;
        if (usingMouse && Mouse.current != null && gameCamera != null)
        {
            Vector2 pointer = pointerAction.ReadValue<Vector2>();
            pointerInside = gameCamera.pixelRect.Contains(pointer);
            if (pointerInside)
            {
                Vector3 screen = new Vector3(pointer.x, pointer.y, -gameCamera.transform.position.z);
                Vector2 direction = (Vector2)gameCamera.ScreenToWorldPoint(screen) - rb.position;
                if (direction.sqrMagnitude > 0.001f) manualAim = direction.normalized;
            }
        }
        else if (stickEngaged)
        {
            float response = ControllerSettings.Response(Mathf.InverseLerp(deadzone, 1f, strength));
            // Entrada inicial e uma virada deliberada devem responder imediatamente.
            bool snap = ControllerSettings.AimStyle == 0 || !wasEngaged
                || (strength > 0.8f && Vector2.Angle(manualAim, stick) > 100f);
            manualAim = snap ? stick.normalized : SmoothDirection(manualAim, stick, response, Time.deltaTime);
        }
        aimDirection = !usingMouse && ControllerSettings.AimAssist ? AssistedDirection(manualAim) : manualAim;
        bool attack = usingMouse ? attackAction.IsPressed() : padAttack;
        if (waitForAttackRelease)
        {
            if (!attackAction.IsPressed() && !padAttack) waitForAttackRelease = false;
            return;
        }
        if (!pointerInside || projectilePrefab == null || !attack || Time.time < nextShotTime) return;
        if (!GameController.TrySpendAmmo()) return;
        Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.Init(aimDirection);
        nextShotTime = Time.time + shotInterval;
    }
    private Vector2 AssistedDirection(Vector2 direction)
    {
        var enemies = WaveManager.Instance != null ? WaveManager.Instance.CurrentEnemies : null;
        if (enemies == null || !GameController.CombatActive) return direction;
        float best = 7f;
        Vector2 target = direction;
        foreach (EnemyChaser enemy in enemies)
        {
            if (enemy == null || enemy.Dead || !enemy.gameObject.activeInHierarchy) continue;
            Vector2 toEnemy = (Vector2)enemy.transform.position - rb.position;
            if (toEnemy.sqrMagnitude < 0.01f || toEnemy.sqrMagnitude > 64f) continue;
            float angle = Vector2.Angle(direction, toEnemy);
            if (angle >= best || Physics2D.Raycast(rb.position, toEnemy.normalized, toEnemy.magnitude, 1 << 7).collider != null) continue;
            best = angle; target = toEnemy.normalized;
        }
        // Corrige até 2,5 graus. Não fixa o alvo nem atravessa coberturas.
        float correction = Mathf.Clamp(Vector2.SignedAngle(direction, target), -2.5f, 2.5f);
        return Quaternion.Euler(0f, 0f, correction) * direction;
    }
    public static Vector2 SmoothDirection(Vector2 current, Vector2 target, float response, float deltaTime)
    {
        if (target.sqrMagnitude < 0.0001f) return current;
        float from = Mathf.Atan2(current.y, current.x) * Mathf.Rad2Deg;
        float to = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
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
