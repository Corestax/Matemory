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
    private GameObject directionalLight;
    [SerializeField]
    private PinchZoom pinchZoom;    
    [SerializeField]
    private LayerMask layer_fruits;

    public bool IsGameRunning { get; private set; }
    public bool EnableAR { get { return enableAR; } }

    public static event Action OnGameStarted;
    public static event Action<EndGameTypes> OnGameEnded;

    private Touch touch;
    private RoomController roomController;
    private FruitRotatorController fruitRotatorController;

    void Start()
    {
        roomController = RoomController.Instance;
        fruitRotatorController = FruitRotatorController.Instance;

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
            directionalLight.SetActive(false);
            camEditor.gameObject.SetActive(false);
            pinchZoom.MainCamera = camAR;
        }
        else
        {
            arCoreController.gameObject.SetActive(false);
            arCoreSession.gameObject.SetActive(false);
            arCoreEnvironmentalLight.gameObject.SetActive(false);
            camAR.gameObject.SetActive(false);
            directionalLight.SetActive(true);
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

        //if (!IsGameRunning || EventSystem.current.IsPointerOverGameObject())
        //    return;

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
            if(Input.touchCount > 0)
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
    public void Clear()
    {
        UIController.Instance.HideActivePanel();
        SnapController.Instance.Clear();
        MeshCombiner.Instance.Clear();
        roomController.Recenter();
        ClearDrag();
    }

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
        if (!IsGameRunning)
            return;

        IsGameRunning = false;
        if (OnGameEnded != null)
            OnGameEnded(_type);
    }
#endregion
}
