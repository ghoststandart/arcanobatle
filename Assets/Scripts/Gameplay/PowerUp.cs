using UnityEngine;

public enum PowerUpType
{
    SpeedBoost,
    RepairPaddle
}

public class PowerUp : MonoBehaviour
{
    public PowerUpType type = PowerUpType.SpeedBoost;
    public float fallSpeed = 2f;
    public float speedBoostAmount = 5f;
    public float speedBoostDuration = 5f;
    public int repairLives = 10;

    private float _topBound;
    private float _bottomBound;
    private Vector2 _direction = Vector2.down;

    /// <summary>Vertical travel direction (up or down). Used by the AI to pick catchable drops.</summary>
    public Vector2 Direction
    {
        get { return _direction; }
    }

    void Start()
    {
        float camSize = Camera.main.orthographicSize;
        _topBound = camSize + 1f;
        _bottomBound = -camSize - 1f;
        _direction = Random.value > 0.5f ? Vector2.down : Vector2.up;
        ApplyIcon();
    }

    // Replaces the placeholder coloured square with this power-up's icon
    // (Resources/Powerups/{speed,repair}). Falls back silently to whatever the
    // spawner set if the texture is missing.
    void ApplyIcon()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            return;
        }

        string resource = type == PowerUpType.SpeedBoost ? "Powerups/speed" : "Powerups/repair";
        Texture2D tex = Resources.Load<Texture2D>(resource);
        if (tex == null)
        {
            return;
        }

        float pixelsPerUnit = Mathf.Max(tex.width, tex.height);
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        sr.color = Color.white;
    }

    void Update()
    {
        transform.position += (Vector3)(_direction * fallSpeed * Time.deltaTime);

        if (transform.position.y < _bottomBound || transform.position.y > _topBound)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // The paddle is made of segment cubes, so the trigger touches a child
        // collider — look for the paddle root component in the parents.
        var paddleHealth = other.GetComponentInParent<PaddleHealth>();
        if (paddleHealth == null)
        {
            return;
        }

        Apply(paddleHealth);
        Destroy(gameObject);
    }

    void Apply(PaddleHealth catcher)
    {
        switch (type)
        {
            case PowerUpType.SpeedBoost:
            {
                var ball = GameObject.Find("Ball");
                if (ball != null)
                {
                    var ballCtrl = ball.GetComponent<BallController>();
                    if (ballCtrl != null)
                    {
                        ballCtrl.ApplySpeedBoost(speedBoostAmount, speedBoostDuration);
                    }
                }
                break;
            }
            case PowerUpType.RepairPaddle:
            {
                catcher.RestoreRandom(repairLives);
                break;
            }
        }
    }
}
