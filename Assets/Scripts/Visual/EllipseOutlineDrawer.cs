using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EllipseOutlineDrawer : MonoBehaviour
{
    public int segments = 64;
    public Color color = Color.red;
    public float thickness = 0.05f;

    private LineRenderer _lr;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.useWorldSpace = true;
        _lr.loop = true;
        _lr.startColor = color;
        _lr.endColor = color;
        _lr.startWidth = thickness;
        _lr.endWidth = thickness;
        _lr.material = new Material(Shader.Find("Sprites/Default"));
        _lr.sortingOrder = 10;
    }

    void LateUpdate()
    {
        UpdateOutline();
    }

    void UpdateOutline()
    {
        float a = transform.localScale.x / 2f;
        float b = transform.localScale.y / 2f;
        Vector3 center = transform.position;

        Vector3[] points = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            points[i] = center + new Vector3(Mathf.Cos(angle) * a, Mathf.Sin(angle) * b, 0f);
        }

        _lr.positionCount = segments;
        _lr.SetPositions(points);
    }
}
