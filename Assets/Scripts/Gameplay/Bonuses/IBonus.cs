using UnityEngine;

/// <summary>
/// One kind of bonus. The <see cref="Bonus"/> processor owns the GameObject,
/// its movement and lifecycle; each IBonus only declares its look, its travel
/// speed, how it interacts with the paddle, and what it does (Apply).
/// </summary>
public interface IBonus
{
    /// <summary>
    /// Builds the visuals (sprite, scale, colour, trail, orientation) on the
    /// already-created bonus GameObject. <paramref name="direction"/> is the
    /// vertical travel direction the processor picked.
    /// </summary>
    void SetupVisual(GameObject go, Vector2 direction);

    /// <summary>Travel speed along the processor's chosen direction.</summary>
    float Speed { get; }

    /// <summary>
    /// False: caught by a paddle and consumed. True: passes through the paddle,
    /// affecting up to <see cref="MaxPaddleHits"/> of its segments.
    /// </summary>
    bool PiercesPaddle { get; }

    /// <summary>How many paddle segments a piercing bonus may affect.</summary>
    int MaxPaddleHits { get; }

    /// <summary>
    /// The effect. <paramref name="ctx"/> carries the caught paddle (catch) or the
    /// hit segment (pierce).
    /// </summary>
    void Apply(BonusContext ctx);
}
