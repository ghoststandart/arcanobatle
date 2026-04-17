using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int topScore;
    public int bottomScore;
    public int winningScore = 10;

    public Vector3 scoreWorldPos = new Vector3(0f, 9.5f, 0f);
    public int fontSize = 36;

    private GUIStyle _style;
    private bool _gameEnded;

    void Awake()
    {
        Instance = this;
        topScore = 0;
        bottomScore = 0;
        _gameEnded = false;
    }

    public void AddTopScore()
    {
        if (_gameEnded)
        {
            return;
        }
        topScore++;
        CheckWin();
    }

    public void AddBottomScore()
    {
        if (_gameEnded)
        {
            return;
        }
        bottomScore++;
        CheckWin();
    }

    void CheckWin()
    {
        if (topScore >= winningScore || bottomScore >= winningScore)
        {
            _gameEnded = true;
            GameResult.topScore = topScore;
            GameResult.bottomScore = bottomScore;
            GameResult.winner = bottomScore >= winningScore ? 1 : 2;
            SceneManager.LoadScene("GameOver");
        }
    }

    void OnGUI()
    {
        if (_style == null)
        {
            _style = new GUIStyle();
            _style.alignment = TextAnchor.MiddleCenter;
            _style.fontStyle = FontStyle.Bold;
            _style.normal.textColor = Color.white;
        }
        _style.fontSize = fontSize;

        string text = $"Player 1  {bottomScore}:{topScore}  Player 2";
        Vector3 screenPos = Camera.main.WorldToScreenPoint(scoreWorldPos);
        float width = 800f;
        float height = fontSize + 20f;
        Rect rect = new Rect(screenPos.x - width / 2f, Screen.height - screenPos.y - height / 2f, width, height);
        GUI.Label(rect, text, _style);
    }
}
