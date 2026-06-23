using UnityEngine;

public class BallController : MonoBehaviour
{
    public float startSpeed = 4f;
    public float maxSpeed = 30f;
    public float accelerationPerSecond = 0.25f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private float _topBound;
    private float _bottomBound;
    private float _currentSpeed;
    private float _speedBonus;
    private float _bonusExpireTime;
    private bool _boostActive;
    private Vector2 _lastDir = Vector2.up;

    [Tooltip("Minimum angle (degrees) the ball's path keeps from horizontal, so it never flies flat between the side walls and stalls the rally.")]
    public float minAngleFromHorizontal = 10f;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null)
        {
            _sr.color = Color.white;
        }
        _boostActive = false;
        float camSize = Camera.main.orthographicSize;
        _topBound = camSize + 1f;
        _bottomBound = -camSize - 1f;
        _currentSpeed = startSpeed;
        Launch();
    }

    void Update()
    {
        if (transform.position.y > _topBound)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddBottomScore();
            }
            Respawn();
        }
        else if (transform.position.y < _bottomBound)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddTopScore();
            }
            Respawn();
        }

        if (_currentSpeed < maxSpeed)
        {
            _currentSpeed = Mathf.Min(_currentSpeed + accelerationPerSecond * Time.deltaTime, maxSpeed);
        }

        if (_speedBonus > 0f && Time.time >= _bonusExpireTime)
        {
            _speedBonus = 0f;
        }

        bool boostNow = _speedBonus > 0f;
        if (boostNow != _boostActive)
        {
            _boostActive = boostNow;
            if (_sr != null)
            {
                _sr.color = _boostActive ? Color.red : Color.white;
            }
        }

        float effectiveSpeed = _currentSpeed + _speedBonus;
        _rb.linearVelocity = MaintainVelocity(_rb.linearVelocity, effectiveSpeed);
    }

    // The ball is supposed to travel at a constant speed; a glancing or two-surface
    // bounce can leave its velocity near zero, and normalizing a zero vector keeps
    // it stuck. So recover the heading from the last known one and always re-apply
    // full speed, keeping a minimum vertical component so it can't crawl horizontally.
    Vector2 MaintainVelocity(Vector2 velocity, float speed)
    {
        Vector2 dir;
        if (velocity.sqrMagnitude > 0.0001f)
        {
            dir = velocity.normalized;
        }
        else
        {
            // Fully stopped — head back the way it came so it doesn't drive into
            // whatever just stopped it.
            dir = -_lastDir;
            if (dir.sqrMagnitude < 0.0001f)
            {
                dir = Vector2.up;
            }
            dir.Normalize();
        }

        float minY = Mathf.Sin(minAngleFromHorizontal * Mathf.Deg2Rad);
        if (Mathf.Abs(dir.y) < minY)
        {
            float signY = dir.y >= 0f ? 1f : -1f;
            float signX = dir.x >= 0f ? 1f : -1f;
            float newX = signX * Mathf.Sqrt(Mathf.Max(0f, 1f - minY * minY));
            dir = new Vector2(newX, signY * minY);
        }

        _lastDir = dir;
        return dir * speed;
    }

    void Launch()
    {
        float angle = Random.Range(30f, 60f);
        if (Random.value > 0.5f)
        {
            angle = -angle;
        }
        float dirY = Random.value > 0.5f ? 1f : -1f;

        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Sin(rad), dirY * Mathf.Cos(rad)).normalized;
        _lastDir = dir;
        _rb.linearVelocity = dir * (_currentSpeed + _speedBonus);
    }

    void Respawn()
    {
        transform.position = Vector3.zero;
        _rb.linearVelocity = Vector2.zero;
        _currentSpeed = startSpeed;
        _speedBonus = 0f;
        _boostActive = false;
        if (_sr != null)
        {
            _sr.color = Color.white;
        }
        Launch();
    }

    public void ApplySpeedBoost(float amount, float duration)
    {
        _speedBonus = amount;
        _bonusExpireTime = Time.time + duration;
    }
}
