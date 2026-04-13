using UnityEngine;

public class GameSetup : MonoBehaviour
{
    void Awake()
    {
        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 10f;
        cam.backgroundColor = Color.black;
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }
}
