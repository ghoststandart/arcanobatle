using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    public float minInterval = 3f;
    public float maxInterval = 7f;
    public float yRangeHalf = 2.5f;
    public float brickSize = 0.2f;
    public float brickSpeed = 2f;
    public int minHealth = 1;
    public int maxHealth = 3;
    public float firstSpawnDelay = 5f;

    private Sprite _whiteSquare;
    private float _nextSpawnTime;

    private static readonly Vector2Int[][] Shapes =
    {
        // Square 2x2
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
        // Horizontal line
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
        // Vertical line
        new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) },
        // L
        new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 0) },
        // T
        new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 0) },
        // Plus
        new[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) },
    };

    void Start()
    {
        CreateSprite();
        _nextSpawnTime = Time.time + firstSpawnDelay;
    }

    void Update()
    {
        if (Time.time >= _nextSpawnTime)
        {
            Spawn();
            ScheduleNext();
        }
    }

    void CreateSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _whiteSquare = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void ScheduleNext()
    {
        _nextSpawnTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    void Spawn()
    {
        bool goingRight = Random.value > 0.5f;
        Vector2Int[] shape = Shapes[Random.Range(0, Shapes.Length)];

        int minCol = int.MaxValue;
        int maxCol = int.MinValue;
        int minRow = int.MaxValue;
        int maxRow = int.MinValue;
        foreach (Vector2Int cell in shape)
        {
            minCol = Mathf.Min(minCol, cell.x);
            maxCol = Mathf.Max(maxCol, cell.x);
            minRow = Mathf.Min(minRow, cell.y);
            maxRow = Mathf.Max(maxRow, cell.y);
        }

        float shapeHalfWidth = (maxCol - minCol + 1) * brickSize / 2f;
        float shapeHalfHeight = (maxRow - minRow + 1) * brickSize / 2f;

        float leftInner, rightInner;
        var wallLeft = GameObject.Find("WallLeft");
        var wallRight = GameObject.Find("WallRight");
        if (wallLeft != null && wallRight != null)
        {
            leftInner = wallLeft.transform.position.x + wallLeft.transform.localScale.x / 2f + shapeHalfWidth;
            rightInner = wallRight.transform.position.x - wallRight.transform.localScale.x / 2f - shapeHalfWidth;
        }
        else
        {
            float camHalfW = Camera.main.orthographicSize * Camera.main.aspect;
            leftInner = -camHalfW + shapeHalfWidth;
            rightInner = camHalfW - shapeHalfWidth;
        }

        float startX = goingRight ? leftInner : rightInner;
        float yMax = Mathf.Max(0f, yRangeHalf - shapeHalfHeight);
        float y = Random.Range(-yMax, yMax);

        GameObject clusterGO = new GameObject("BrickCluster");
        clusterGO.transform.position = new Vector3(startX, y, 0f);

        var cluster = clusterGO.AddComponent<BrickCluster>();
        cluster.speed = brickSpeed;
        cluster.direction = goingRight ? Vector2.right : Vector2.left;

        float centerCol = (minCol + maxCol) / 2f;
        float centerRow = (minRow + maxRow) / 2f;
        foreach (Vector2Int cell in shape)
        {
            GameObject go = new GameObject("Brick");
            go.transform.SetParent(clusterGO.transform);
            go.transform.localPosition = new Vector3((cell.x - centerCol) * brickSize, (cell.y - centerRow) * brickSize, 0f);
            go.transform.localScale = new Vector3(brickSize, brickSize, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _whiteSquare;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            var brick = go.AddComponent<Brick>();
            brick.health = Random.Range(minHealth, maxHealth + 1);
            brick.selfMove = false;
        }
    }
}
