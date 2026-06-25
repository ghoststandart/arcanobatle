using UnityEngine;

/// <summary>
/// Positions the side walls (WallLeft / WallRight) at the camera's visible edges so
/// the ball never escapes the screen, regardless of aspect ratio. Add this component
/// once to any object in the scene (e.g. Main Camera) — walls are auto-discovered
/// by name. Works both at Play time and in the Editor (via ExecuteAlways).
///
/// The visible wall bar stays thin and sits right at the screen edge (so the play
/// field is as wide as possible), while at runtime a deep collider is extended
/// behind it, off-screen, so a fast ball cannot tunnel through the thin bar.
/// </summary>
[ExecuteAlways]
public class BoundaryFitter : MonoBehaviour
{
    [Tooltip("Optional explicit references. Leave empty to auto-find by name.")]
    public Transform wallLeft;
    public Transform wallRight;

    [Tooltip("How far the wall centre sits inside the screen edge. 0 = walls as wide as possible (the ball bounces a hair inside the visible edge). Larger = narrower field.")]
    public float inset = 0f;

    [Tooltip("Depth (world units) of the collider extended off-screen behind each wall so a fast ball can't tunnel through the thin visible bar. Only applied at Play time.")]
    public float colliderDepth = 3f;

    private Camera _cam;

    void OnEnable()
    {
        _cam = Camera.main;
        TryAutoBind();
    }

    void LateUpdate()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                return;
            }
        }

        if (wallLeft == null || wallRight == null)
        {
            TryAutoBind();
        }

        if (!_cam.orthographic)
        {
            return;
        }

        float halfW = _cam.orthographicSize * _cam.aspect;
        FitWall(wallLeft, -halfW + inset, -1f);
        FitWall(wallRight, halfW - inset, 1f);
    }

    // Centres the thin visible bar at edgeX and, in Play mode, deepens its collider
    // outward (off-screen, in the outwardSign direction) so the ball can't tunnel.
    // The position is only written when it actually changes: re-teleporting a static
    // collider every frame breaks the ball's continuous-collision sweep and used to
    // let it slip past the wall.
    void FitWall(Transform wall, float edgeX, float outwardSign)
    {
        if (wall == null)
        {
            return;
        }

        Vector3 p = wall.position;
        if (Mathf.Abs(p.x - edgeX) > 0.0005f)
        {
            p.x = edgeX;
            wall.position = p;
        }

        if (!Application.isPlaying)
        {
            return;
        }

        var box = wall.GetComponent<BoxCollider2D>();
        if (box == null)
        {
            return;
        }

        // Grow the collider box outward while keeping its inner (ball-facing) face
        // where the thin bar already is. Sizes are in the collider's local space,
        // so convert the desired world depth through the wall's x scale.
        float scaleX = Mathf.Max(Mathf.Abs(wall.lossyScale.x), 1e-4f);
        float targetSizeX = 1f + colliderDepth / scaleX;
        float grow = targetSizeX - 1f;
        Vector2 size = new Vector2(targetSizeX, box.size.y);
        Vector2 offset = new Vector2(outwardSign * grow * 0.5f, box.offset.y);
        if (box.size != size)
        {
            box.size = size;
        }
        if (box.offset != offset)
        {
            box.offset = offset;
        }
    }

    void TryAutoBind()
    {
        if (wallLeft == null)
        {
            var go = GameObject.Find("WallLeft");
            if (go != null)
            {
                wallLeft = go.transform;
            }
        }
        if (wallRight == null)
        {
            var go = GameObject.Find("WallRight");
            if (go != null)
            {
                wallRight = go.transform;
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInstall()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            return;
        }
        if (cam.GetComponent<BoundaryFitter>() == null)
        {
            cam.gameObject.AddComponent<BoundaryFitter>();
        }
    }
}
