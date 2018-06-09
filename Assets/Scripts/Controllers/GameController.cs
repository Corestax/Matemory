using System;
using UnityEngine;
using GoogleARCore;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class GameController : Singleton<GameController>
{
    public enum EndGameTypes { NONE, WIN, LOSE }

    [SerializeField]
    private bool enableAR;
    [SerializeField]
    private ARCoreController arCoreController;
    [SerializeField]
    private ARCoreSession arCoreSession;
    [SerializeField]
    private EnvironmentalLight arCoreEnvironmentalLight;
    [SerializeField]
    private Camera camEditor;
    [SerializeField]
    private Camera camAR;
    [SerializeField]
    private PinchZoom pinchZoom;
    [SerializeField]
    private LayerMask layer_fruits;
    [SerializeField]
    private GameObject Plate;

    public bool IsGamePaused { get; private set; }
    public bool IsGameRunning { get; private set; }
    public bool EnableAR { get { return enableAR; } }

    public static event Action OnGameStarted;
    public static event Action<EndGameTypes> OnGameEnded;
    public static event Action OnGamePaused;
    public static event Action OnGameUnpaused;

    private Touch touch;
    private UIController uiController;
    private MapsController mapsController;
    private FruitRotatorController fruitRotatorController;
    private TutorialsController tutorialController;

    void Start()
    {
        uiController = UIController.Instance;
        mapsController = MapsController.Instance;
        fruitRotatorController = FruitRotatorController.Instance;
        tutorialController = TutorialsController.Instance;

        // Force AR on builds
#if !UNITY_EDITOR && UNITY_ANDROID
        enableAR = true;
#endif

        if (enableAR)
        {
            arCoreController.gameObject.SetActive(true);
            arCoreSession.gameObject.SetActive(true);
            arCoreEnvironmentalLight.gameObject.SetActive(true);
            camAR.gameObject.SetActive(true);
            camEditor.gameObject.SetActive(false);
            pinchZoom.MainCamera = camAR;
        }
        else
        {
            arCoreController.gameObject.SetActive(false);
            arCoreSession.gameObject.SetActive(false);
            arCoreEnvironmentalLight.gameObject.SetActive(false);
            camAR.gameObject.SetActive(false);
            camEditor.gameObject.SetActive(true);
            pinchZoom.MainCamera = camEditor;
        }

        Screen.orientation = ScreenOrientation.Landscape;
    }

    void Update()
    {
        DragItem();
    }

    #region SHOW/HIDE MAP
    public void ShowMap(bool _animateCharacter, bool _showCloseMapButton)
    {
        Plate.SetActive(false);

        // Animate character
        if (_animateCharacter)
            mapsController.ShowMapAndAnimateCharacter(2.0f);
        else
            mapsController.ShowMap();

        // Show map button
        if (_showCloseMapButton)
            uiController.ShowCloseMapButton();
        else
            uiController.HideCloseMapButton();

        PauseGame();
    }

    public void HideMap()
    {
        mapsController.HideMap();
        uiController.HideCloseMapButton();
        Plate.SetActive(true);

        UnpauseGame();
    }
    #endregion


    #region DEBUG BUTTON CLICKS
    public void Restart()
    {
        // Delete playerprefs all just for Demo purposes
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(0);
    }
    #endregion


    #region DRAGGABLE    
    private Vector3 screenPos;
    private Vector3 vOffset;
    private RaycastHit hit;
    private Ray ray;
    private Transform dragItem;

    void DragItem()
    {
        if (!IsGameRunning || tutorialController.IsActive)
            return;

        // Mouse input
        if (!enableAR)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Raycast
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 10f, layer_fruits))
                {
                    if (hit.collider.tag == "Draggable")
                    {
                        dragItem = hit.collider.transform;
                        var fruitItem = dragItem.GetComponent<FruitItem>();
                        fruitRotatorController.SetGrabbed(fruitItem);

                        // Convert world position to screen position.
                        screenPos = Camera.main.WorldToScreenPoint(dragItem.position);
                        vOffset = dragItem.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPos.z));
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (dragItem)
                {
                    // Touch screen position
                    Vector3 touchPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPos.z);

                    // Convert screen position to world position with offset changes.
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPos) + vOffset;

                    // Drag object
                    dragItem.position = worldPos;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (dragItem)
                    ClearDrag();
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
                    if (Physics.Raycast(ray, out hit, 10f, layer_fruits))
                    {
                        if (hit.collider.tag == "Draggable")
                        {
                            dragItem = hit.collider.transform;
                            var fruitItem = dragItem.GetComponent<FruitItem>();
                            fruitRotatorController.SetGrabbed(fruitItem);

                            // Convert world position to screen position.
                            screenPos = Camera.main.WorldToScreenPoint(dragItem.position);
                            vOffset = dragItem.position - Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, screenPos.z));
                        }
                    }
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    if (dragItem)
                    {
                        // Touch screen position
                        Vector3 touchPos = new Vector3(touch.position.x, touch.position.y, screenPos.z);

                        // Convert screen position to world position with offset changes.
                        Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPos) + vOffset;

                        // Drag object
                        dragItem.position = worldPos;
                    }
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    if (dragItem)
                        ClearDrag();
                }
            }
        }
    }
    #endregion


    #region RESET     
    private void ClearDrag()
    {
        fruitRotatorController.SetDropped();
        dragItem = null;
    }
    #endregion


    #region START/STOP GAME
    public void StartGame()
    {
        if (IsGameRunning)
            return;

        IsGameRunning = true;
        if (OnGameStarted != null)
            OnGameStarted();
    }

    public void StopGame(EndGameTypes _type)
    {
        IsGameRunning = false;
        ClearDrag();

        if (OnGameEnded != null)
            OnGameEnded(_type);
    }
    #endregion


    #region PAUSE/UNPAUSE GAME
    public void PauseGame()
    {
        if (!IsGameRunning)
            return;

        IsGamePaused = true;
        if (OnGamePaused != null)
            OnGamePaused();
    }

    public void UnpauseGame()
    {
        if (!IsGameRunning)
            return;

        IsGamePaused = false;
        if (OnGameUnpaused != null)
            OnGameUnpaused();
    }
    #endregion
}
