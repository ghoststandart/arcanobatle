using UnityEngine;

/// <summary>Caught bonus that repairs random damaged segments of the catching paddle.</summary>
public class RepairBonus : IBonus
{
    private const int Lives = 10;

    public float Speed { get { return 2f; } }
    public bool PiercesPaddle { get { return false; } }
    public int MaxPaddleHits { get { return 0; } }

    public void SetupVisual(GameObject go, Vector2 direction)
    {
        Bonus.AddIcon(go, "Powerups/repair", 0.6f, 5);
    }

    public void Apply(BonusContext ctx)
    {
        if (ctx.paddle != null)
        {
            ctx.paddle.RestoreRandom(Lives);
        }
    }
}
