using UnityEngine;
using Dreamteck.Splines;

public class MapsController : Singleton<MapsController>
{
    [SerializeField]
    private GameObject[] Maps;
        
    private SplineComputer splineComputer;
    private SplineFollower follower;
    private Character character;
    private PathGenerator path;
    private Material[] material;
    private int currentIndex;
    private GameObject currentMap;

    void Start()
    {
        // Default current map to 0
        // NOTE: This will need to change in the future, when loading a saved map
        SetMap(currentIndex);

        splineComputer = currentMap.GetComponent<SplineComputer>();
        follower = currentMap.GetComponentInChildren<SplineFollower>();
        character = follower.GetComponent<Character>();
        path = splineComputer.GetComponent<PathGenerator>();        

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            character.Play();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            character.Stop();
        }
    }

    public void SetMap(int _mapIndex)
    {
        currentIndex = _mapIndex;
        currentMap = Maps[currentIndex];
        follower = currentMap.GetComponentInChildren<SplineFollower>();
    }

    public void ShowMap()
    {
        currentMap.SetActive(true);
    }

    public void HideMap()
    {
        currentMap.SetActive(false);
    }
}
