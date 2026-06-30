using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public string gameSceneName = "SampleScene";

    private RectTransform _startButton;
    private RectTransform _rulesButton;
    private RectTransform _backButton;
    private GameObject _rulesPanel;
    private bool _loading;
    private Font _font;

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

        _startButton = MakeButton(canvasGO.transform, "Start Game", new Vector2(0.5f, 0.44f), new Vector2(540f, 140f));
        _rulesButton = MakeButton(canvasGO.transform, "Rules", new Vector2(0.5f, 0.31f), new Vector2(540f, 140f));

        BuildRulesPanel(canvasGO.transform);
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
        float y = 0.83f;
        const float step = 0.097f;
        MakeRow(_rulesPanel.transform, "Powerups/speed", "Speed Boost", "Speeds up one ball for a few seconds.", y, false, Color.white); y -= step;
        MakeRow(_rulesPanel.transform, "Powerups/repair", "Repair", "Restores about a third of the paddle.", y, false, Color.white); y -= step;
        MakeRow(_rulesPanel.transform, "Powerups/extracubes", "Extra Cubes", "Adds 3 spare cubes around the paddle.", y, false, Color.white); y -= step;
        MakeRow(_rulesPanel.transform, "Powerups/splitball", "Split Ball", "Splits one ball into two.", y, false, Color.white); y -= step;
        MakeRow(_rulesPanel.transform, "Powerups/bullet", "Bullet", "Flies fast through the paddle, killing one cube per row.", y, false, Color.white); y -= step;
        MakeRow(_rulesPanel.transform, "Powerups/bullet", "Homing Bullet", "Slower, aims at the paddle centre, chips one cube per row.", y, false, red); y -= step;
        MakeRow(_rulesPanel.transform, "paddle", "Paddle", "Destructible cubes. The ball and bullets break them — Repair rebuilds it.", y - 0.005f, true, Color.white);

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

        if (_rulesPanel != null && _rulesPanel.activeSelf)
        {
            if (Hit(_backButton, pos))
            {
                _rulesPanel.SetActive(false);
            }
            return;
        }

        if (Hit(_startButton, pos))
        {
            _loading = true;
            SceneManager.LoadScene(gameSceneName);
        }
        else if (Hit(_rulesButton, pos))
        {
            _rulesPanel.SetActive(true);
        }
    }

    static bool Hit(RectTransform rt, Vector2 screenPos)
    {
        return rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null);
    }
}
