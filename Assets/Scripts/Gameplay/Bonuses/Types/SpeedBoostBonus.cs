using UnityEngine;

/// <summary>Caught bonus that temporarily speeds the ball up.</summary>
public class SpeedBoostBonus : IBonus
{
    private const float Amount = 5f;
    private const float Duration = 5f;

    public float DropChance { get { return 0.1f; } }
    public float Speed { get { return 2f; } }
    public bool PiercesPaddle { get { return false; } }
    public int MaxPaddleHits { get { return 0; } }
    public bool HomesToPaddle { get { return false; } }

    public Vector2 PickDirection()
    {
        return Bonus.RandomDirection();
    }

    public void SetupVisual(GameObject go, Vector2 direction)
    {
        Bonus.AddIcon(go, "Powerups/speed", 0.6f, 5);
    }

    public void Apply(BonusContext ctx)
    {
        // Speed up one random ball in play.
        var balls = Object.FindObjectsByType<BallController>(FindObjectsSortMode.None);
        if (balls.Length == 0)
        {
            return;
        }
        balls[Random.Range(0, balls.Length)].ApplySpeedBoost(Amount, Duration);
    }
}
