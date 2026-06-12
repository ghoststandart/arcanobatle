using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
    [Tooltip("How much of the paddle is allowed to slide past the screen edge. 0 = fully on-screen, 0.5 = half off-screen, 1 = paddle center can reach the edge.")]
    [Range(0f, 1f)]
    public float edgeOverhang = 0.5f;

    [Tooltip("Upper world-Y limit of the drag-capture zone for the player's paddle. Touches above this line do nothing; below — drag the paddle. 0 = screen middle.")]
    public float dragZoneMaxY = 0f;

    public Transform mirrorPaddle;

    private Camera _cam;
    private bool _isDragging;
    private float _dragOffsetX;
    private float _minX;
    private float _maxX;
    private float _paddleHalfWidth;

    void Start()
    {
        _cam = Camera.main;
        CalculateBounds();
    }

    void CalculateBounds()
    {
        _paddleHalfWidth = transform.localScale.x / 2f;
        float camHalfWidth = _cam.orthographicSize * _cam.aspect;
        float allowed = _paddleHalfWidth * (1f - edgeOverhang);
        _minX = -camHalfWidth + allowed;
        _maxX = camHalfWidth - allowed;
    }

    void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null)
        {
            return;
        }

        Vector2 screenPos = pointer.position.ReadValue();
        Vector3 worldPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));

        if (pointer.press.wasPressedThisFrame)
        {
            if (worldPos.y < dragZoneMaxY)
            {
                _isDragging = true;
                _dragOffsetX = transform.position.x - worldPos.x;
            }
        }

        if (pointer.press.wasReleasedThisFrame)
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            float targetX = Mathf.Clamp(worldPos.x + _dragOffsetX, _minX, _maxX);
            Vector3 pos = transform.position;
            pos.x = targetX;
            transform.position = pos;

            if (mirrorPaddle != null)
            {
                Vector3 mirrorPos = mirrorPaddle.position;
                mirrorPos.x = targetX;
                mirrorPaddle.position = mirrorPos;
            }
        }
    }
}
