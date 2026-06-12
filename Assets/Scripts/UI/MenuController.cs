using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public string gameSceneName = "SampleScene";
    public string title = "ArcanoBattle";

    private GUIStyle _titleStyle;
    private GUIStyle _buttonStyle;
    private bool _loading;

    void Awake()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.backgroundColor = Color.black;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }
    }

    // The button is drawn with IMGUI but clicks are handled here through the
    // Input System: IMGUI only receives input from the legacy Input Manager,
    // which is unavailable when Active Input Handling is "Input System Package".
    void Update()
    {
        if (_loading)
        {
            return;
        }

        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame)
        {
            return;
        }

        Vector2 pos = pointer.position.ReadValue();
        // Pointer origin is bottom-left, GUI rects are top-left. Flip Y.
        Vector2 guiPos = new Vector2(pos.x, Screen.height - pos.y);
        if (StartButtonRect().Contains(guiPos))
        {
            _loading = true;
            SceneManager.LoadScene(gameSceneName);
        }
    }

    Rect StartButtonRect()
    {
        float w = Screen.width;
        float h = Screen.height;
        float btnW = w * 0.55f;
        float btnH = h * 0.08f;
        return new Rect(w / 2f - btnW / 2f, h * 0.55f, btnW, btnH);
    }

    void OnGUI()
    {
        if (_titleStyle == null)
        {
            _titleStyle = new GUIStyle();
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.normal.textColor = Color.white;
        }

        if (_buttonStyle == null)
        {
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontStyle = FontStyle.Bold;
        }

        float w = Screen.width;
        float h = Screen.height;

        // All sizes are fractions of the screen so the layout works on any resolution.
        _titleStyle.fontSize = Mathf.RoundToInt(h * 0.055f);

        float titleW = w * 0.9f;
        float titleH = h * 0.1f;
        GUI.Label(new Rect(w / 2f - titleW / 2f, h * 0.3f, titleW, titleH), title, _titleStyle);

        Rect btnRect = StartButtonRect();
        _buttonStyle.fontSize = Mathf.RoundToInt(btnRect.height * 0.4f);
        GUI.Button(btnRect, "Start Game", _buttonStyle);
    }
}
