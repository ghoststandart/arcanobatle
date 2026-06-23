using UnityEngine;

/// <summary>
/// Positions the side walls (WallLeft / WallRight) at the camera's visible edges so
/// the ball never escapes the screen, regardless of aspect ratio. Add this component
/// once to any object in the scene (e.g. Main Camera) — walls are auto-discovered
/// by name. Works both at Play time and in the Editor (via ExecuteAlways).
/// </summary>
[ExecuteAlways]
public class BoundaryFitter : MonoBehaviour
{
    [Tooltip("Optional explicit references. Leave empty to auto-find by name.")]
    public Transform wallLeft;
    public Transform wallRight;

    [Tooltip("Distance from the camera edge. Positive = walls sit inside the visible area. Negative = walls are pushed off-screen (only the inner edge touches the view).")]
    public float inset = 0.2f;

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

        if (wallLeft != null)
        {
            Vector3 p = wallLeft.position;
            p.x = -halfW + inset;
            wallLeft.position = p;
        }

        if (wallRight != null)
        {
            Vector3 p = wallRight.position;
            p.x = halfW - inset;
            wallRight.position = p;
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
