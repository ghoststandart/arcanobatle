using UnityEngine;

/// <summary>
/// A fast drop-shaped projectile that flies straight up or down and passes
/// through the paddle, damaging up to two of its segment cubes by one each.
/// </summary>
public class BulletBonus : IBonus
{
    private static Material _trailMat;

    public float Speed { get { return 30f; } }
    public bool PiercesPaddle { get { return true; } }
    public int MaxPaddleHits { get { return 2; } }

    public void SetupVisual(GameObject go, Vector2 direction)
    {
        Bonus.AddIcon(go, "Powerups/bullet", 0.48f, 6);

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
        trail.startWidth = 0.35f;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.05f;
        trail.numCapVertices = 2;
        trail.material = _trailMat;
        trail.startColor = new Color(1f, 0.85f, 0.2f, 0.7f);
        trail.endColor = new Color(1f, 0.85f, 0.2f, 0f);
        trail.sortingOrder = 5;
    }

    public void Apply(BonusContext ctx)
    {
        if (ctx.segment != null)
        {
            ctx.segment.Damage(1);
        }
    }
}
