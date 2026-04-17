using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultController : MonoBehaviour
{
    public string menuSceneName = "MainMenu";

    private GUIStyle _titleStyle;
    private GUIStyle _scoreStyle;
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
            _titleStyle.fontSize = 56;
            _titleStyle.normal.textColor = Color.white;
        }

        if (_scoreStyle == null)
        {
            _scoreStyle = new GUIStyle();
            _scoreStyle.alignment = TextAnchor.MiddleCenter;
            _scoreStyle.fontSize = 40;
            _scoreStyle.normal.textColor = Color.white;
        }

        if (_buttonStyle == null)
        {
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 30;
            _buttonStyle.fontStyle = FontStyle.Bold;
        }

        float w = Screen.width;
        float h = Screen.height;

        string winnerText = $"Player {GameResult.winner} Wins!";
        string scoreText = $"{GameResult.bottomScore} : {GameResult.topScore}";

        float blockW = 600f;
        GUI.Label(new Rect(w / 2f - blockW / 2f, h * 0.28f, blockW, 80f), winnerText, _titleStyle);
        GUI.Label(new Rect(w / 2f - blockW / 2f, h * 0.45f, blockW, 60f), scoreText, _scoreStyle);

        float btnW = 300f;
        float btnH = 80f;
        if (GUI.Button(new Rect(w / 2f - btnW / 2f, h * 0.65f, btnW, btnH), "Main Menu", _buttonStyle))
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
