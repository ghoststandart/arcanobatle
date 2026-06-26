using UnityEngine;

/// <summary>
/// A slower, weaker bullet variant: half speed, 1 damage per cube, a slightly
/// redder tint, and biased to fly toward the bottom paddle.
/// </summary>
public class SlowBulletBonus : BulletBonus
{
    protected override float Chance { get { return 0.3f; } }
    protected override float FlySpeed { get { return 12f; } }
    protected override int Damage { get { return 1; } }
    protected override Color Tint { get { return new Color(1f, 0.7f, 0.6f); } }
    protected override Color TrailColor { get { return new Color(1f, 0.45f, 0.3f, 0.7f); } }
    protected override float DownChance { get { return 1f; } }   // always toward the paddle
    protected override bool Homes { get { return true; } }       // steers onto the paddle centre
}
