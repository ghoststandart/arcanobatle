using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 8f;

    private Rigidbody2D _rb;
    private float _topBound;
    private float _bottomBound;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        float camSize = Camera.main.orthographicSize;
        _topBound = camSize + 1f;
        _bottomBound = -camSize - 1f;
        Launch();
    }

    void Update()
    {
        if (transform.position.y > _topBound)
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddBottomScore();
            Respawn();
        }
        else if (transform.position.y < _bottomBound)
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddTopScore();
            Respawn();
        }

        // Keep constant speed regardless of physics drift
        if (_rb.linearVelocity.sqrMagnitude > 0.01f)
            _rb.linearVelocity = _rb.linearVelocity.normalized * speed;
    }

    void Launch()
    {
        float angle = Random.Range(30f, 60f);
        if (Random.value > 0.5f) angle = -angle;
        float dirY = Random.value > 0.5f ? 1f : -1f;

        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Sin(rad), dirY * Mathf.Cos(rad)).normalized;
        _rb.linearVelocity = dir * speed;
    }

    void Respawn()
    {
        transform.position = Vector3.zero;
        _rb.linearVelocity = Vector2.zero;
        Launch();
    }
}
