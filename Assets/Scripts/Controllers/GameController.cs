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
    public enum ModelTypes { NONE, BUTTERFLY = 1, GIRAFFE = 100, LIZARD = 200, SPACESHIP = 300 }

    [Serializable]
    public struct Model
    {
        public ModelTypes Type;
        public GameObject Prefab;
    }

    public Transform Platform;

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
    private Material mat_outline;
    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private Model[] modelPrefabs;    
    
    public Dictionary<string, GameObject> Models { get; private set; }
    public bool IsGameRunning { get; private set; }
    public bool EnableAR { get { return enableAR; } }

    public static event Action OnGameStarted;
    public static event Action<EndGameTypes> OnGameEnded;

    private Touch touch;
    private FruitItemController fruitItemController;
    private AudioManager audioManager;
    private GameObject activeModel;
    private Coroutine CR_RotatePlatform;

    private const float PLATFORM_ROTATION_INCREMENT = 1f;

    void Start()
    {
        audioManager = AudioManager.Instance;
        fruitItemController = FruitItemController.Instance;
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
        DragItem();
    }    


    #region DEBUG BUTTON CLICKS
    public void ReSpawn()
    {
        Spawn(ModelTypes.BUTTERFLY, true);
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
                if (Physics.Raycast(ray, out hit, 10f, layerMask))
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
                    if (Physics.Raycast(ray, out hit, 10f, layerMask))
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
    public void Spawn(int index)
    {
        ModelTypes _type = (ModelTypes)index;
        Spawn(_type, true);
    }

    public void Spawn(ModelTypes _type, bool _newGame)
    {
        // Clear old items
        Clear();

        // Stop game first before starting a new game
        if (_newGame)
            StopGame(EndGameTypes.NONE);

        SpawnModel(_type);
        RotatePlatform(Explode, 1.5f, _newGame);
    }    

    private void SpawnModel(ModelTypes _type)
    {
        // Instantiate model
        GameObject go = Instantiate(Models[_type.ToString()], Platform);
        activeModel = go;
        fruitItemController.PopulateFruits(go.transform);

        // Combine mesh to create a clone to show sillouette
        MeshCombiner.Instance.CombineMesh(go.GetComponent<DynamicOutline>(), mat_outline, Platform, false);

        audioManager.PlaySound(audioManager.audio_spawn);
    }

    private void Clear()
    {
        UIController.Instance.HideActivePanel();
        SnapController.Instance.Clear();
        MeshCombiner.Instance.Clear();
        StopExplode();
        RemoveActiveType();
        Platform.rotation = Quaternion.identity;
    }

    private void RemoveActiveType()
    {
        if (!activeModel)
            return;

        DestroyImmediate(activeModel);
    }

    private Coroutine CR_Explode;
    private void Explode(bool startGame)
    {
        StopExplode();
        CR_Explode = StartCoroutine(ExplodeCR(startGame));
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

    private void StopExplode()
    {
        if (CR_Explode != null)
        {
            StopCoroutine(CR_Explode);
            CR_Explode = null;
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
}
