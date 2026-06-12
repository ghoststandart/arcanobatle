using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the paddle out of destructible cubes (children of the paddle root)
/// and distributes repair lives among damaged/destroyed segments.
/// The paddle is two rows of cubes with a single centered cube on each edge
/// (a capsule-like shape). Cube world size is (rootWidth / columns) wide and
/// rootHeight tall, so with a root scale of (2, 0.2) cubes are 0.2 x 0.2 and
/// the full paddle is 2.0 x 0.4.
/// </summary>
public class PaddleHealth : MonoBehaviour
{
    public int columns = 10;
    public int segmentMaxHealth = 2;

    [Tooltip("Max deflection from vertical (degrees) when the ball hits the very edge of the paddle. Center hits bounce straight.")]
    public float maxBounceAngle = 60f;

    private readonly List<PaddleSegment> _segments = new List<PaddleSegment>();

    public void Build(Sprite sprite)
    {
        _segments.Clear();
        for (int col = 0; col < columns; col++)
        {
            float x = -0.5f + (col + 0.5f) / columns;
            bool edge = col == 0 || col == columns - 1;
            if (edge)
            {
                CreateSegment(sprite, x, 0f);
            }
            else
            {
                CreateSegment(sprite, x, 0.5f);
                CreateSegment(sprite, x, -0.5f);
            }
        }
    }

    void CreateSegment(Sprite sprite, float localX, float localY)
    {
        GameObject seg = new GameObject("PaddleSegment");
        seg.transform.SetParent(transform, false);
        seg.transform.localPosition = new Vector3(localX, localY, 0f);
        seg.transform.localScale = new Vector3(1f / columns, 1f, 1f);

        var sr = seg.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        var col = seg.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        var segment = seg.AddComponent<PaddleSegment>();
        segment.owner = this;
        segment.Init(segmentMaxHealth);
        _segments.Add(segment);
    }

    /// <summary>
    /// Hands out the given number of lives one by one to random segments that
    /// are below max health. Destroyed segments come back to life as soon as
    /// they receive a point. Stops early when the whole paddle is at full health.
    /// </summary>
    public void RestoreRandom(int lives)
    {
        List<PaddleSegment> damaged = new List<PaddleSegment>();
        for (int i = 0; i < lives; i++)
        {
            damaged.Clear();
            foreach (PaddleSegment segment in _segments)
            {
                if (segment.health < segment.maxHealth)
                {
                    damaged.Add(segment);
                }
            }

            if (damaged.Count == 0)
            {
                return;
            }

            damaged[Random.Range(0, damaged.Count)].Heal(1);
        }
    }
}
