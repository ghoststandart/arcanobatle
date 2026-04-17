using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    public float minInterval = 3f;
    public float maxInterval = 7f;
    public float yRangeHalf = 2.5f;
    public float brickSize = 0.4f;
    public float brickSpeed = 2f;
    public int minHealth = 1;
    public int maxHealth = 3;
    public float firstSpawnDelay = 5f;

    private Sprite _whiteSquare;
    private float _nextSpawnTime;

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

        float leftInner, rightInner;
        float halfSize = brickSize / 2f;
        var wallLeft = GameObject.Find("WallLeft");
        var wallRight = GameObject.Find("WallRight");
        if (wallLeft != null && wallRight != null)
        {
            leftInner = wallLeft.transform.position.x + wallLeft.transform.localScale.x / 2f + halfSize;
            rightInner = wallRight.transform.position.x - wallRight.transform.localScale.x / 2f - halfSize;
        }
        else
        {
            float camHalfW = Camera.main.orthographicSize * Camera.main.aspect;
            leftInner = -camHalfW + halfSize;
            rightInner = camHalfW - halfSize;
        }

        float startX = goingRight ? leftInner : rightInner;
        float y = Random.Range(-yRangeHalf, yRangeHalf);

        GameObject go = new GameObject("Brick");
        go.transform.position = new Vector3(startX, y, 0f);
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
        brick.speed = brickSpeed;
        brick.direction = goingRight ? Vector2.right : Vector2.left;
    }
}
