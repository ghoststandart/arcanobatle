using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
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
        _minX = -camHalfWidth + _paddleHalfWidth;
        _maxX = camHalfWidth - _paddleHalfWidth;
    }

    void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 screenPos = pointer.position.ReadValue();
        Vector3 worldPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));

        if (pointer.press.wasPressedThisFrame)
        {
            if (worldPos.y < transform.position.y)
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
