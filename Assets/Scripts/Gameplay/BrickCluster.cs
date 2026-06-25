using UnityEngine;

/// <summary>
/// Moves a group of child bricks as a single formation and reverses it at the
/// arena edges. By default it only travels horizontally and bounces off the
/// side walls. When <see cref="fallSpeed"/> is set it also drifts vertically,
/// bouncing off bounds placed just inside the paddles so it never touches them.
/// Individual bricks still take hits and die on their own; the cluster destroys
/// itself when no bricks remain.
/// </summary>
public class BrickCluster : MonoBehaviour
{
    public float speed = 2f;
    public Vector2 direction = Vector2.right;

    [Tooltip("Downward drift speed. 0 = horizontal only. When > 0 the cluster also bounces vertically inside the paddles so it never hits them.")]
    public float fallSpeed = 0f;

    [Tooltip("Fraction of the paddle-to-paddle gap the vertical bounce uses, centred on the arena. 1 = full range (small cubes), 0.5 = half the up/down travel (formations).")]
    [Range(0f, 1f)]
    public float verticalRangeFraction = 1f;

    [Tooltip("Gap kept between the vertical bounce bounds and the paddles.")]
    public float paddleClearance = 0.5f;

    private float _leftBound;
    private float _rightBound;
    private float _topBound;
    private float _bottomBound;
    private int _vDir = -1; // start drifting down

    void Start()
    {
        var wallLeft = GameObject.Find("WallLeft");
        var wallRight = GameObject.Find("WallRight");
        if (wallLeft != null && wallRight != null)
        {
            _leftBound = wallLeft.transform.position.x + wallLeft.transform.localScale.x / 2f;
            _rightBound = wallRight.transform.position.x - wallRight.transform.localScale.x / 2f;
        }
        else
        {
            float camHalfW = Camera.main.orthographicSize * Camera.main.aspect;
            _leftBound = -camHalfW;
            _rightBound = camHalfW;
        }

        float camHalfH = Camera.main.orthographicSize;
        _topBound = camHalfH;
        _bottomBound = -camHalfH;
        if (fallSpeed > 0f)
        {
            // Start drifting up or down at random instead of always downward.
            _vDir = Random.value < 0.5f ? 1 : -1;

            var paddleTop = GameObject.Find("PaddleTop");
            var paddleBottom = GameObject.Find("Paddle");
            if (paddleTop != null)
            {
                _topBound = paddleTop.transform.position.y - paddleTop.transform.localScale.y - paddleClearance;
            }
            if (paddleBottom != null)
            {
                _bottomBound = paddleBottom.transform.position.y + paddleBottom.transform.localScale.y + paddleClearance;
            }

            // Shrink the bounce band symmetrically around its centre so formations
            // can travel a smaller vertical distance than the small cubes.
            float center = (_topBound + _bottomBound) * 0.5f;
            float halfRange = (_topBound - _bottomBound) * 0.5f * verticalRangeFraction;
            _topBound = center + halfRange;
            _bottomBound = center - halfRange;
        }
    }

    void Update()
    {
        Brick[] bricks = GetComponentsInChildren<Brick>();
        if (bricks.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 velocity = (Vector3)(direction.normalized * speed);
        if (fallSpeed > 0f)
        {
            velocity += Vector3.up * (_vDir * fallSpeed);
        }
        transform.position += velocity * Time.deltaTime;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        foreach (Brick brick in bricks)
        {
            var box = brick.GetComponent<BoxCollider2D>();
            // The brick transform is unscaled (its visual lives on a scaled child),
            // so the real cell size comes from the collider, not transform.localScale.
            float halfW = box != null ? box.size.x * brick.transform.lossyScale.x / 2f : 0f;
            float halfH = box != null ? box.size.y * brick.transform.lossyScale.y / 2f : 0f;
            minX = Mathf.Min(minX, brick.transform.position.x - halfW);
            maxX = Mathf.Max(maxX, brick.transform.position.x + halfW);
            minY = Mathf.Min(minY, brick.transform.position.y - halfH);
            maxY = Mathf.Max(maxY, brick.transform.position.y + halfH);
        }

        if (minX <= _leftBound && direction.x < 0f)
        {
            direction = Vector2.right;
        }
        else if (maxX >= _rightBound && direction.x > 0f)
        {
            direction = Vector2.left;
        }

        if (fallSpeed > 0f)
        {
            if (minY <= _bottomBound && _vDir < 0)
            {
                _vDir = 1;
            }
            else if (maxY >= _topBound && _vDir > 0)
            {
                _vDir = -1;
            }
        }
    }
}
