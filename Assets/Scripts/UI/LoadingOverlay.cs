using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// A full-screen "Loading" overlay that persists across the scene switch. It loads
/// the game scene asynchronously, activates it (running its heavy first-frame setup
/// behind the opaque overlay), waits for the scene to warm up, then reveals the
/// ready game by fading out. Gameplay is held via <see cref="GameBoot.Ready"/> so
/// nothing happens (no ball, no score) until the reveal.
/// </summary>
public class LoadingOverlay : MonoBehaviour
{
    private Image _bg;
    private Text _text;

    public static void Begin(string sceneName)
    {
        var go = new GameObject("LoadingOverlay");
        DontDestroyOnLoad(go);
        var overlay = go.AddComponent<LoadingOverlay>();
        overlay.Build();
        overlay.StartCoroutine(overlay.Run(sceneName));
    }

    void Build()
    {
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000; // above any in-game canvas
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        var bgGO = new GameObject("BG", typeof(Image));
        bgGO.transform.SetParent(canvasGO.transform, false);
        _bg = bgGO.GetComponent<Image>();
        _bg.color = new Color(0.02f, 0.03f, 0.06f, 1f);
        var brt = _bg.rectTransform;
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var textGO = new GameObject("Text", typeof(Text));
        textGO.transform.SetParent(canvasGO.transform, false);
        _text = textGO.GetComponent<Text>();
        _text.font = font;
        _text.fontSize = 56;
        _text.color = Color.white;
        _text.alignment = TextAnchor.MiddleCenter;
        _text.text = "Loading...";
        var trt = _text.rectTransform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = Vector2.zero;
        trt.sizeDelta = new Vector2(900f, 200f);
    }

    IEnumerator Run(string sceneName)
    {
        GameBoot.Ready = false;

        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // Hold briefly so the screen is seen even when the load is instant.
        yield return WaitUnscaled(0.3f);

        // Activate: the game's heavy Awake/Start runs this frame, hidden behind us.
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }

        // Let the fresh scene warm up (initial microbes spawn ~0.2s into the scene).
        yield return WaitUnscaled(0.5f);

        // Reveal: release the ball and fade the overlay away.
        GameBoot.Ready = true;
        yield return FadeOut(0.3f);
        Destroy(gameObject);
    }

    IEnumerator FadeOut(float duration)
    {
        Color bg = _bg.color;
        Color tx = _text.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(1f - t / duration);
            _bg.color = new Color(bg.r, bg.g, bg.b, a);
            _text.color = new Color(tx.r, tx.g, tx.b, a);
            yield return null;
        }
    }

    static IEnumerator WaitUnscaled(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
