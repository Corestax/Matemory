using System.Collections;
using UnityEngine;
using Dreamteck.Splines;

public class MapsController : Singleton<MapsController>
{
    [SerializeField]
    private GameObject[] Maps;

    private LevelsController levelsController;
    private SplineComputer splineComputer;
    private SplineFollower follower;
    private Character character;
    private PathGenerator path;
    private Material[] material;
    private int currentIndex;
    private GameObject currentMap;
    private RaycastHit hit;
    private Ray ray;
    private Touch touch;
    
    public bool IsMapShowing { get; private set; }

    void Start()
    {
        // Default current map to 0
        // NOTE: This will need to change in the future, when loading a saved map
        SetMap(currentIndex);

        levelsController = LevelsController.Instance;
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

    void Update()
    {
        if (!IsMapShowing)
            return;

        SelectLevel();
    }

    private void SelectLevel()
    {
        // Mouse input
        if (!GameController.Instance.EnableAR)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Raycast
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.tag == "TargetPointPlatform")
                    {
                        // Replay previous level
                        int selectedLevel = hit.collider.GetComponentInParent<TargetPoint>().Level;
                        if (selectedLevel <= levelsController.HighestLevel)
                        {
                            // If character needs to move
                            if (selectedLevel != levelsController.CurrentLevel)
                            {
                                levelsController.CurrentLevel = selectedLevel;
                                SetCharacterPosition(selectedLevel);
                                UIController.Instance.ShowPanel(UIController.PanelTypes.PLAY_LEVEL);
                            }
                            // If character already on selected level
                            else
                            {
                                UIController.Instance.ShowPanel(UIController.PanelTypes.PLAY_LEVEL);
                            }
                        }
                    }
                }
            }
        }
        // Touch input
        else
        {
            if (Input.touchCount > 0)
            {
                touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    ray = Camera.main.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.tag == "TargetPointPlatform")
                        {
                            // Replay previous level
                            int selectedLevel = hit.collider.GetComponentInParent<TargetPoint>().Level;
                            if (selectedLevel <= levelsController.HighestLevel)
                            {
                                // If character needs to move
                                if (selectedLevel != levelsController.CurrentLevel)
                                {
                                    levelsController.CurrentLevel = selectedLevel;
                                    SetCharacterPosition(selectedLevel);
                                    UIController.Instance.ShowPanel(UIController.PanelTypes.PLAY_LEVEL);
                                }
                                // If character already on selected level
                                else
                                {
                                    UIController.Instance.ShowPanel(UIController.PanelTypes.PLAY_LEVEL);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void SetCharacterPosition(int level)
    {
        character.SetPosition(level);
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
        character.Play(true);
    }

    // Do not call this function: Call GameController.HideMap() instead to hide plate
    public void HideMap()
    {
        currentMap.SetActive(false);
        IsMapShowing = false;
    }
}
