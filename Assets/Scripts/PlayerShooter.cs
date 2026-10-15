using UnityEngine; // Disponibiliza câmera, vetores e criação de objetos.
using UnityEngine.InputSystem; // Disponibiliza ações e acesso ao mouse.

public class PlayerShooter : MonoBehaviour // Controla mira e disparos.
{
    [SerializeField] private Projectile projectilePrefab; // Prefab usado nos disparos.
    [SerializeField] private Transform aimPivot; // Objeto que mostrará a direção da mira.
    [SerializeField] private Camera gameCamera; // Câmera usada para interpretar o mouse.
    [SerializeField, Min(0.05f)] private float shotInterval = 0.2f; // Intervalo mínimo entre tiros.

    private InputAction aimAction; // Ação do analógico direito.
    private InputAction pointerAction; // Ação da posição do mouse.
    private InputAction attackAction; // Ação Attack já existente no projeto.

    private Vector2 aimDirection = Vector2.right; // Direção inicial e última direção válida.
    private bool usingMouse; // Indica qual dispositivo controla a mira.
    private float nextShotTime; // Instante em que outro tiro será permitido.

    private void Awake() // Obtém as referências das ações.
    {
        aimAction = InputSystem.actions.FindAction("Player/Aim", true); // Localiza a ação do analógico.
        pointerAction = InputSystem.actions.FindAction("Player/Pointer", true); // Localiza a posição do mouse.
        attackAction = InputSystem.actions.FindAction("Player/Attack", true); // Reutiliza a ação de ataque.
    }

    private void Update() // Atualiza a mira e verifica os disparos.
    {
        if (GameController.gameOver) return; // Bloqueia o combate após o encerramento.

        Vector2 stick = aimAction.ReadValue<Vector2>(); // Lê o analógico direito.

        if (stick.sqrMagnitude > 0.04f) // Verifica se o analógico está inclinado além do limite escolhido.
        {
            usingMouse = false; // Passa o controle da mira para o gamepad.
        }
        else if (Mouse.current != null) // Verifica se existe um mouse.
        {
            bool moved = Mouse.current.delta.ReadValue().sqrMagnitude > 0f; // Detecta movimento do mouse.
            bool clicked = Mouse.current.leftButton.wasPressedThisFrame; // Detecta um clique de disparo.

            if (moved || clicked) usingMouse = true; // Passa a mira para o mouse quando ele é utilizado.
        }

        Vector2 desiredDirection = stick; // Começa considerando a direção do analógico.

        if (usingMouse && Mouse.current != null) // Verifica se a mira está sendo controlada pelo mouse.
        {
            Vector2 pointerPosition = pointerAction.ReadValue<Vector2>(); // Lê a posição do cursor na tela.

            if (!gameCamera.pixelRect.Contains(pointerPosition)) return; // Interrompe a mira e o disparo quando o cursor está fora da área da câmera.

            Vector3 screenPosition = pointerPosition; // Prepara a posição para a conversão.
            screenPosition.z = -gameCamera.transform.position.z; // Define a distância até o plano Z = 0.

            Vector2 worldPosition = gameCamera.ScreenToWorldPoint(screenPosition); // Converte o cursor para uma posição na arena.
            desiredDirection = worldPosition - (Vector2)transform.position; // Calcula a direção até essa posição.
        }

        if (desiredDirection.sqrMagnitude > 0.001f) // Ignora uma direção praticamente nula.
        {
            aimDirection = desiredDirection.normalized; // Atualiza a última direção válida.
        }

        if (!attackAction.IsPressed() || Time.time < nextShotTime) return; // Aguarda o comando e o intervalo.
        if (!GameController.TrySpendAmmo()) return; // Interrompe se não houver munição.

        Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity); // Cria o tiro no centro do jogador.
        projectile.Init(aimDirection); // Define a direção do projétil.
        nextShotTime = Time.time + shotInterval; // Agenda o próximo disparo permitido.
    }

    private void LateUpdate() // Atualiza o indicador depois das atualizações normais.
    {
        aimPivot.position = transform.position; // Faz o indicador acompanhar o jogador.
        aimPivot.right = aimDirection; // Aponta seu eixo X na direção da mira.
    }
}