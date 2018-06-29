using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Platform : MonoBehaviour
{
    private bool isRotate;
    private bool isClockwise;
    private GameController gameController;
    private FruitsController fruitsController;
    private AudioManager audioManager;
    private Coroutine CR_RotatePlatform;

    public Material Material { get; private set; }

    private Quaternion startRotation;

    private const float SPEED = 100f;

    void Start()
    {
        gameController = GameController.Instance;
        fruitsController = FruitsController.Instance;
        audioManager = AudioManager.Instance;

        startRotation = transform.localRotation;

        Material = GetComponentInChildren<MeshRenderer>().material;
        Material.CopyPropertiesFromMaterial(new Material(Material));
    }

    void Update()
    {
        if (!gameController.IsGameRunning || gameController.IsGamePaused || !isRotate)
            return;

        if (isClockwise)
            transform.Rotate(Vector3.up * Time.deltaTime * SPEED, Space.World);
        else
            transform.Rotate(-Vector3.up, Time.deltaTime * SPEED, Space.World);        
    }

    private void OnEnable()
    {
        GameController.OnGameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        GameController.OnGameEnded -= OnGameEnded;

        if (CR_RotatePlatform != null)
        {
            ResetRotation();
            audioManager.StopSpinSound();

            StopCoroutine(CR_RotatePlatform);
            CR_RotatePlatform = null;
        }
    }

    private void OnGameEnded(GameController.EndGameTypes _type)
    {
        StopPlatformRotation();
    }

    public void Rotate(bool clockwise)
    {
        fruitsController.FreezeFruits();
        audioManager.PlaySpinSound();
        isRotate = true;
        isClockwise = clockwise;
    }

    public void StopRotate()
    {
        fruitsController.UnfreezeFruits();
        audioManager.StopSpinSound();
        isRotate = false;
    }

    public void ResetRotation()
    {
        transform.localRotation = startRotation;
    }

    #region PLATFORM ROTATION
    public void RotatePlatform(Action<bool> callback = null, float delay = 0f, bool startGame = true)
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

        float startAngle = transform.eulerAngles.y;
        float endAngle = startAngle + 360.0f;

        while (fElapsed < fDuration)
        {
            fElapsed += Time.deltaTime;
            float rotY = Mathf.Lerp(startAngle, endAngle, fElapsed / fDuration) % 360.0f;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotY, transform.eulerAngles.z);
            yield return null;
        }
        CR_RotatePlatform = null;
        audioManager.StopSpinSound();

        // Trigger start game event
        if (callback != null)
            callback(startGame);
    }

    public void StopPlatformRotation()
    {
        if (CR_RotatePlatform != null)
        {
            StopCoroutine(CR_RotatePlatform);
            CR_RotatePlatform = null;
        }
    }
    #endregion
}
