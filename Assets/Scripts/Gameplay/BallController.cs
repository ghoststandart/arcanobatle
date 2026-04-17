using UnityEngine;

public class BallController : MonoBehaviour
{
    public float startSpeed = 8f;
    public float maxSpeed = 20f;
    public float accelerationPerSecond = 0.5f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private float _topBound;
    private float _bottomBound;
    private float _currentSpeed;
    private float _speedBonus;
    private float _bonusExpireTime;
    private bool _boostActive;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
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
        if (_rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * effectiveSpeed;
        }
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
        _rb.linearVelocity = dir * (_currentSpeed + _speedBonus);
    }

    void Respawn()
    {
        transform.position = Vector3.zero;
        _rb.linearVelocity = Vector2.zero;
        _currentSpeed = startSpeed;
        _speedBonus = 0f;
        Launch();
    }

    public void ApplySpeedBoost(float amount, float duration)
    {
        _speedBonus = amount;
        _bonusExpireTime = Time.time + duration;
    }
}
