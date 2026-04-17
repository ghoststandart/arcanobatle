using UnityEngine;

public enum PowerUpType
{
    SpeedBoost
}

public class PowerUp : MonoBehaviour
{
    public PowerUpType type = PowerUpType.SpeedBoost;
    public float fallSpeed = 2f;
    public float speedBoostAmount = 5f;
    public float speedBoostDuration = 5f;

    private float _topBound;
    private float _bottomBound;
    private Vector2 _direction = Vector2.down;

    void Start()
    {
        float camSize = Camera.main.orthographicSize;
        _topBound = camSize + 1f;
        _bottomBound = -camSize - 1f;
        _direction = Random.value > 0.5f ? Vector2.down : Vector2.up;
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
        if (other.gameObject.name != "Paddle" && other.gameObject.name != "PaddleTop")
        {
            return;
        }

        Apply();
        Destroy(gameObject);
    }

    void Apply()
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
        }
    }
}
