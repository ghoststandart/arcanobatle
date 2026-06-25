using UnityEngine;

/// <summary>
/// A fast projectile that drops from a brick and flies straight up or down at
/// roughly the ball's top speed, leaving a trail. It passes straight through the
/// paddle (its collider is a trigger), damaging up to <see cref="maxHits"/> of the
/// paddle's segment cubes by one each, then flies off-screen and despawns.
/// </summary>
public class Bullet : MonoBehaviour
{
    public float speed = 30f;
    public Vector2 direction = Vector2.up;
    public int maxHits = 2;

    private int _hits;
    private float _topBound;
    private float _bottomBound;

    void Start()
    {
        float camSize = Camera.main.orthographicSize;
        _topBound = camSize + 2f;
        _bottomBound = -camSize - 2f;

        // The drop texture leads with its thick (round) end pointing up, so flip
        // it when the bullet flies downward.
        if (direction.y < 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
    }

    void Update()
    {
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
        float y = transform.position.y;
        if (y > _topBound || y < _bottomBound)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_hits >= maxHits)
        {
            return;
        }

        var segment = other.GetComponentInParent<PaddleSegment>();
        if (segment == null || segment.IsDestroyed)
        {
            return;
        }

        // Damage the cube by one and keep flying through.
        segment.Damage(1);
        _hits++;
    }
}
