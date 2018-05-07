using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleARCore;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class GameController : Singleton<GameController>
{
    public enum EndGameTypes { NONE, WIN, LOSE }
    public enum ModelTypes { NONE, GIRAFFE, TREE }

    [Serializable]
    public struct Model
    {
        public ModelTypes Type;
        public GameObject Prefab;
    }

    [SerializeField]
    private bool enableAR;
    public bool EnableAR { get { return enableAR; } }

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
    private Model[] modelPrefabs;

    public Transform Platform;
    
    public Dictionary<string, GameObject> Models { get; private set; }
    public bool IsGameRunning { get; private set; }

    public static event Action OnGameStarted;
    public static event Action<EndGameTypes> OnGameEnded;

    private Touch touch;
    private AudioManager audioManager;
    private VisualIndicatorController visualIndicator;
    private GameObject activeModel;
    private Coroutine CR_RotatePlatform;

    private const float PLATFORM_ROTATION_INCREMENT = 1f;

    void Start()
    {
        visualIndicator = VisualIndicatorController.Instance;
        audioManager = AudioManager.Instance;
        Models = new Dictionary<string, GameObject>();        

        // Populate dictionary of model items from inspector
        foreach (var item in modelPrefabs)
        {
            string name = item.Type.ToString();
            GameObject go = item.Prefab;
            Models.Add(name, go);
        }

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
#if UNITY_EDITOR
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            Spawn(ModelTypes.GIRAFFE, true);
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            Spawn(ModelTypes.TREE, true);

        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(0);
#endif

        DragItem();
    }    


    #region DEBUG BUTTON CLICKS
    public void ReSpawn()
    {
        Spawn(ModelTypes.GIRAFFE, true);
    }

    public void Restart()
    {
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
        if (!IsGameRunning)
            return;

        // Mouse input
        if (!enableAR)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Raycast
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.collider.tag == "Draggable")
                    {
                        dragItem = hit.collider.transform;
                        var fruitItem = dragItem.GetComponent<FruitItem>();
                        FruitItemController.Instance.SetGrabbed(fruitItem);

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
                    //var fruitCollider = dragItem.GetComponent<Collider>();

                    //// Offset X
                    //float padding = fruitCollider.bounds.size.x / 2f;
                    //float offset = RoomController.Instance.Room.transform.position.x;
                    //worldPos.x = Mathf.Clamp(worldPos.x, -RoomController.Instance.MaxDraggableBoundaries.x + padding + offset, RoomController.Instance.MaxDraggableBoundaries.x - padding + offset);

                    //// Offset Y
                    //float minHeight = fruitCollider.bounds.size.y / 2f;
                    //padding = fruitCollider.bounds.size.y / 2f;
                    //offset = RoomController.Instance.Room.transform.position.y;
                    //worldPos.y = Mathf.Clamp(worldPos.y, minHeight + padding + offset, RoomController.Instance.MaxDraggableBoundaries.y - padding + offset);

                    //// Offset Z
                    //offset = RoomController.Instance.Room.transform.position.z;
                    //padding = fruitCollider.bounds.size.z / 2f;
                    //worldPos.z = Mathf.Clamp(worldPos.z, -RoomController.Instance.MaxDraggableBoundaries.z + padding + offset, RoomController.Instance.MaxDraggableBoundaries.z - padding + offset);

                    // Drag object
                    dragItem.position = worldPos;                    
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (dragItem)
                {
                    FruitItemController.Instance.SetDropped();
                    dragItem = null;
                }
            }
        }
        // Touch input
        else
        {
            if(Input.touchCount > 0)
            {
                touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    ray = Camera.main.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        if (hit.collider.tag == "Draggable")
                        {
                            dragItem = hit.collider.transform;
                            var fruitItem = dragItem.GetComponent<FruitItem>();
                            FruitItemController.Instance.SetGrabbed(fruitItem);

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
                    {
                        FruitItemController.Instance.SetDropped();
                        dragItem = null;
                    }
                }
            }
        }
    }
#endregion


#region LOAD GAME    
    public void Spawn(ModelTypes _type, bool _newGame)
    {
        SnapController.Instance.ClearSnapColliders();

        // Stop game first before starting a new game
        if (_newGame)
            StopGame(EndGameTypes.NONE);

        HideIndicators();
        SpawnItem(_type);
        RotatePlatform(Explode, 1.5f, _newGame);
    }

    private void SpawnItem(ModelTypes _type)
    {
        RemoveActiveType();

        // Reset platform rotation
        Platform.rotation = Quaternion.identity;

        // Instantiate model
        GameObject go = Instantiate(Models[_type.ToString()], Platform);
        activeModel = go;
        audioManager.PlaySound(audioManager.audio_spawn);
    }

    private void RemoveActiveType()
    {
        if (!activeModel)
            return;

        DestroyImmediate(activeModel);
    }

    private void Explode(bool startGame)
    {
        StartCoroutine(ExplodeCR(startGame));
    }

    private IEnumerator ExplodeCR(bool startGame)
    {
        // Explode
        yield return new WaitForSeconds(1.5f);
        FruitItem[] fruitItems = activeModel.GetComponentsInChildren<FruitItem>(true);
        foreach (var fi in fruitItems)
            fi.Explode();
        audioManager.PlaySound(audioManager.audio_explode);

        // Start game
        if (startGame)
        {
            yield return new WaitForSeconds(1.5f);
            StartGame();
        }
    }
#endregion


#region PLATFORM ROTATION
    private void RotatePlatform(Action<bool> callback = null, float delay = 0f, bool startGame = true)
    {
        if (CR_RotatePlatform != null)
            StopCoroutine(CR_RotatePlatform);

        CR_RotatePlatform = StartCoroutine(RotatePlatformCR(callback, delay, startGame));
    }

    private IEnumerator RotatePlatformCR(Action<bool> callback, float delay, bool startGame)
    {
        yield return new WaitForSeconds(delay);
        audioManager.PlaySpinSound();

        float fElapsed = 0f;
        float fDuration = 1.5f;

        float startAngle = Platform.eulerAngles.y;
        float endAngle = startAngle + 360.0f;

        while (fElapsed < fDuration)
        {
            fElapsed += Time.deltaTime;
            float rotY = Mathf.Lerp(startAngle, endAngle, fElapsed / fDuration) % 360.0f;
            Platform.eulerAngles = new Vector3(Platform.eulerAngles.x, rotY, Platform.eulerAngles.z);
            yield return null;
        }
        CR_RotatePlatform = null;
        audioManager.StopSpinSound();

        // Trigger start game event
        if (callback != null)
            callback(startGame);
    }

    private void StopPlatformRotation()
    {
        if (CR_RotatePlatform != null)
        {
            StopCoroutine(CR_RotatePlatform);
            CR_RotatePlatform = null;
        }
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
        if (!IsGameRunning)
            return;

        IsGameRunning = false;
        if (OnGameEnded != null)
            OnGameEnded(_type);

        StopPlatformRotation();
    }
#endregion


#region EXTRAS
    private void HideIndicators()
    {
        visualIndicator.HideIndicators();
    }
#endregion
}
