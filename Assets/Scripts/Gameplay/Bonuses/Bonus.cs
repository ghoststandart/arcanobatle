using UnityEngine;

/// <summary>
/// Processor for every bonus: owns the GameObject, its kinematic trigger body,
/// vertical movement, paddle contact and despawn. All behaviour-specific bits
/// (look, speed, effect) live in the <see cref="IBonus"/> it carries, so adding a
/// bonus never touches this class.
/// </summary>
public class Bonus : MonoBehaviour
{
    private IBonus _def;
    private Vector2 _direction = Vector2.down;
    private float _topBound;
    private float _bottomBound;
    private int _hits;

    /// <summary>Travel direction (up/down). Used by the AI to pick catchable drops.</summary>
    public Vector2 Direction { get { return _direction; } }

    /// <summary>True when this bonus is caught by a paddle (vs. flying through it).</summary>
    public bool Catchable { get { return _def != null && !_def.PiercesPaddle; } }

    /// <summary>Creates a bonus GameObject for the given definition at <paramref name="pos"/>.</summary>
    public static Bonus Spawn(IBonus def, Vector3 pos)
    {
        GameObject go = new GameObject("Bonus");
        go.transform.position = pos;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
        // Bonuses (especially the fast bullet) sweep so they can't skip the paddle.
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;

        var bonus = go.AddComponent<Bonus>();
        bonus._def = def;
        return bonus;
    }

    /// <summary>
    /// Adds a sprite icon loaded from a Resources path to a bonus GameObject and
    /// sizes it. Shared by the bonus definitions so they don't repeat the boilerplate.
    /// </summary>
    public static SpriteRenderer AddIcon(GameObject go, string resource, float scale, int sortingOrder)
    {
        go.transform.localScale = new Vector3(scale, scale, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        Texture2D tex = Resources.Load<Texture2D>(resource);
        if (tex != null)
        {
            float pixelsPerUnit = Mathf.Max(tex.width, tex.height);
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
        sr.sortingOrder = sortingOrder;
        return sr;
    }

    void Start()
    {
        float camSize = Camera.main.orthographicSize;
        _topBound = camSize + 2f;
        _bottomBound = -camSize - 2f;
        _direction = Random.value > 0.5f ? Vector2.up : Vector2.down;

        if (_def != null)
        {
            _def.SetupVisual(gameObject, _direction);
        }
    }

    void Update()
    {
        float speed = _def != null ? _def.Speed : 0f;
        transform.position += (Vector3)(_direction * speed * Time.deltaTime);

        float y = transform.position.y;
        if (y > _topBound || y < _bottomBound)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_def == null)
        {
            return;
        }

        if (_def.PiercesPaddle)
        {
            if (_hits >= _def.MaxPaddleHits)
            {
                return;
            }
            var segment = other.GetComponentInParent<PaddleSegment>();
            if (segment == null || segment.IsDestroyed)
            {
                return;
            }
            _def.Apply(new BonusContext { bonus = this, segment = segment });
            _hits++;
        }
        else
        {
            var paddle = other.GetComponentInParent<PaddleHealth>();
            if (paddle == null)
            {
                return;
            }
            _def.Apply(new BonusContext { bonus = this, paddle = paddle });
            Destroy(gameObject);
        }
    }
}
