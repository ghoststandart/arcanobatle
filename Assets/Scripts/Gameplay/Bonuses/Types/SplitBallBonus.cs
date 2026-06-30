using UnityEngine;

/// <summary>Caught bonus that splits every ball in play into two.</summary>
public class SplitBallBonus : IBonus
{
    private const int MaxBalls = 4;

    // Doesn't drop once there are already two balls; in-flight ones still split up to MaxBalls.
    public float DropChance { get { return BallCount() >= 2 ? 0f : 0.05f; } }
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
        Bonus.AddIcon(go, "Powerups/splitball", 0.6f, 5);
    }

    public void Apply(BonusContext ctx)
    {
        // New bonuses stop dropping at two balls, but ones already in flight can
        // still double the field up to a hard cap of four.
        var balls = Object.FindObjectsByType<BallController>(FindObjectsSortMode.None);
        if (balls.Length == 0 || balls.Length >= MaxBalls)
        {
            return;
        }
        // Split one random ball into two.
        balls[Random.Range(0, balls.Length)].SpawnSplit();
    }

    static int BallCount()
    {
        return Object.FindObjectsByType<BallController>(FindObjectsSortMode.None).Length;
    }
}
