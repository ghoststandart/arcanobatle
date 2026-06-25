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
    public float powerUpDropChance = 0.5f;

    [Tooltip("When a drop happens, the chance it's a fast drop-shaped bullet that flies through the paddle instead of a catchable power-up.")]
    public float bulletChance = 0.33f;

    [Tooltip("Bullet flight speed — roughly the ball's top speed.")]
    public float bulletSpeed = 30f;

    [Tooltip("False when the brick is part of a BrickCluster — the cluster moves and bounces the whole formation instead.")]
    public bool selfMove = true;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private float _leftBound;
    private float _rightBound;

    private static Sprite _powerUpSprite;
    private static Material _bulletTrailMat;

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
            TrySpawnPowerUp();
            Destroy(gameObject);
        }
        else
        {
            UpdateColor();
        }
    }

    void TrySpawnPowerUp()
    {
        if (Random.value > powerUpDropChance)
        {
            return;
        }

        if (_powerUpSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _powerUpSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        if (Random.value < bulletChance)
        {
            SpawnBullet();
            return;
        }

        PowerUpType type = Random.value > 0.5f ? PowerUpType.RepairPaddle : PowerUpType.SpeedBoost;

        GameObject go = new GameObject("PowerUp");
        go.transform.position = transform.position;
        // Sized a bit above a small-virus footprint; the icons read smaller than
        // a solid microbe because they're detailed/sparse.
        go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _powerUpSprite;
        sr.color = type == PowerUpType.SpeedBoost ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.4f, 0.9f);
        sr.sortingOrder = 5;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;

        var powerUp = go.AddComponent<PowerUp>();
        powerUp.type = type;
    }

    void SpawnBullet()
    {
        GameObject go = new GameObject("Bullet");
        go.transform.position = transform.position;
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        Texture2D tex = Resources.Load<Texture2D>("Powerups/bullet");
        if (tex != null)
        {
            float pixelsPerUnit = Mathf.Max(tex.width, tex.height);
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
        else
        {
            sr.sprite = _powerUpSprite;
        }
        sr.sortingOrder = 6;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
        // The bullet is fast; sweep so it can't skip over the paddle in one step.
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;

        // Trailing streak behind the bullet.
        if (_bulletTrailMat == null)
        {
            _bulletTrailMat = new Material(Shader.Find("Sprites/Default"));
        }
        var trail = go.AddComponent<TrailRenderer>();
        trail.time = 0.18f;
        trail.startWidth = 0.35f;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.05f;
        trail.numCapVertices = 2;
        trail.material = _bulletTrailMat;
        trail.startColor = new Color(1f, 0.85f, 0.2f, 0.7f);
        trail.endColor = new Color(1f, 0.85f, 0.2f, 0f);
        trail.sortingOrder = 5;

        var bullet = go.AddComponent<Bullet>();
        bullet.direction = Random.value > 0.5f ? Vector2.up : Vector2.down;
        bullet.speed = bulletSpeed;
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
