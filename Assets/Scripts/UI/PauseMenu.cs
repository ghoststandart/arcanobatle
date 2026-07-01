using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// In-game menu: a small "Menu" button at the top of the screen that pauses the
/// game and opens an overlay with Resume and Main Menu. Input is read directly
/// from the pointer (like the rest of the project) so it works while paused.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public string menuSceneName = "MainMenu";

    private RectTransform _openButton;
    private RectTransform _resumeButton;
    private RectTransform _mainMenuButton;
    private GameObject _overlay;
    private bool _paused;
    private Font _font;

    private RectTransform _pressed;
    private RectTransform _animButton;
    private Vector3 _animTarget = Vector3.one;
    private static readonly Vector3 PressedScale = new Vector3(0.92f, 0.92f, 1f);
    private const float PressLerpSpeed = 16f;

    void Awake()
    {
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
    }

    void BuildUI()
    {
        var canvasGO = new GameObject("PauseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        // Top-right corner "Menu" button, kept clear of the notch/score.
        _openButton = MakeButton(canvasGO.transform, "Menu", 40);
        _openButton.anchorMin = _openButton.anchorMax = _openButton.pivot = new Vector2(1f, 1f);
        _openButton.anchoredPosition = new Vector2(-30f, -130f);
        _openButton.sizeDelta = new Vector2(210f, 100f);

        // Pause overlay.
        _overlay = new GameObject("PauseOverlay", typeof(Image));
        _overlay.transform.SetParent(canvasGO.transform, false);
        var bg = _overlay.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.8f);
        Stretch(bg.rectTransform);

        var title = NewText("PausedTitle", _overlay.transform, "Paused", 80, TextAnchor.MiddleCenter);
        Place(title.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(900f, 160f));

        _resumeButton = MakeButton(_overlay.transform, "Resume", 52);
        Place(_resumeButton, new Vector2(0.5f, 0.46f), new Vector2(520f, 150f));

        _mainMenuButton = MakeButton(_overlay.transform, "Main Menu", 52);
        Place(_mainMenuButton, new Vector2(0.5f, 0.31f), new Vector2(520f, 150f));

        _overlay.SetActive(false);
    }

    void Update()
    {
        AnimatePress();

        // Ignore input while the loading screen still owns the scene.
        if (!GameBoot.Ready)
        {
            return;
        }

        var pointer = Pointer.current;
        if (pointer == null)
        {
            return;
        }
        Vector2 pos = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame)
        {
            _pressed = FindButton(pos);
            if (_pressed != null)
            {
                _animButton = _pressed;
                _animTarget = PressedScale;
            }
            return;
        }

        if (pointer.press.wasReleasedThisFrame)
        {
            if (_pressed != null)
            {
                _animButton = _pressed;
                _animTarget = Vector3.one;
            }
            RectTransform released = FindButton(pos);
            if (released != null && released == _pressed)
            {
                Activate(released);
            }
            _pressed = null;
        }
    }

    RectTransform FindButton(Vector2 pos)
    {
        if (_paused)
        {
            if (Hit(_resumeButton, pos))
            {
                return _resumeButton;
            }
            if (Hit(_mainMenuButton, pos))
            {
                return _mainMenuButton;
            }
            return null;
        }
        if (Hit(_openButton, pos))
        {
            return _openButton;
        }
        return null;
    }

    void Activate(RectTransform rt)
    {
        if (_paused)
        {
            if (rt == _resumeButton)
            {
                SetPaused(false);
            }
            else if (rt == _mainMenuButton)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(menuSceneName);
            }
            return;
        }
        if (rt == _openButton)
        {
            SetPaused(true);
        }
    }

    void SetPaused(bool paused)
    {
        _paused = paused;
        _overlay.SetActive(paused);
        _openButton.gameObject.SetActive(!paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    void AnimatePress()
    {
        if (_animButton == null)
        {
            return;
        }
        _animButton.localScale = Vector3.Lerp(_animButton.localScale, _animTarget, PressLerpSpeed * Time.unscaledDeltaTime);
        if (_animTarget == Vector3.one && (_animButton.localScale - Vector3.one).sqrMagnitude < 0.0001f)
        {
            _animButton.localScale = Vector3.one;
            _animButton = null;
        }
    }

    RectTransform MakeButton(Transform parent, string label, int fontSize)
    {
        var go = new GameObject("Btn_" + label, typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        var btnTex = Resources.Load<Texture2D>("menu_button");
        if (btnTex != null)
        {
            img.sprite = Sprite.Create(btnTex, new Rect(0f, 0f, btnTex.width, btnTex.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(40f, 40f, 40f, 40f));
            img.type = Image.Type.Sliced;
        }
        else
        {
            img.color = new Color(0.09f, 0.12f, 0.17f);
        }

        var lbl = NewText("Label", go.transform, label, fontSize, TextAnchor.MiddleCenter);
        Stretch(lbl.rectTransform);
        return img.rectTransform;
    }

    Text NewText(string name, Transform parent, string content, int fontSize, TextAnchor align)
    {
        var go = new GameObject(name, typeof(Text));
        go.transform.SetParent(parent, false);
        var txt = go.GetComponent<Text>();
        txt.text = content;
        txt.font = _font;
        txt.fontSize = fontSize;
        txt.alignment = align;
        txt.color = Color.white;
        return txt;
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static bool Hit(RectTransform rt, Vector2 screenPos)
    {
        return rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null);
    }
}
