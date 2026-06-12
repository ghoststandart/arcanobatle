using UnityEngine;

/// <summary>
/// Moves a group of child bricks horizontally as a single formation and
/// reverses the whole group when its outer edge touches a side wall.
/// Individual bricks still take hits and die on their own; the cluster
/// destroys itself when no bricks remain.
/// </summary>
public class BrickCluster : MonoBehaviour
{
    public float speed = 2f;
    public Vector2 direction = Vector2.right;

    private float _leftBound;
    private float _rightBound;

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
    }

    void Update()
    {
        Brick[] bricks = GetComponentsInChildren<Brick>();
        if (bricks.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        foreach (Brick brick in bricks)
        {
            float half = brick.transform.localScale.x / 2f;
            minX = Mathf.Min(minX, brick.transform.position.x - half);
            maxX = Mathf.Max(maxX, brick.transform.position.x + half);
        }

        if (minX <= _leftBound && direction.x < 0f)
        {
            direction = Vector2.right;
        }
        else if (maxX >= _rightBound && direction.x > 0f)
        {
            direction = Vector2.left;
        }
    }
}
