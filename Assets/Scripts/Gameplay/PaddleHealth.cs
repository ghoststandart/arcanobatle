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

    [Tooltip("Max deflection from vertical (degrees) at the paddle edge. Higher = flatter (more horizontal) edge bounces. Center hits bounce straight up.")]
    public float maxBounceAngle = 80f;

    [Tooltip("After the ball damages one segment, ignore further segment damage from it for this long, so a single bounce can only break one cube.")]
    public float ballHitCooldown = 0.15f;

    private readonly List<PaddleSegment> _segments = new List<PaddleSegment>();
    private Texture2D _texture;
    private Sprite _fallback;
    private float _lastBallHitTime = -999f;

    /// <summary>
    /// Returns true for the first segment touched in a given ball-paddle contact and
    /// blocks the rest for a short window, so one bounce only ever breaks one cube.
    /// </summary>
    public bool TryClaimBallHit()
    {
        if (Time.time - _lastBallHitTime < ballHitCooldown)
        {
            return false;
        }
        _lastBallHitTime = Time.time;
        return true;
    }

    /// <summary>Full health summed over all segments (each segment's max).</summary>
    public int MaxHealth
    {
        get
        {
            int max = 0;
            foreach (PaddleSegment segment in _segments)
            {
                max += segment.maxHealth;
            }
            return max;
        }
    }

    /// <summary>Current health summed over all segments as a 0..1 fraction of full.</summary>
    public float HealthFraction
    {
        get
        {
            int current = 0;
            int max = 0;
            foreach (PaddleSegment segment in _segments)
            {
                current += segment.health;
                max += segment.maxHealth;
            }
            return max > 0 ? (float)current / max : 0f;
        }
    }

    // Builds the paddle and skins it with the paddle texture, sliced across the
    // segments the same way microbe bricks are sliced across a cluster. Each
    // segment shows its piece; together they reconstruct the white-bar-with-red-
    // caps paddle. Falls back to the plain sprite when no texture is supplied.
    public void Build(Texture2D texture, Sprite fallback)
    {
        _texture = texture;
        _fallback = fallback;
        _segments.Clear();
        for (int col = 0; col < columns; col++)
        {
            float x = -0.5f + (col + 0.5f) / columns;
            bool edge = col == 0 || col == columns - 1;
            if (edge)
            {
                CreateSegment(x, 0f);
            }
            else
            {
                CreateSegment(x, 0.5f);
                CreateSegment(x, -0.5f);
            }
        }
    }

    void CreateSegment(float localX, float localY)
    {
        GameObject seg = new GameObject("PaddleSegment");
        seg.transform.SetParent(transform, false);
        seg.transform.localPosition = new Vector3(localX, localY, 0f);
        seg.transform.localScale = new Vector3(1f / columns, 1f, 1f);

        var col = seg.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        // The visual lives on a child so the (non-square) texture slice can be
        // scaled to fill the segment without the segment's own (1/columns, 1)
        // scale distorting it, and so it can be toggled/tinted independently.
        GameObject skin = new GameObject("Skin");
        skin.transform.SetParent(seg.transform, false);

        var sr = skin.AddComponent<SpriteRenderer>();
        if (_texture != null)
        {
            // Map the segment's local rect (x in [-0.5,0.5], y in [-1,1]) onto the
            // texture (u,v in [0,1]) and slice out that piece. Texture y is bottom-up.
            float u0 = localX - 0.5f / columns + 0.5f;
            float u1 = localX + 0.5f / columns + 0.5f;
            float v0 = (localY + 0.5f) / 2f;
            float v1 = (localY + 1.5f) / 2f;
            int x0 = Mathf.RoundToInt(u0 * _texture.width);
            int x1 = Mathf.RoundToInt(u1 * _texture.width);
            int y0 = Mathf.RoundToInt(v0 * _texture.height);
            int y1 = Mathf.RoundToInt(v1 * _texture.height);
            int w = Mathf.Max(1, x1 - x0);
            int h = Mathf.Max(1, y1 - y0);
            sr.sprite = Sprite.Create(_texture, new Rect(x0, y0, w, h), new Vector2(0.5f, 0.5f), 100f);
            skin.transform.localScale = new Vector3(100f / w, 100f / h, 1f);
        }
        else
        {
            sr.sprite = _fallback;
        }

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

    /// <summary>
    /// Returns the paddle-center world x that lines up the middle of the longest
    /// run of contiguous intact columns with <paramref name="worldX"/>. A column
    /// counts as intact when at least one of its (top/bottom) cubes is alive.
    /// This makes the AI block with the center of its biggest solid piece instead
    /// of an edge, and degrades to the plain paddle center while it is undamaged.
    /// Falls back to worldX when every segment is destroyed.
    /// </summary>
    public float SegmentAlignedCenter(float worldX)
    {
        // Collapse the two cube rows into one alive/dead flag per column.
        List<float> xs = new List<float>();
        List<bool> alive = new List<bool>();
        foreach (PaddleSegment segment in _segments)
        {
            float x = segment.transform.localPosition.x;
            int idx = -1;
            for (int i = 0; i < xs.Count; i++)
            {
                if (Mathf.Abs(xs[i] - x) < 1e-4f)
                {
                    idx = i;
                    break;
                }
            }
            if (idx < 0)
            {
                xs.Add(x);
                alive.Add(false);
                idx = xs.Count - 1;
            }
            if (!segment.IsDestroyed)
            {
                alive[idx] = true;
            }
        }

        int n = xs.Count;
        // Sort columns left-to-right so "contiguous" means adjacent in the list.
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (xs[j] < xs[i])
                {
                    (xs[i], xs[j]) = (xs[j], xs[i]);
                    (alive[i], alive[j]) = (alive[j], alive[i]);
                }
            }
        }

        // Longest contiguous run of intact columns.
        int bestStart = -1;
        int bestLen = 0;
        int curStart = 0;
        int curLen = 0;
        for (int i = 0; i < n; i++)
        {
            if (alive[i])
            {
                if (curLen == 0)
                {
                    curStart = i;
                }
                curLen++;
                if (curLen > bestLen)
                {
                    bestLen = curLen;
                    bestStart = curStart;
                }
            }
            else
            {
                curLen = 0;
            }
        }

        if (bestLen == 0)
        {
            return worldX;
        }

        float centerLocalX = (xs[bestStart] + xs[bestStart + bestLen - 1]) * 0.5f;
        float offset = centerLocalX * transform.lossyScale.x;
        return worldX - offset;
    }
}
