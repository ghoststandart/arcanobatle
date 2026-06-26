using UnityEngine;

/// <summary>
/// A fast drop-shaped projectile that flies straight up or down and passes
/// through the paddle, damaging up to <see cref="Hits"/> of its segment cubes.
/// Variants (e.g. <see cref="SlowBulletBonus"/>) just override the knobs below.
/// </summary>
public class BulletBonus : IBonus
{
    private static Material _trailMat;

    protected virtual float FlySpeed { get { return 30f; } }
    protected virtual int Damage { get { return 2; } }
    protected virtual int Hits { get { return 2; } }
    protected virtual Color Tint { get { return Color.white; } }
    protected virtual Color TrailColor { get { return new Color(1f, 0.85f, 0.2f, 0.7f); } }
    protected virtual float DownChance { get { return 0.5f; } }   // 0.5 = even up/down
    protected virtual float Chance { get { return 0.2f; } }
    protected virtual bool Homes { get { return false; } }
    protected virtual float Scale { get { return 0.32f; } }

    public float DropChance { get { return Chance; } }
    public float Speed { get { return FlySpeed; } }
    public bool PiercesPaddle { get { return true; } }
    public int MaxPaddleHits { get { return Hits; } }
    public bool HomesToPaddle { get { return Homes; } }

    public Vector2 PickDirection()
    {
        return Random.value < DownChance ? Vector2.down : Vector2.up;
    }

    public void SetupVisual(GameObject go, Vector2 direction)
    {
        var sr = Bonus.AddIcon(go, "Powerups/bullet", Scale, 6);
        sr.color = Tint;

        // The drop texture leads with its thick (round) end up; flip it when the
        // bullet flies downward so the thick end always points the way it travels.
        if (direction.y < 0f)
        {
            go.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }

        if (_trailMat == null)
        {
            _trailMat = new Material(Shader.Find("Sprites/Default"));
        }
        var trail = go.AddComponent<TrailRenderer>();
        trail.time = 0.18f;
        trail.startWidth = Scale * 0.7f;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.05f;
        trail.numCapVertices = 2;
        trail.material = _trailMat;
        Color tc = TrailColor;
        trail.startColor = tc;
        trail.endColor = new Color(tc.r, tc.g, tc.b, 0f);
        trail.sortingOrder = 5;
    }

    public void Apply(BonusContext ctx)
    {
        if (ctx.segment != null)
        {
            ctx.segment.Damage(Damage);
        }
    }
}
