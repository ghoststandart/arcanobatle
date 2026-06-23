using UnityEngine;

public class AIPaddleController : MonoBehaviour
{
    public float maxMoveSpeed = 12f;
    public float minReactionDelay = 0.07f;
    public float maxReactionDelay = 0.3f;
    public float reactionJitterInterval = 1.5f;

    [Tooltip("Matches PaddleController.edgeOverhang for visual symmetry. 0 = paddle fully on-screen, 0.5 = half off-screen.")]
    [Range(0f, 1f)]
    public float edgeOverhang = 0.5f;

    [Tooltip("When the ball is moving away, leave the goal and go grab incoming power-ups.")]
    public bool chasePowerUps = true;

    private Camera _cam;
    private PaddleHealth _health;
    private Transform _ball;
    private Rigidbody2D _ballRb;
    private float _paddleHalfWidth;
    private float _minX;
    private float _maxX;

    private float _currentDelay;
    private float _nextJitterTime;

    private float _targetX;
    private float _bufferedTargetX;
    private float _bufferTimer;

    void Start()
    {
        _cam = Camera.main;
        _health = GetComponent<PaddleHealth>();
        _paddleHalfWidth = transform.localScale.x / 2f;
        float camHalfWidth = _cam.orthographicSize * _cam.aspect;
        float allowed = _paddleHalfWidth * (1f - edgeOverhang);
        _minX = -camHalfWidth + allowed;
        _maxX = camHalfWidth - allowed;

        var ballGO = GameObject.Find("Ball");
        if (ballGO != null)
        {
            _ball = ballGO.transform;
            _ballRb = ballGO.GetComponent<Rigidbody2D>();
        }

        _targetX = transform.position.x;
        _bufferedTargetX = _targetX;
        PickNewDelay();
    }

    void Update()
    {
        if (_ball == null)
        {
            return;
        }

        if (Time.time >= _nextJitterTime)
        {
            PickNewDelay();
        }

        float coverX = ChooseTargetX();
        // Aim the center of the longest intact piece at the ball — not an edge.
        _bufferedTargetX = _health != null
            ? _health.SegmentAlignedCenter(coverX)
            : coverX;
        _bufferTimer += Time.deltaTime;
        if (_bufferTimer >= _currentDelay)
        {
            _targetX = _bufferedTargetX;
            _bufferTimer = 0f;
        }

        float clampedTarget = Mathf.Clamp(_targetX, _minX, _maxX);
        float newX = Mathf.MoveTowards(transform.position.x, clampedTarget, maxMoveSpeed * Time.deltaTime);
        Vector3 pos = transform.position;
        pos.x = newX;
        transform.position = pos;
    }

    void PickNewDelay()
    {
        _currentDelay = Random.Range(minReactionDelay, maxReactionDelay);
        _nextJitterTime = Time.time + reactionJitterInterval;
    }

    // Defend (track the ball) only while it is both heading toward this paddle
    // AND already on this paddle's side; otherwise go chase a power-up that's
    // drifting toward this side. While the ball is on the opponent's half there
    // is time to leave the goal and grab one. Falls back to the ball's x when
    // there's nothing to grab.
    float ChooseTargetX()
    {
        float toMe = Mathf.Sign(transform.position.y);
        bool ballComingToMe = _ballRb != null && _ballRb.linearVelocity.y * toMe > 0f;
        bool ballOnMySide = _ball.position.y * toMe > 0f;
        bool mustDefend = ballComingToMe && ballOnMySide;

        if (mustDefend || !chasePowerUps)
        {
            return _ball.position.x;
        }

        PowerUp target = FindIncomingPowerUp(toMe);
        return target != null ? target.transform.position.x : _ball.position.x;
    }

    PowerUp FindIncomingPowerUp(float toMe)
    {
        PowerUp[] powerUps = FindObjectsByType<PowerUp>(FindObjectsSortMode.None);
        PowerUp best = null;
        float bestDist = float.MaxValue;
        foreach (PowerUp pu in powerUps)
        {
            // Only ones drifting toward this paddle can actually be caught.
            if (pu.Direction.y * toMe <= 0f)
            {
                continue;
            }
            float dist = Mathf.Abs(pu.transform.position.x - transform.position.x);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = pu;
            }
        }
        return best;
    }
}
