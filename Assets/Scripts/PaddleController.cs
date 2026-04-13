using UnityEngine;

public class PaddleController : MonoBehaviour
{
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
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            if (worldPos.y < transform.position.y)
            {
                _isDragging = true;
                _dragOffsetX = transform.position.x - worldPos.x;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            Vector3 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            float targetX = Mathf.Clamp(worldPos.x + _dragOffsetX, _minX, _maxX);
            Vector3 pos = transform.position;
            pos.x = targetX;
            transform.position = pos;
        }
    }
}
