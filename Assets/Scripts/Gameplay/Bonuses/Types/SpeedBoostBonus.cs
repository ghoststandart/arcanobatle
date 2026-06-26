using UnityEngine;

/// <summary>Caught bonus that temporarily speeds the ball up.</summary>
public class SpeedBoostBonus : IBonus
{
    private const float Amount = 5f;
    private const float Duration = 5f;

    public float DropChance { get { return 0.12f; } }
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
        var ball = GameObject.Find("Ball");
        if (ball == null)
        {
            return;
        }
        var ballCtrl = ball.GetComponent<BallController>();
        if (ballCtrl != null)
        {
            ballCtrl.ApplySpeedBoost(Amount, Duration);
        }
    }
}
