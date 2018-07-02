using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Dreamteck.Splines;

public class MapsController : Singleton<MapsController>
{
    [SerializeField]
    private GameObject[] Maps;

    private LevelsController levelsController;
    private UIController uiController;
    private ScoreController scoreController;
    private AudioManager audioManager;
    private SplineFollower follower;
    private Character character;
    private List<TargetPoint> targetPoints;
    private Material[] material;
    private GameObject currentMap;
    private RaycastHit hit;
    private Ray ray;
    private Touch touch;

    public bool IsMapShowing { get; private set; }
    public int MapIndex { get; private set; }

    void Start()
    {
        // Default current map to 0
        // NOTE: This will need to change in the future, when loading a saved map
        SetMap(MapIndex);

        levelsController = LevelsController.Instance;
        uiController = UIController.Instance;
        scoreController = ScoreController.Instance;
        audioManager = AudioManager.Instance;
        follower = currentMap.GetComponentInChildren<SplineFollower>();
        character = follower.GetComponent<Character>();

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

        // UI panels
        if (uiController.ActivePanel != UIController.PanelTypes.NONE)
            return;

        OnLevelSelected();
    }

    private void OnLevelSelected()
    {
        // Mouse input
        if (!GameController.Instance.EnableAR)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                    SelectLevel();
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
                        SelectLevel();
                }
            }
        }
    }

    private void SelectLevel()
    {
        if (hit.collider.tag == "TargetPointPlatform")
        {
            // Replay previous level
            TargetPoint targetPoint = hit.collider.GetComponentInParent<TargetPoint>();
            int selectedLevel = targetPoint.Level;
            if (selectedLevel <= levelsController.HighestLevel)
            {
                HighlightTargetPoint(selectedLevel);
                SetCharacterLevel(selectedLevel);
                uiController.ShowPanel(UIController.PanelTypes.PLAY_LEVEL);

                //// Move character to selected level
                //if (selectedLevel != levelsController.CurrentLevel)
                //{
                //    HighlightTargetPoint(selectedLevel);
                //    SetCharacterLevel(selectedLevel);
                //    uiController.ShowPanel(UIController.PanelTypes.PLAY_LEVEL);
                //}
                //// If character already on selected level: Show UI
                //else
                //{
                //    uiController.ShowPanel(UIController.PanelTypes.PLAY_LEVEL);
                //}
                audioManager.PlaySound(audioManager.audio_selectLevel);
            }
        }
    }    

    public void SetCharacterLevel(int level)
    {
        character.SetPosition(level);
        HighlightTargetPoint(level);
        scoreController.LoadHighScore(level);
    }

    public int GetCharacterLevel()
    {
        return character.CurrentLevel;
    }

    private void LoadTargetPoints()
    {
        targetPoints = new List<TargetPoint>();
        targetPoints = currentMap.GetComponentsInChildren<TargetPoint>().ToList();
    }

    public void HighlightTargetPoint(int level)
    {
        foreach (var tp in targetPoints)
        {
            if (tp.Level == level)
                tp.SetSelected();
            else if (tp.Level <= levelsController.HighestLevel)
                tp.SetUnselected();
        }
    }

    public void SetMap(int _mapIndex)
    {
        MapIndex = _mapIndex;
        currentMap = Maps[MapIndex];
        follower = currentMap.GetComponentInChildren<SplineFollower>();
        LoadTargetPoints();
    }

    // Do not call this function: Call GameController.ShowMap() instead to hide plate
    public void ShowMap()
    {
        currentMap.SetActive(true);
        IsMapShowing = true;

        // Fix bug for initial map rotation
        character.Play(true);
        character.Stop();
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
    public void HideMap(bool discardCharacterLevelChanges)
    {
        // Move back to current level
        if (discardCharacterLevelChanges)
            SetCharacterLevel(levelsController.CurrentLevel);
        
        currentMap.SetActive(false);
        IsMapShowing = false;
    }
}
