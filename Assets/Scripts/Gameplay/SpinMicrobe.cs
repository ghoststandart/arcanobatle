using UnityEngine;

/// <summary>
/// Spins a microbe cluster about its centre at a fixed signed speed (negative =
/// clockwise, positive = counter-clockwise). Rotating the whole cluster keeps a
/// sliced microbe's image coherent (it turns as one rigid picture); BrickCluster
/// reads collider world bounds so its wall/paddle bounces stay correct while spun.
/// </summary>
public class SpinMicrobe : MonoBehaviour
{
    public float degreesPerSecond;

    void Update()
    {
        transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
    }
}
