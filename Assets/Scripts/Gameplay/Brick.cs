using UnityEngine;

public class Brick : MonoBehaviour
{
    public int health = 1;
    public float speed = 2f;
    public Vector2 direction = Vector2.right;
    public float powerUpDropChance = 0.5f;

    [Tooltip("False when the brick is part of a BrickCluster — the cluster moves and bounces the whole formation instead.")]
    public bool selfMove = true;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private float _leftBound;
    private float _rightBound;

    private static Sprite _powerUpSprite;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
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

        PowerUpType type = Random.value > 0.5f ? PowerUpType.RepairPaddle : PowerUpType.SpeedBoost;

        GameObject go = new GameObject("PowerUp");
        go.transform.position = transform.position;
        go.transform.localScale = new Vector3(0.33f, 0.33f, 1f);

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

    void UpdateColor()
    {
        if (health >= 3)
        {
            _sr.color = new Color(0.85f, 0.6f, 0.1f);
        }
        else if (health == 2)
        {
            _sr.color = new Color(1f, 0.85f, 0.2f);
        }
        else
        {
            _sr.color = new Color(1f, 1f, 0.55f);
        }
    }
}
