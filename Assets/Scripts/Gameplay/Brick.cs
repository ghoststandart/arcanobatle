using UnityEngine;

public class Brick : MonoBehaviour
{
    public int health = 1;

    [Tooltip("Highest health this brick was spawned with — used to map current health onto the colour gradient.")]
    public int maxHealth = 2;

    [Tooltip("Tint at full health (multiplies the microbe skin — white keeps its natural colour).")]
    public Color fullHealthColor = Color.white;

    [Tooltip("Tint at one hit from death — the skin shifts towards this warm amber as it takes damage.")]
    public Color lowHealthColor = new Color(1f, 0.72f, 0.38f);

    public float speed = 2f;
    public Vector2 direction = Vector2.right;

    [Tooltip("False when the brick is part of a BrickCluster — the cluster moves and bounces the whole formation instead.")]
    public bool selfMove = true;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private float _leftBound;
    private float _rightBound;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // The visual sprite lives on a child "Skin" object so it can be drawn
        // larger than the brick's collider.
        _sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        UpdateColor();

        if (!selfMove)
        {
            return;
        }

        float halfSize = transform.localScale.x / 2f;
        var wallLeft = GameObject.Find("WallLeft");
        var wallRight = GameObject.Find("WallRight");
        if (wallLeft != null && wallRight != null)
        {
            _leftBound = wallLeft.transform.position.x + wallLeft.transform.localScale.x / 2f + halfSize;
            _rightBound = wallRight.transform.position.x - wallRight.transform.localScale.x / 2f - halfSize;
        }
        else
        {
            float camHalfW = Camera.main.orthographicSize * Camera.main.aspect;
            _leftBound = -camHalfW + halfSize;
            _rightBound = camHalfW - halfSize;
        }

        _rb.linearVelocity = new Vector2(Mathf.Sign(direction.x) * speed, 0f);
    }

    void Update()
    {
        if (!selfMove)
        {
            return;
        }

        Vector3 pos = transform.position;
        Vector2 vel = _rb.linearVelocity;

        if (pos.x <= _leftBound && vel.x < 0f)
        {
            vel.x = speed;
            pos.x = _leftBound;
        }
        else if (pos.x >= _rightBound && vel.x > 0f)
        {
            vel.x = -speed;
            pos.x = _rightBound;
        }

        vel.y = 0f;
        vel.x = Mathf.Sign(vel.x) * speed;

        transform.position = pos;
        _rb.linearVelocity = vel;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name != "Ball")
        {
            return;
        }

        health--;
        if (health <= 0)
        {
            TrySpawnBonus();
            Destroy(gameObject);
        }
        else
        {
            UpdateColor();
        }
    }

    void TrySpawnBonus()
    {
        // Each bonus rolls independently; up to one bullet and one power-up can
        // drop together. Spread multiple drops a little so they don't stack.
        System.Collections.Generic.List<IBonus> drops = BonusDropper.Roll();
        for (int i = 0; i < drops.Count; i++)
        {
            float offsetX = (i - (drops.Count - 1) * 0.5f) * 0.2f;
            Bonus.Spawn(drops[i], transform.position + Vector3.right * offsetX);
        }
    }

    void UpdateColor()
    {
        if (_sr == null)
        {
            return;
        }

        // Map current health onto a smooth gradient: full health -> fullHealthColor,
        // one hit from death -> lowHealthColor. Single-HP bricks read as "low".
        float t = maxHealth > 1 ? Mathf.Clamp01((health - 1f) / (maxHealth - 1f)) : 0f;
        _sr.color = Color.Lerp(lowHealthColor, fullHealthColor, t);
    }
}
