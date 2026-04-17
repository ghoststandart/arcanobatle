using UnityEngine;

public class SpeedDisplay : MonoBehaviour
{
    public Vector3 worldPosition = new Vector3(7f, 0f, 0f);
    public int fontSize = 22;

    private GUIStyle _style;
    private Rigidbody2D _ballRb;

    void Start()
    {
        var ball = GameObject.Find("Ball");
        if (ball != null)
        {
            _ballRb = ball.GetComponent<Rigidbody2D>();
        }
    }

    void OnGUI()
    {
        if (_ballRb == null)
        {
            return;
        }

        if (_style == null)
        {
            _style = new GUIStyle();
            _style.alignment = TextAnchor.MiddleCenter;
            _style.fontStyle = FontStyle.Bold;
            _style.normal.textColor = Color.white;
        }
        _style.fontSize = fontSize;

        float speed = _ballRb.linearVelocity.magnitude;
        string text = $"Speed\n{speed:F1}";

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        float width = 160f;
        float height = fontSize * 3f;
        Rect rect = new Rect(screenPos.x - width / 2f, Screen.height - screenPos.y - height / 2f, width, height);
        GUI.Label(rect, text, _style);
    }
}
