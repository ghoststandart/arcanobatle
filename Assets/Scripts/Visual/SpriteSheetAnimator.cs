using UnityEngine;

/// <summary>
/// Slices a sprite-sheet texture into an even grid at runtime and cycles the
/// frames on this object's SpriteRenderer. Frames are read left-to-right,
/// top-to-bottom (so frame 0 is the top-left cell).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSheetAnimator : MonoBehaviour
{
    [Tooltip("Texture name to load from a Resources folder (without extension).")]
    public string resourceName = "griz";

    public int columns = 4;
    public int rows = 2;

    [Tooltip("How many cells actually contain frames (<= columns * rows).")]
    public int frameCount = 8;

    public float framesPerSecond = 12f;

    [Tooltip("Pixels-per-unit used when building frame sprites. Larger = smaller on screen. " +
             "Set so the frame width maps to roughly one world unit before the transform scale.")]
    public float pixelsPerUnit = 337.5f;

    private SpriteRenderer _sr;
    private Sprite[] _frames;
    private int _current;
    private float _timer;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        BuildFrames();
        if (_frames != null && _frames.Length > 0)
        {
            _sr.sprite = _frames[0];
        }
    }

    void BuildFrames()
    {
        Texture2D tex = Resources.Load<Texture2D>(resourceName);
        if (tex == null)
        {
            Debug.LogError($"[SpriteSheetAnimator] Texture '{resourceName}' not found in a Resources folder.");
            return;
        }

        // Use integer cell sizes so frame rects land on pixel boundaries (avoids
        // a 1px seam bleeding from the neighbouring frame under bilinear filtering).
        int frameW = tex.width / columns;
        int frameH = tex.height / rows;
        Vector2 pivot = new Vector2(0.5f, 0.5f);

        _frames = new Sprite[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            int col = i % columns;
            int row = i / columns;
            // Texture space has y=0 at the bottom, so the visual top row is the highest y.
            int x = col * frameW;
            int y = (rows - 1 - row) * frameH;
            Rect rect = new Rect(x, y, frameW, frameH);
            _frames[i] = Sprite.Create(tex, rect, pivot, pixelsPerUnit);
        }
    }

    void Update()
    {
        if (_frames == null || _frames.Length == 0 || framesPerSecond <= 0f)
        {
            return;
        }

        _timer += Time.deltaTime;
        float frameDuration = 1f / framesPerSecond;
        while (_timer >= frameDuration)
        {
            _timer -= frameDuration;
            _current = (_current + 1) % _frames.Length;
            _sr.sprite = _frames[_current];
        }
    }
}
