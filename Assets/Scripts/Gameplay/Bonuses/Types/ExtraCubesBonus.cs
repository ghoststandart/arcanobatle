using UnityEngine;

/// <summary>Caught bonus that adds extra cubes at random spots around the catching paddle.</summary>
public class ExtraCubesBonus : IBonus
{
    private const int Cubes = 3;

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
        Bonus.AddIcon(go, "Powerups/extracubes", 0.6f, 5);
    }

    public void Apply(BonusContext ctx)
    {
        if (ctx.paddle != null)
        {
            ctx.paddle.AddRandomSegments(Cubes);
        }
    }
}
