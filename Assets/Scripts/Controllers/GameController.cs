using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class GameController : Singleton<GameController>
{
    public enum PremadeTypes { NONE, GIRAFFE, DOG }

    [Serializable]
    public struct Premades
    {
        public PremadeTypes Type;
        public GameObject Prefab;
    }

    [SerializeField]
    private Premades[] PremadeItems;

    public Transform Platform;

    public Dictionary<string, GameObject> Items { get; private set; }
    public bool IsGameRunning { get; private set; }

    public static event Action<bool> OnGameStarted;
    public static event Action<bool> OnGameEnded;

    private VisualIndicatorController visualIndicator;
    private GameObject activeItem;
    private Coroutine CR_RotatePlatform;

    private const float PLATFORM_ROTATION_INCREMENT = 1f;

    void Start()
    {
        visualIndicator = VisualIndicatorController.Instance;
        Items = new Dictionary<string, GameObject>();

        // Populate dictionary of premade items from inspector
        foreach (var item in PremadeItems)
        {
            string name = item.Type.ToString();
            GameObject go = item.Prefab;
            Items.Add(name, go);
        }
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //    Spawn(PremadeTypes.GIRAFFE, true);
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //    Spawn(PremadeTypes.DOG, true);
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //    Spawn(PremadeTypes.GIRAFFE, false);
        //if (Input.GetKeyDown(KeyCode.Alpha4))
        //    Spawn(PremadeTypes.DOG, false);

        //if (Input.GetKeyDown(KeyCode.Escape))
        //    SceneManager.LoadScene(0);

        DragItem();
    }


    #region DRAGGABLE
    private bool isDragging;
    private Transform dragObject;
    private Vector3 screenPos;
    private Vector3 offset;
    private RaycastHit hit;
    private Ray ray;

    void DragItem()
    {
        if (!IsGameRunning)
            return;

#if UNITY_EDITOR
        //if (Input.GetMouseButtonDown(0))
        //{
        //    // Raycast
        //    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    if (Physics.Raycast(ray, out hit, 100f))
        //    {
        //        if (hit.collider.tag == "Draggable")
        //        {
        //            dragObject = hit.collider.transform;
        //            isDragging = true;

        //            // Convert world position to screen position.
        //            screenPos = Camera.main.WorldToScreenPoint(hit.collider.transform.position);
        //            offset = hit.collider.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPos.z));
        //        }
        //    }
        //}
        //if (Input.GetMouseButtonUp(0))
        //{
        //    isDragging = false;
        //    dragObject = null;
        //}

        //if (isDragging)
        //{
        //    // Touch screen position
        //    Vector3 touchPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPos.z);

        //    // Convert screen position to world position with offset changes.
        //    Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPos) + offset;

        //    // Drag object
        //    dragObject.position = worldPos;
        //}
//#else

        if (Input.touchCount < 1)
            return;

        //Touch touch = Input.GetTouch(0);
        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            //ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            ray = Camera.main.ScreenPointToRay(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 0f));
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Draggable")
                {
                    print("********** STARTED!");
                    dragObject = hit.collider.transform;
                    isDragging = true;

                    // Convert world position to screen position.
                    screenPos = Camera.main.WorldToScreenPoint(hit.collider.transform.position);
                    offset = hit.collider.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, screenPos.z));
                }
            }
        }
        if (Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            print("********* ENDED!");
            isDragging = false;
            dragObject = null;
        }

        if (isDragging)
        {
            // Touch screen position
            Vector3 touchPos = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, screenPos.z);

            // Convert screen position to world position with offset changes.
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPos) + offset;

            // Drag object
            dragObject.position = worldPos;
        }        
#endif
    }
    #endregion


    #region LOAD GAME    
    public void Spawn(PremadeTypes _type, bool startGame)
    {
        // Stop game first before starting a new game
        if (startGame)
            StopGame(false);

        HideIndicators();
        SpawnItem(_type);
        RotatePlatform(Explode, 0.25f, startGame);
    }

    private void SpawnItem(PremadeTypes _type)
    {
        RemoveActiveType();

        // Reset platform rotation
        Platform.rotation = Quaternion.identity;

        // Instantiate premade item
        GameObject go = Instantiate(Items[_type.ToString()], Platform);
        activeItem = go;

        //switch (_type)
        //{
        //    case PremadeTypes.GIRAFFE:
        //        //GameObject go = Instantiate(Items[_type.ToString()]);
        //        break;
        //    case PremadeTypes.DOG:
        //        break;
        //    default:
        //        break;
        //}
    }

    private void RemoveActiveType()
    {
        if (!activeItem)
            return;

        DestroyImmediate(activeItem);
    }

    private void Explode(bool startGame)
    {
        StartCoroutine(ExplodeCR(startGame));
    }

    private IEnumerator ExplodeCR(bool startGame)
    {
        // Explode
        yield return new WaitForSeconds(0.25f);
        FruitItem[] fruitItems = activeItem.GetComponentsInChildren<FruitItem>(true);
        foreach (var fi in fruitItems)
            fi.Explode();

        // Start game
        if (startGame)
        {
            yield return new WaitForSeconds(0.25f);
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

        float fElapsed = 0f;
        float fDuration = 0.25f;

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
    public void StartGame(bool showStatusText = true)
    {
        if (IsGameRunning)
            return;

        IsGameRunning = true;
        if (OnGameStarted != null)
            OnGameStarted(showStatusText);
    }

    public void StopGame(bool showStatusText = true)
    {
        if (!IsGameRunning)
            return;

        IsGameRunning = false;
        if (OnGameEnded != null)
            OnGameEnded(showStatusText);

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
