using UnityEngine;

public class GameSetup : MonoBehaviour
{
    private Sprite _whiteSquare;

    void Awake()
    {
        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 10f;
        cam.backgroundColor = Color.black;
        cam.transform.position = new Vector3(0f, 0f, -10f);

        CreateWhiteSprite();
        AssignSprites();
        SetupPaddleColliders();
        SetupWallColliders();
        SetupBallPhysics();
        LinkPaddles();
    }

    void CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _whiteSquare = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void AssignSprites()
    {
        foreach (var sr in FindObjectsByType<SpriteRenderer>())
        {
            if (sr.sprite == null)
                sr.sprite = _whiteSquare;
        }
    }

    void SetupPaddleColliders()
    {
        int ellipseSegments = 32;
        Vector2[] ellipsePoints = new Vector2[ellipseSegments];
        for (int i = 0; i < ellipseSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / ellipseSegments;
            ellipsePoints[i] = new Vector2(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f);
        }

        foreach (string paddleName in new[] { "Paddle", "PaddleTop" })
        {
            var go = GameObject.Find(paddleName);
            if (go == null) continue;

            var box = go.GetComponent<BoxCollider2D>();
            if (box != null) Destroy(box);

            var capsule = go.GetComponent<CapsuleCollider2D>();
            if (capsule != null) Destroy(capsule);

            var polygon = go.GetComponent<PolygonCollider2D>();
            if (polygon == null) polygon = go.AddComponent<PolygonCollider2D>();
            polygon.points = ellipsePoints;
            polygon.offset = Vector2.zero;

            if (go.GetComponent<LineRenderer>() == null)
                go.AddComponent<LineRenderer>();
            if (go.GetComponent<EllipseOutlineDrawer>() == null)
                go.AddComponent<EllipseOutlineDrawer>();
        }
    }

    void SetupWallColliders()
    {
        foreach (string wallName in new[] { "WallLeft", "WallRight" })
        {
            var go = GameObject.Find(wallName);
            if (go == null) continue;

            var box = go.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = Vector2.one;
                box.offset = Vector2.zero;
            }
        }
    }

    void SetupBallPhysics()
    {
        var ball = GameObject.Find("Ball");
        if (ball == null) return;

        var rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
        }

        var col = ball.GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.radius = 0.5f;
            col.offset = Vector2.zero;

            PhysicsMaterial2D mat = new PhysicsMaterial2D("BallBounce");
            mat.bounciness = 1f;
            mat.friction = 0f;
            col.sharedMaterial = mat;
        }
    }

    void LinkPaddles()
    {
        var paddle = GameObject.Find("Paddle");
        var paddleTop = GameObject.Find("PaddleTop");
        if (paddle != null && paddleTop != null)
        {
            var controller = paddle.GetComponent<PaddleController>();
            if (controller != null)
                controller.mirrorPaddle = paddleTop.transform;
        }
    }
}
