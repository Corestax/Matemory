using System.Collections;
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

    public bool IsMapShowing { get; private set; }

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

        // Maps have to start enabled by default to avoid DreamTek script errors on start
        foreach (var m in Maps)
            m.SetActive(false);
    }

    public void SetMap(int _mapIndex)
    {
        currentIndex = _mapIndex;
        currentMap = Maps[currentIndex];
        follower = currentMap.GetComponentInChildren<SplineFollower>();
    }

    // Do not call this function: Call GameController.ShowMap() instead to hide plate
    public void ShowMap()
    {
        currentMap.SetActive(true);
        IsMapShowing = true;
    }

    public void ShowMapAndAnimateCharacter(float _delay)
    {
        StartCoroutine(ShowMapAndAnimateCharacterCR(_delay));        
    }

    private IEnumerator ShowMapAndAnimateCharacterCR(float _delay)
    {
        ShowMap();
        yield return new WaitForSeconds(_delay);
        character.Play();
    }

    // Do not call this function: Call GameController.HideMap() instead to hide plate
    public void HideMap()
    {
        currentMap.SetActive(false);
        IsMapShowing = false;
    }
}
