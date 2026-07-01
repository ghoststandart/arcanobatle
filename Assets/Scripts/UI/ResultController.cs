using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ResultController : MonoBehaviour
{
    public string menuSceneName = "MainMenu";

    private RectTransform _menuButton;
    private RectTransform _pressed;
    private RectTransform _animButton;
    private Vector3 _animTarget = Vector3.one;
    private bool _loading;
    private Font _font;

    private static readonly Vector3 PressedScale = new Vector3(0.92f, 0.92f, 1f);
    private const float PressLerpSpeed = 16f;

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

        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
    }

    void BuildUI()
    {
        var canvasGO = new GameObject("ResultCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        var winner = NewText("Winner", canvasGO.transform, $"Player {GameResult.winner} Wins!", 84, TextAnchor.MiddleCenter);
        Place(winner.rectTransform, new Vector2(0.5f, 0.66f), new Vector2(1000f, 160f));

        var score = NewText("Score", canvasGO.transform, $"{GameResult.bottomScore} : {GameResult.topScore}", 64, TextAnchor.MiddleCenter);
        Place(score.rectTransform, new Vector2(0.5f, 0.54f), new Vector2(900f, 120f));

        _menuButton = MakeButton(canvasGO.transform, "Main Menu", new Vector2(0.5f, 0.38f), new Vector2(560f, 150f));
    }

    // Fire on release (finger lift) over the button rather than the initial press.
    void Update()
    {
        if (_loading)
        {
            return;
        }

        AnimatePress();

        var pointer = Pointer.current;
        if (pointer == null)
        {
            return;
        }

        Vector2 pos = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame)
        {
            _pressed = Hit(_menuButton, pos) ? _menuButton : null;
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
            bool onButton = _pressed == _menuButton && Hit(_menuButton, pos);
            _pressed = null;
            if (onButton)
            {
                _loading = true;
                SceneManager.LoadScene(menuSceneName);
            }
        }
    }

    // Eases the currently animating button toward its target scale each frame.
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

    RectTransform MakeButton(Transform parent, string label, Vector2 anchor, Vector2 size)
    {
        var img = NewImage("Btn_" + label, parent);
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
        Place(img.rectTransform, anchor, size);

        var lbl = NewText("Label", img.transform, label, 52, TextAnchor.MiddleCenter);
        Stretch(lbl.rectTransform);
        return img.rectTransform;
    }

    static Image NewImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(Image));
        go.transform.SetParent(parent, false);
        return go.GetComponent<Image>();
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
        txt.supportRichText = true;
        return txt;
    }

    static Sprite ToSprite(Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
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
