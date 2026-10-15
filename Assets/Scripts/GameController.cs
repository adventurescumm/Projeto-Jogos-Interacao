using UnityEngine; // Disponibiliza Mathf e Time.

public static class GameController // Guarda o estado compartilhado da partida.
{
    public const int MaxHealth = 3; // Define o máximo de pontos de vida.

    public static int health { get; private set; } // Guarda a vida atual.
    public static int ammo { get; private set; } // Guarda a munição disponível.
    public static int score { get; private set; } // Guarda a pontuação acumulada.
    public static int wave { get; private set; } // Guarda o número da onda.
    public static int enemiesRemaining { get; private set; } // Conta os inimigos vivos.

    public static float elapsedTime { get; private set; } // Guarda a duração da partida.
    public static float timeLimit { get; private set; } // Guarda o prazo opcional.
    public static bool timeLimitEnabled { get; private set; } // Indica se o prazo está ativo.

    public static bool timeExpired => timeLimitEnabled && elapsedTime >= timeLimit; // Verifica o prazo apenas quando habilitado.
    public static bool gameOver => health <= 0 || timeExpired; // Encerra por falta de vida ou pelo prazo opcional.

    private static float nextDamageTime; // Instante em que outro dano será permitido.

    public static void Init(float duration, bool useTimeLimit) // Inicializa uma partida inteira.
    {
        health = MaxHealth; // Restaura as três vidas.
        ammo = 12; // Entrega doze tiros iniciais.
        score = 0; // Zera os pontos.
        wave = 0; // A primeira onda será iniciada pelo gerenciador.
        enemiesRemaining = 0; // Ainda não existem inimigos registrados.
        elapsedTime = 0f; // Zera o tempo total.
        timeLimit = Mathf.Max(1f, duration); // Garante um prazo positivo.
        timeLimitEnabled = useTimeLimit; // Registra se haverá limite de tempo.
        nextDamageTime = 0f; // Permite receber o primeiro dano.
    }

    public static void StartWave(int amount) // Registra uma nova onda.
    {
        wave++; // Avança o número da onda.
        enemiesRemaining = amount; // Registra quantos inimigos foram criados.
    }

    public static void UpdateTime(float deltaTime) // Atualiza o tempo da partida.
    {
        if (gameOver) return; // Para o cronômetro após a derrota.

        elapsedTime += deltaTime; // Acrescenta o tempo transcorrido.

        if (timeLimitEnabled) // Verifica se existe prazo.
        {
            elapsedTime = Mathf.Min(elapsedTime, timeLimit); // Impede ultrapassar o limite.
        }
    }

    public static bool TrySpendAmmo() // Tenta consumir a munição de um tiro.
    {
        if (gameOver || ammo <= 0) return false; // Impede disparos sem munição ou após a derrota.

        ammo--; // Consome uma unidade.
        return true; // Informa que o disparo pode acontecer.
    }

    public static void AddAmmo(int amount) // Acrescenta munição.
    {
        if (gameOver) return; // Impede alterações após a derrota.

        ammo += Mathf.Max(0, amount); // Acrescenta uma quantidade não negativa.
    }

    public static bool TryHeal() // Tenta recuperar uma vida.
    {
        if (gameOver || health >= MaxHealth) return false; // Impede reviver ou ultrapassar três vidas.

        health++; // Recupera uma vida.
        return true; // Informa que o item pode ser consumido.
    }

    public static void TakeDamage() // Tenta retirar uma vida.
    {
        if (gameOver || Time.time < nextDamageTime) return; // Respeita a derrota e o intervalo de proteção.

        health--; // Retira uma vida.
        nextDamageTime = Time.time + 1f; // Bloqueia outros danos por um segundo.
    }

    public static void RegisterKill() // Registra a eliminação de um inimigo.
    {
        if (gameOver || enemiesRemaining <= 0) return; // Impede alterações fora de uma onda válida.

        enemiesRemaining--; // Desconta um inimigo.
        score += 10; // Acrescenta dez pontos pela eliminação.
    }
}