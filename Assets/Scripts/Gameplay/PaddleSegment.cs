using UnityEngine;

/// <summary>
/// One cube of a segmented paddle. Takes a hit each time the ball touches it,
/// fades color as health drops and turns off (renderer + collider) at zero.
/// The GameObject is kept alive so PaddleHealth can revive it later.
/// </summary>
public class PaddleSegment : MonoBehaviour
{
    public int maxHealth = 2;
    public int health = 2;
    public PaddleHealth owner;

    private SpriteRenderer _sr;
    private BoxCollider2D _col;

    private static readonly Color FullColor = Color.white;
    private static readonly Color DamagedColor = new Color(1f, 0.25f, 0.2f);

    public bool IsDestroyed
    {
        get { return health <= 0; }
    }

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<BoxCollider2D>();
    }

    public void Init(int max)
    {
        maxHealth = max;
        health = max;
        UpdateVisual();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name != "Ball")
        {
            return;
        }

        Damage(1);
        RedirectBall(col.rigidbody);
    }

    // Arkanoid-style aiming: the bounce angle depends on where the ball hit
    // the paddle — vertical at the center, up to maxBounceAngle at the edges.
    void RedirectBall(Rigidbody2D ballRb)
    {
        if (ballRb == null || owner == null)
        {
            return;
        }

        Transform paddle = owner.transform;
        float halfWidth = paddle.localScale.x / 2f;
        if (halfWidth <= 0f)
        {
            return;
        }

        float offset = Mathf.Clamp((ballRb.transform.position.x - paddle.position.x) / halfWidth, -1f, 1f);
        float angle = offset * owner.maxBounceAngle * Mathf.Deg2Rad;

        float awayY = Mathf.Sign(ballRb.transform.position.y - paddle.position.y);
        if (awayY == 0f)
        {
            awayY = 1f;
        }

        Vector2 dir = new Vector2(Mathf.Sin(angle), awayY * Mathf.Cos(angle));
        ballRb.linearVelocity = dir * ballRb.linearVelocity.magnitude;
    }

    public void Damage(int amount)
    {
        health = Mathf.Max(health - amount, 0);
        UpdateVisual();
    }

    public void Heal(int amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
        UpdateVisual();
    }

    void UpdateVisual()
    {
        bool alive = health > 0;
        if (_sr != null)
        {
            _sr.enabled = alive;
            if (alive)
            {
                float t = maxHealth > 1 ? (health - 1f) / (maxHealth - 1f) : 1f;
                _sr.color = Color.Lerp(DamagedColor, FullColor, t);
            }
        }
        if (_col != null)
        {
            _col.enabled = alive;
        }
    }
}
