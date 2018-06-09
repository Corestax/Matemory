using UnityEngine;
using Dreamteck.Splines;

public class SplineTest : MonoBehaviour
{
    [SerializeField]
    private SplineComputer spline;
    [SerializeField]
    private SplineFollower follower;

    private Material[] material;

    void Start()
    {
        // TEMPORARY EDITOR FIX FOR SHADER
        var meshes = follower.GetComponentsInChildren<MeshRenderer>();
        foreach (var m in meshes)
        {
            // Clone material
            m.material.CopyPropertiesFromMaterial(new Material(m.material));

            // Determine if ARCore lighting should be enabled
#if UNITY_EDITOR
            m.material.DisableKeyword("ARCORELIGHT_ON");
#else
            m.material.EnableKeyword("ARCORELIGHT_ON");
#endif
        }     
    }
}
