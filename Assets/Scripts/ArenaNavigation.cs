using System.Collections.Generic;
using UnityEngine;

// Campo de distâncias compartilhado: vários perseguidores contornam os mesmos obstáculos.
public sealed class ArenaNavigation
{
    private const int Width = 28, Height = 16;
    private const float Step = 0.5f;
    private readonly bool[,] blocked = new bool[Width, Height];
    private readonly int[,] distance = new int[Width, Height];
    private readonly Queue<Vector2Int> queue = new Queue<Vector2Int>();
    private float nextRefresh;
    private Vector2 target;
    private readonly float clearance;

    public ArenaNavigation(ArenaObstacle[] obstacles, float bodyClearance = 0.4f)
    {
        clearance = bodyClearance;
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            Vector2 point = Position(x, y);
            foreach (ArenaObstacle obstacle in obstacles)
            {
                Vector2 half = obstacle.size * 0.5f + Vector2.one * clearance;
                if (Mathf.Abs(point.x - obstacle.position.x) < half.x && Mathf.Abs(point.y - obstacle.position.y) < half.y)
                    blocked[x, y] = true;
            }
        }
    }

    public void Refresh(Vector2 player)
    {
        target = player;
        if (Time.time < nextRefresh) return;
        nextRefresh = Time.time + 0.15f;
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++) distance[x, y] = int.MaxValue;
        Vector2Int start = Cell(player);
        if (blocked[start.x, start.y])
        {
            float best = float.MaxValue;
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                float d = (Position(x, y) - player).sqrMagnitude;
                if (!blocked[x, y] && d < best) { best = d; start = new Vector2Int(x, y); }
            }
        }
        queue.Clear();
        distance[start.x, start.y] = 0;
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = cell.x + dx, ny = cell.y + dy;
                if ((dx == 0 && dy == 0) || !CanStep(cell.x, cell.y, nx, ny)) continue;
                if (distance[nx, ny] != int.MaxValue) continue;
                distance[nx, ny] = distance[cell.x, cell.y] + 1;
                queue.Enqueue(new Vector2Int(nx, ny));
            }
        }
    }

    public Vector2 Direction(Vector2 from)
    {
        Vector2 direct = target - from;
        if (direct.sqrMagnitude < 0.01f) return Vector2.zero;
        if (Physics2D.CircleCast(from, clearance, direct.normalized, direct.magnitude, 1 << 7).collider == null)
            return direct.normalized;
        Vector2Int cell = Cell(from);
        int best = distance[cell.x, cell.y];
        Vector2 destination = from;
        float bestPhysical = float.MaxValue;
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            int nx = cell.x + dx, ny = cell.y + dy;
            if (!CanStep(cell.x, cell.y, nx, ny)) continue;
            int cost = distance[nx, ny];
            float physical = (Position(nx, ny) - from).sqrMagnitude;
            if (cost < best || (cost == best && cost != int.MaxValue && physical < bestPhysical))
            {
                best = cost; bestPhysical = physical; destination = Position(nx, ny);
            }
        }
        return (destination - from).normalized;
    }

    // Consulta o mesmo campo de distâncias usado pelos perseguidores.
    // A margem deles é maior que a do jogador: a seleção de itens é conservadora.
    public bool IsReachable(Vector2 point)
    {
        if (Mathf.Abs(point.x) > 7f - clearance || Mathf.Abs(point.y) > 4f - clearance) return false;
        Vector2Int cell = Cell(point);
        return !blocked[cell.x, cell.y] && distance[cell.x, cell.y] != int.MaxValue;
    }

    public bool TryFindPickupPosition(Vector2 preferred, out Vector2 result)
    {
        result = Vector2.zero;
        float best = float.MaxValue;
        bool found = false;
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            Vector2 candidate = Position(x, y);
            float cost = (candidate - preferred).sqrMagnitude;
            if (cost >= best || !IsReachable(candidate)) continue;
            // Evita paredes, inimigos e o próprio jogador; não impede outros itens.
            if (Physics2D.OverlapCircle(candidate, 0.5f, (1 << 7) | (1 << 6) | (1 << 3)) != null) continue;
            best = cost;
            result = candidate;
            found = true;
        }
        return found;
    }

    public bool CanSpawnEnemy(Vector2 point, Vector2 player)
    {
        return IsReachable(point) && (point - player).sqrMagnitude >= 2.25f
            && Physics2D.OverlapCircle(point, clearance + 0.05f, (1 << 7) | (1 << 6) | (1 << 3)) == null;
    }

    public bool NeedsRecovery(Vector2 point)
    {
        if (IsReachable(point)) return false;
        // Não confunde a borda de uma célula com um inimigo preso.
        for (int x = -2; x <= 2; x++)
        for (int y = -2; y <= 2; y++)
        {
            Vector2 destination = point + new Vector2(x, y) * Step;
            Vector2 direction = destination - point;
            if (!IsReachable(destination)) continue;
            if (Physics2D.CircleCast(point, clearance, direction.normalized, direction.magnitude, 1 << 7).collider == null) return false;
        }
        return true;
    }

    public bool TryFindEnemyPosition(Vector2 preferred, Vector2 player, out Vector2 result)
    {
        result = Vector2.zero;
        float best = float.MaxValue;
        bool found = false;
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            Vector2 point = Position(x, y);
            float cost = (point - preferred).sqrMagnitude;
            if (cost >= best || !CanSpawnEnemy(point, player)) continue;
            best = cost; result = point; found = true;
        }
        return found;
    }

    private bool CanStep(int x, int y, int nx, int ny)
    {
        if (nx < 0 || ny < 0 || nx >= Width || ny >= Height || blocked[nx, ny]) return false;
        return nx == x || ny == y || (!blocked[nx, y] && !blocked[x, ny]);
    }
    private static Vector2 Position(int x, int y) => new Vector2(-7f + (x + 0.5f) * Step, -4f + (y + 0.5f) * Step);
    private static Vector2Int Cell(Vector2 p) => new Vector2Int(Mathf.Clamp(Mathf.FloorToInt((p.x + 7f) / Step), 0, Width - 1), Mathf.Clamp(Mathf.FloorToInt((p.y + 4f) / Step), 0, Height - 1));
}
