using UnityEngine;

public class AIPaddleController : MonoBehaviour
{
    public float maxMoveSpeed = 12f;
    public float minReactionDelay = 0.07f;
    public float maxReactionDelay = 0.3f;
    public float reactionJitterInterval = 1.5f;

    private Camera _cam;
    private Transform _ball;
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
        _paddleHalfWidth = transform.localScale.x / 2f;
        float camHalfWidth = _cam.orthographicSize * _cam.aspect;
        _minX = -camHalfWidth + _paddleHalfWidth;
        _maxX = camHalfWidth - _paddleHalfWidth;

        var ballGO = GameObject.Find("Ball");
        if (ballGO != null)
        {
            _ball = ballGO.transform;
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

        _bufferedTargetX = _ball.position.x;
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
}
