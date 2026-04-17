using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public string gameSceneName = "SampleScene";
    public string title = "ArcanoBattle";

    private GUIStyle _titleStyle;
    private GUIStyle _buttonStyle;

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

    void OnGUI()
    {
        if (_titleStyle == null)
        {
            _titleStyle = new GUIStyle();
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.fontSize = 64;
            _titleStyle.normal.textColor = Color.white;
        }

        if (_buttonStyle == null)
        {
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 36;
            _buttonStyle.fontStyle = FontStyle.Bold;
        }

        float w = Screen.width;
        float h = Screen.height;

        float titleW = 600f;
        float titleH = 100f;
        GUI.Label(new Rect(w / 2f - titleW / 2f, h * 0.3f, titleW, titleH), title, _titleStyle);

        float btnW = 300f;
        float btnH = 80f;
        if (GUI.Button(new Rect(w / 2f - btnW / 2f, h * 0.55f, btnW, btnH), "Start Game", _buttonStyle))
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
