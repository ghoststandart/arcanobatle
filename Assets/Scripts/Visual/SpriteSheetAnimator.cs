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
    public float pixelsPerUnit = 100f;

    [Tooltip("Rotate the sprite to face this object's Rigidbody2D velocity.")]
    public bool faceVelocity = false;

    [Tooltip("Added to the velocity angle. Use 0 when the artwork's 'forward' (mouth/teeth) points +X, 180 when it points -X.")]
    public float facingOffsetDegrees = 0f;

    [Tooltip("Pivot each frame on its own opaque-pixel center so off-center frames don't wobble. Needs the texture's Read/Write enabled. Off when the sheet is already a clean centered grid.")]
    public bool autoCenterFrames = false;

    [Tooltip("Alpha (0-255) above which a pixel counts as part of the drawing for auto-centering.")]
    public int alphaThreshold = 16;

    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private Sprite[] _frames;
    private int _current;
    private float _timer;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        BuildFrames();
        if (_frames != null && _frames.Length > 0)
        {
            _sr.sprite = _frames[0];
        }
    }

    // Done in LateUpdate so the facing rotation is the last write to the
    // transform each frame (physics syncs the transform from the body in
    // FixedUpdate; setting it here wins for rendering).
    void LateUpdate()
    {
        if (!faceVelocity || _rb == null)
        {
            return;
        }

        Vector2 v = _rb.linearVelocity;
        if (v.sqrMagnitude < 0.01f)
        {
            return;
        }

        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + facingOffsetDegrees;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
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

        // Read the whole texture once so each frame can be pivoted on the center
        // of its own drawing (frames aren't centered in their cells otherwise the
        // sprite wobbles as it spins). Falls back to cell-center if not readable.
        Color32[] pixels = null;
        if (autoCenterFrames && tex.isReadable)
        {
            pixels = tex.GetPixels32();
        }

        _frames = new Sprite[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            int col = i % columns;
            int row = i / columns;
            // Texture space has y=0 at the bottom, so the visual top row is the highest y.
            int x = col * frameW;
            int y = (rows - 1 - row) * frameH;
            Rect rect = new Rect(x, y, frameW, frameH);
            Vector2 pivot = ContentPivot(pixels, tex.width, x, y, frameW, frameH);
            _frames[i] = Sprite.Create(tex, rect, pivot, pixelsPerUnit);
        }
    }

    // Returns the normalized pivot (0-1 within the cell) at the center of the
    // cell's opaque bounding box, so every frame is anchored on the drawing
    // rather than the cell. Defaults to the cell center when pixels are missing.
    Vector2 ContentPivot(Color32[] pixels, int texWidth, int x0, int y0, int w, int h)
    {
        if (pixels == null)
        {
            return new Vector2(0.5f, 0.5f);
        }

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        for (int yy = 0; yy < h; yy++)
        {
            int rowBase = (y0 + yy) * texWidth + x0;
            for (int xx = 0; xx < w; xx++)
            {
                if (pixels[rowBase + xx].a >= alphaThreshold)
                {
                    if (xx < minX) { minX = xx; }
                    if (xx > maxX) { maxX = xx; }
                    if (yy < minY) { minY = yy; }
                    if (yy > maxY) { maxY = yy; }
                }
            }
        }

        if (maxX < minX)
        {
            return new Vector2(0.5f, 0.5f);
        }

        float centerX = (minX + maxX + 1) * 0.5f;
        float centerY = (minY + maxY + 1) * 0.5f;
        return new Vector2(centerX / w, centerY / h);
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
