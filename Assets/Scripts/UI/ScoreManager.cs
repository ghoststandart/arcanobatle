using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int topScore;
    public int bottomScore;
    public int winningScore = 10;

    [Tooltip("Legacy world-space position. Only used when useSafeArea = false.")]
    public Vector3 scoreWorldPos = new Vector3(0f, 9.5f, 0f);

    [Tooltip("If true, score is pinned below the top safe-area edge (under notch/Dynamic Island).")]
    public bool useSafeArea = true;

    [Tooltip("Extra pixels between the top safe-area edge and the score line.")]
    public float safeAreaTopPadding = 12f;

    public int fontSize = 54;

    [Tooltip("Name shown for the AI opponent in the score line.")]
    public string aiName = "Virus";

    private GUIStyle _style;
    private bool _gameEnded;
    private string _playerName;

    void Awake()
    {
        Instance = this;
        topScore = 0;
        bottomScore = 0;
        _gameEnded = false;

        // Player 1 (bottom paddle) is the human: use the name set in Settings.
        string n = PlayerPrefs.GetString(MenuController.NameKey, "");
        if (string.IsNullOrEmpty(n))
        {
            n = "Player 1";
        }
        if (n.Length > MenuController.MaxNameLength)
        {
            n = n.Substring(0, MenuController.MaxNameLength);
        }
        _playerName = n;

        // Make the names available to the game-over screen too.
        GameResult.playerName = _playerName;
        GameResult.aiName = aiName;
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

    /// <summary>
    /// Ends the game because a paddle lost all of its cubes; the other player wins.
    /// </summary>
    public void PaddleDestroyed(int losingPlayer)
    {
        if (_gameEnded)
        {
            return;
        }
        _gameEnded = true;
        GameResult.topScore = topScore;
        GameResult.bottomScore = bottomScore;
        GameResult.winner = losingPlayer == 1 ? 2 : 1;
        SceneManager.LoadScene("GameOver");
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
        // IMGUI draws on top of the loading overlay canvas, so hide the score
        // until the game is revealed.
        if (!GameBoot.Ready)
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

        string text = $"{_playerName}  {bottomScore}:{topScore}  {aiName}";
        float width = 950f;
        float height = fontSize + 20f;

        float x;
        float y;
        if (useSafeArea)
        {
            Rect safe = Screen.safeArea;
            // Screen.safeArea uses y=0 at bottom. OnGUI uses y=0 at top. Flip.
            float safeTopY = Screen.height - (safe.y + safe.height);
            x = safe.x + safe.width / 2f - width / 2f;
            y = safeTopY + safeAreaTopPadding;
        }
        else
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(scoreWorldPos);
            x = screenPos.x - width / 2f;
            y = Screen.height - screenPos.y - height / 2f;
        }

        Rect rect = new Rect(x, y, width, height);
        GUI.Label(rect, text, _style);
    }
}
