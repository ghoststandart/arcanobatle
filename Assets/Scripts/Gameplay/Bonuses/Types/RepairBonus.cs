using UnityEngine;

/// <summary>Caught bonus that repairs random damaged segments of the catching paddle.</summary>
public class RepairBonus : IBonus
{
    private const float HealFraction = 1f / 3f;

    // How hard a fully-dead paddle pulls the life bonus, relative to a healthy one.
    // The weak paddle's weight runs from 1 (at half health) up to 1 + PullStrength
    // (at zero), so even at its strongest it's a ratio, never a certainty.
    private const float PullStrength = 5f;

    public float DropChance { get { return 0.08f; } }
    public float Speed { get { return 2f; } }
    public bool PiercesPaddle { get { return false; } }
    public int MaxPaddleHits { get { return 0; } }
    public bool HomesToPaddle { get { return false; } }

    // Each paddle gets a pull weight (1 while at or above half health, rising the
    // weaker it gets). The bonus drifts toward a paddle with probability equal to
    // its share of the total weight — so two healthy paddles split 1:1, and a
    // struggling player gets repairs more often, proportional to how hurt it is.
    public Vector2 PickDirection()
    {
        float wBottom = Weight(FindPaddle("Paddle"));     // bottom paddle (-y)
        float wTop = Weight(FindPaddle("PaddleTop"));     // top paddle (+y)
        float pDown = wBottom / (wBottom + wTop);
        return Random.value < pDown ? Vector2.down : Vector2.up;
    }

    static float Weight(PaddleHealth paddle)
    {
        if (paddle == null)
        {
            return 1f;
        }
        // 0 while healthy (>= half), ramping to 1 as health reaches zero.
        float deficit = Mathf.Clamp01((0.5f - paddle.HealthFraction) / 0.5f);
        return 1f + PullStrength * deficit;
    }

    static PaddleHealth FindPaddle(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<PaddleHealth>() : null;
    }

    public void SetupVisual(GameObject go, Vector2 direction)
    {
        Bonus.AddIcon(go, "Powerups/repair", 0.6f, 5);
    }

    public void Apply(BonusContext ctx)
    {
        if (ctx.paddle != null)
        {
            // Always restore about a third of the paddle's full health.
            int lives = Mathf.Max(1, Mathf.RoundToInt(ctx.paddle.MaxHealth * HealFraction));
            ctx.paddle.RestoreRandom(lives);
        }
    }
}
