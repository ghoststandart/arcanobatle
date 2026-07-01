using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public string gameSceneName = "SampleScene";

    private RectTransform _startButton;
    private RectTransform _rulesButton;
    private RectTransform _aboutButton;
    private RectTransform _settingsButton;
    private RectTransform _backButton;
    private RectTransform _aboutBackButton;
    private RectTransform _settingsBackButton;
    private GameObject _rulesPanel;
    private GameObject _aboutPanel;
    private GameObject _settingsPanel;

    // PlayerPrefs key for the player's saved name.
    public const string NameKey = "PlayerName";

    // Kept short so two names of this length still fit on one score line.
    public const int MaxNameLength = 8;
    private RectTransform _pressed;
    private RectTransform _animButton;
    private Vector3 _animTarget = Vector3.one;
    private bool _loading;
    private Font _font;

    // Button scale while held, and how fast it eases toward its target.
    private static readonly Vector3 PressedScale = new Vector3(0.92f, 0.92f, 1f);
    private const float PressLerpSpeed = 16f;

    // Link buttons on the About panel, paired with the URL each one opens.
    private readonly System.Collections.Generic.List<RectTransform> _linkButtons = new System.Collections.Generic.List<RectTransform>();
    private readonly System.Collections.Generic.List<string> _linkUrls = new System.Collections.Generic.List<string>();

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
        var canvasGO = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        // The name InputField needs an EventSystem to receive focus/keyboard. The
        // menu's own button clicks are read directly from Pointer, so this only
        // powers the text field and doesn't interfere with them.
        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        var titleTex = Resources.Load<Texture2D>("menu_title");
        if (titleTex != null)
        {
            var img = NewImage("Title", canvasGO.transform);
            img.sprite = ToSprite(titleTex);
            img.preserveAspect = true;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.78f);
            rt.anchoredPosition = Vector2.zero;   // title image is symmetric -> centred
            float tw = 860f;
            rt.sizeDelta = new Vector2(tw, tw * titleTex.height / titleTex.width);
        }

        _startButton = MakeButton(canvasGO.transform, "Start Game", new Vector2(0.5f, 0.48f), new Vector2(540f, 132f));
        _rulesButton = MakeButton(canvasGO.transform, "Rules", new Vector2(0.5f, 0.37f), new Vector2(540f, 132f));
        _aboutButton = MakeButton(canvasGO.transform, "About", new Vector2(0.5f, 0.26f), new Vector2(540f, 132f));
        _settingsButton = MakeButton(canvasGO.transform, "Settings", new Vector2(0.5f, 0.15f), new Vector2(540f, 132f));

        BuildRulesPanel(canvasGO.transform);
        BuildAboutPanel(canvasGO.transform);
        BuildSettingsPanel(canvasGO.transform);
    }

    void BuildRulesPanel(Transform parent)
    {
        _rulesPanel = new GameObject("RulesPanel", typeof(Image));
        _rulesPanel.transform.SetParent(parent, false);
        var bg = _rulesPanel.GetComponent<Image>();
        bg.color = new Color(0.02f, 0.03f, 0.06f, 1f);   // opaque so the menu behind is hidden
        Stretch(bg.rectTransform);

        var title = NewText("RulesTitle", _rulesPanel.transform, "Rules", 72, TextAnchor.MiddleCenter);
        Place(title.rectTransform, new Vector2(0.5f, 0.93f), new Vector2(900f, 100f));

        Color red = new Color(1f, 0.7f, 0.6f);
        float y = 0.82f;
        const float step = 0.11f;
        MakeRow(_rulesPanel.transform, "Powerups/speed", "Speed Boost", "Speeds the ball up for a while.", y, false, Color.white); y -= step;
        MakeRow(_rulesPanel.transform, "Powerups/repair", "Repair", "Heals part of your paddle.", y, false, Color.white); y -= step;
        MakeRow(_rulesPanel.transform, "Powerups/extracubes", "Extra Cubes", "Adds spare cubes to your paddle.", y, false, Color.white); y -= step;
        MakeRow(_rulesPanel.transform, "Powerups/splitball", "Split Ball", "Turns one ball into two.", y, false, Color.white); y -= step;
        MakeBulletRow(_rulesPanel.transform, "Bullets", "Shots that pierce a paddle, breaking a cube per row. The red one homes in.", y, red); y -= step;
        MakeRow(_rulesPanel.transform, "paddle", "Paddle", "Your wall of cubes — keep it alive.", y, true, Color.white);

        _backButton = MakeButton(_rulesPanel.transform, "Back", new Vector2(0.5f, 0.05f), new Vector2(400f, 110f));

        _rulesPanel.SetActive(false);
    }

    // One rules row: icon on the left (optionally tinted), name + description on the right.
    void MakeRow(Transform parent, string iconRes, string name, string desc, float anchorY, bool wideIcon, Color iconTint)
    {
        var tex = Resources.Load<Texture2D>(iconRes);
        if (tex != null)
        {
            var icon = NewImage("Icon_" + name, parent);
            icon.sprite = ToSprite(tex);
            icon.preserveAspect = true;
            icon.color = iconTint;
            float iw = wideIcon ? 144f : 120f;
            Place(icon.rectTransform, new Vector2(0.16f, anchorY), new Vector2(iw, 120f));
        }

        var txt = NewText("Desc_" + name, parent, "<b>" + name + "</b>\n" + desc, 32, TextAnchor.MiddleLeft);
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        Place(txt.rectTransform, new Vector2(0.6f, anchorY), new Vector2(660f, 140f));
    }

    // Both bullets share one row: two icons side by side (plain + red homing) with
    // a single shared description.
    void MakeBulletRow(Transform parent, string name, string desc, float anchorY, Color homingTint)
    {
        var tex = Resources.Load<Texture2D>("Powerups/bullet");
        if (tex != null)
        {
            var a = NewImage("Icon_" + name + "A", parent);
            a.sprite = ToSprite(tex);
            a.preserveAspect = true;
            Place(a.rectTransform, new Vector2(0.11f, anchorY), new Vector2(90f, 110f));

            var b = NewImage("Icon_" + name + "B", parent);
            b.sprite = ToSprite(tex);
            b.preserveAspect = true;
            b.color = homingTint;
            Place(b.rectTransform, new Vector2(0.22f, anchorY), new Vector2(90f, 110f));
        }

        var txt = NewText("Desc_" + name, parent, "<b>" + name + "</b>\n" + desc, 32, TextAnchor.MiddleLeft);
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        Place(txt.rectTransform, new Vector2(0.6f, anchorY), new Vector2(660f, 140f));
    }

    RectTransform MakeButton(Transform parent, string label, Vector2 anchor, Vector2 size, int fontSize = 52)
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

        var lbl = NewText("Label", img.transform, label, fontSize, TextAnchor.MiddleCenter);
        Stretch(lbl.rectTransform);
        return img.rectTransform;
    }

    // A tappable button on the About panel that opens a URL in the browser.
    void MakeLinkButton(Transform parent, string label, float anchorY, string url)
    {
        var rt = MakeButton(parent, label, new Vector2(0.5f, anchorY), new Vector2(860f, 120f), 34);
        _linkButtons.Add(rt);
        _linkUrls.Add(url);
    }

    // "About the developer" screen. The contact details are placeholders showing
    // the kind of data a real listing would carry; the Support link opens an
    // external donation page in the browser (no In-App Purchase, so App Store
    // rules on developer donations are respected).
    void BuildAboutPanel(Transform parent)
    {
        _aboutPanel = new GameObject("AboutPanel", typeof(Image));
        _aboutPanel.transform.SetParent(parent, false);
        var bg = _aboutPanel.GetComponent<Image>();
        bg.color = new Color(0.02f, 0.03f, 0.06f, 1f);
        Stretch(bg.rectTransform);

        var title = NewText("AboutTitle", _aboutPanel.transform, "About", 72, TextAnchor.MiddleCenter);
        Place(title.rectTransform, new Vector2(0.5f, 0.93f), new Vector2(900f, 100f));

        var dev = NewText("DevName", _aboutPanel.transform, "Nimbus Interactive", 60, TextAnchor.MiddleCenter);
        Place(dev.rectTransform, new Vector2(0.5f, 0.83f), new Vector2(900f, 90f));

        var tag = NewText("DevTag", _aboutPanel.transform, "Independent game studio", 32, TextAnchor.MiddleCenter);
        Place(tag.rectTransform, new Vector2(0.5f, 0.76f), new Vector2(960f, 70f));

        // Fictional example data (reserved .example domain) — shows the format only,
        // with no link to the real project. Replace with the studio's real details.
        MakeLinkButton(_aboutPanel.transform, "Email:  contact@nimbus-interactive.example", 0.62f, "mailto:contact@nimbus-interactive.example");
        MakeLinkButton(_aboutPanel.transform, "Website:  nimbus-interactive.example", 0.50f, "https://nimbus-interactive.example");
        MakeLinkButton(_aboutPanel.transform, "Support the developer  ♥", 0.38f, "https://ko-fi.com/nimbusinteractive");

        var note = NewText("AboutNote", _aboutPanel.transform,
            "Support opens in your browser — the App Store isn't charged.", 28, TextAnchor.MiddleCenter);
        note.horizontalOverflow = HorizontalWrapMode.Wrap;
        Place(note.rectTransform, new Vector2(0.5f, 0.26f), new Vector2(880f, 110f));

        _aboutBackButton = MakeButton(_aboutPanel.transform, "Back", new Vector2(0.5f, 0.05f), new Vector2(400f, 110f));

        _aboutPanel.SetActive(false);
    }

    // "Settings" screen. For now it holds a single setting: the player's name,
    // typed into an InputField and saved to PlayerPrefs.
    void BuildSettingsPanel(Transform parent)
    {
        _settingsPanel = new GameObject("SettingsPanel", typeof(Image));
        _settingsPanel.transform.SetParent(parent, false);
        var bg = _settingsPanel.GetComponent<Image>();
        bg.color = new Color(0.02f, 0.03f, 0.06f, 1f);
        Stretch(bg.rectTransform);

        var title = NewText("SettingsTitle", _settingsPanel.transform, "Settings", 72, TextAnchor.MiddleCenter);
        Place(title.rectTransform, new Vector2(0.5f, 0.9f), new Vector2(900f, 100f));

        var label = NewText("NameLabel", _settingsPanel.transform, "Player name", 40, TextAnchor.MiddleCenter);
        Place(label.rectTransform, new Vector2(0.5f, 0.72f), new Vector2(900f, 80f));

        BuildNameField(_settingsPanel.transform, new Vector2(0.5f, 0.62f), new Vector2(760f, 130f));

        _settingsBackButton = MakeButton(_settingsPanel.transform, "Back", new Vector2(0.5f, 0.05f), new Vector2(400f, 110f));

        _settingsPanel.SetActive(false);
    }

    // A legacy UGUI InputField built in code, pre-filled from PlayerPrefs and
    // saving back on every change.
    void BuildNameField(Transform parent, Vector2 anchor, Vector2 size)
    {
        var bg = NewImage("NameInput", parent);
        bg.color = new Color(1f, 1f, 1f, 0.12f);
        Place(bg.rectTransform, anchor, size);

        var input = bg.gameObject.AddComponent<InputField>();
        input.targetGraphic = bg;

        var text = NewText("Text", bg.transform, "", 40, TextAnchor.MiddleLeft);
        text.supportRichText = false;
        InsetStretch(text.rectTransform, 24f, 8f);

        var placeholder = NewText("Placeholder", bg.transform, "Enter your name", 40, TextAnchor.MiddleLeft);
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.color = new Color(1f, 1f, 1f, 0.4f);
        InsetStretch(placeholder.rectTransform, 24f, 8f);

        input.textComponent = text;
        input.placeholder = placeholder;
        input.characterLimit = MaxNameLength;
        input.text = PlayerPrefs.GetString(NameKey, "");
        input.onValueChanged.AddListener(delegate(string v) { PlayerPrefs.SetString(NameKey, v); });
        input.onEndEdit.AddListener(delegate(string v)
        {
            PlayerPrefs.SetString(NameKey, v);
            PlayerPrefs.Save();
        });
    }

    static void InsetStretch(RectTransform rt, float padX, float padY)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padX, padY);
        rt.offsetMax = new Vector2(-padX, -padY);
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

        // Press: sink the button under the finger for feedback.
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

        // Release: pop the button back and fire only if the finger lifts on the
        // same button it was pressed on.
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

    // The button under a screen position, limited to whichever set is interactable
    // for the current panel state.
    RectTransform FindButton(Vector2 pos)
    {
        if (_rulesPanel != null && _rulesPanel.activeSelf)
        {
            if (Hit(_backButton, pos))
            {
                return _backButton;
            }
            return null;
        }

        if (_aboutPanel != null && _aboutPanel.activeSelf)
        {
            if (Hit(_aboutBackButton, pos))
            {
                return _aboutBackButton;
            }
            for (int i = 0; i < _linkButtons.Count; i++)
            {
                if (Hit(_linkButtons[i], pos))
                {
                    return _linkButtons[i];
                }
            }
            return null;
        }

        if (_settingsPanel != null && _settingsPanel.activeSelf)
        {
            // Only the Back button is a "button" here; taps on the name field are
            // left to the EventSystem to focus it.
            if (Hit(_settingsBackButton, pos))
            {
                return _settingsBackButton;
            }
            return null;
        }

        if (Hit(_startButton, pos))
        {
            return _startButton;
        }
        if (Hit(_rulesButton, pos))
        {
            return _rulesButton;
        }
        if (Hit(_aboutButton, pos))
        {
            return _aboutButton;
        }
        if (Hit(_settingsButton, pos))
        {
            return _settingsButton;
        }
        return null;
    }

    void Activate(RectTransform rt)
    {
        if (_rulesPanel != null && _rulesPanel.activeSelf)
        {
            if (rt == _backButton)
            {
                _rulesPanel.SetActive(false);
            }
            return;
        }

        if (_aboutPanel != null && _aboutPanel.activeSelf)
        {
            if (rt == _aboutBackButton)
            {
                _aboutPanel.SetActive(false);
                return;
            }
            for (int i = 0; i < _linkButtons.Count; i++)
            {
                if (rt == _linkButtons[i])
                {
                    Application.OpenURL(_linkUrls[i]);
                    return;
                }
            }
            return;
        }

        if (_settingsPanel != null && _settingsPanel.activeSelf)
        {
            if (rt == _settingsBackButton)
            {
                _settingsPanel.SetActive(false);
            }
            return;
        }

        if (rt == _startButton)
        {
            _loading = true;
            SceneManager.LoadScene(gameSceneName);
        }
        else if (rt == _rulesButton)
        {
            _rulesPanel.SetActive(true);
        }
        else if (rt == _aboutButton)
        {
            _aboutPanel.SetActive(true);
        }
        else if (rt == _settingsButton)
        {
            _settingsPanel.SetActive(true);
        }
    }


    static bool Hit(RectTransform rt, Vector2 screenPos)
    {
        return rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null);
    }
}
