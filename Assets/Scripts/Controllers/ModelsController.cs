using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelsController : Singleton<ModelsController>
{
    public enum ModelTypes { NONE, ANT = 100, ARMADILLO = 200, BEETLE = 300, BIRD = 400, BUTTERFLY = 500, CAT = 600, CATERPILLAR = 700, CRAB = 800, ELEPHANT = 900,  GIRAFFE = 1000, LIZARD = 1100, RABBIT = 1200, SEMI_TRUCK = 1300, SNOWMAN = 1400, SPACESHIP = 1500, SQUID = 1600, TEDDYBEAR = 1700, TURTLE = 1800, WARBOAT = 1900, WINDMILL = 2000 }

    [Serializable]
    public struct Model
    {
        public ModelTypes Type;
        public GameObject Prefab;
    }

    [SerializeField]
    private Model[] modelPrefabs;
    [SerializeField]
    private Platform platform;
    [SerializeField]
    private Material mat_meshCombinedOutline;

    private FruitsController fruitsController;
    private UIController uiController;
    private AudioManager audioManager;
    private Coroutine CR_Explode;

    public Dictionary<string, GameObject> Models { get; private set; }
    public ModelTypes ActiveModelType { get; private set; }
    public GameObject ActiveModel { get; private set; }

    private float time_rotatePlatform;
    private float time_beforeExplosion;
    private float time_afterExplosion;
    //private float timeToMemorize;

    void Start()
    {
        fruitsController = FruitsController.Instance;
        uiController = UIController.Instance;
        audioManager = AudioManager.Instance;

        // Define times
        time_rotatePlatform = 3.5f;
        time_beforeExplosion = 1.5f;
        time_afterExplosion = 1.5f;
        //timeToMemorize = time_beforeExplosion + time_rotatePlatform;

        // Populate dictionary of model items from inspector
        Models = new Dictionary<string, GameObject>();
        foreach (var item in modelPrefabs)
        {
            string name = item.Type.ToString();
            GameObject go = item.Prefab;
            Models.Add(name, go);
        }
    }

    private void OnEnable()
    {
        GameController.OnGameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        GameController.OnGameEnded -= OnGameEnded;
    }

    private void OnGameEnded(GameController.EndGameTypes _type)
    {
        Clear();
    }

    public void Spawn(ModelTypes _type)
    {
        TutorialsController.Instance.ShowTutorial(1);
        //uiController.ShowStatusText("You have " + timeToMemorize + " seconds to memorize the pieces!", uiController.Color_statusText, timeToMemorize);
        //platform.RotatePlatform(Explode, time_rotatePlatform, _newGame);

        // Instantiate model
        GameObject go = Instantiate(Models[_type.ToString()], platform.transform);
        ActiveModel = go;
        ActiveModelType = _type;
        fruitsController.PopulateFruits(go.transform);

        // Combine mesh to create a clone to show sillouette
        MeshCombiner.Instance.CombineMesh(go.GetComponent<DynamicOutline>(), mat_meshCombinedOutline, platform.transform, false);

        audioManager.PlaySound(audioManager.audio_spawn);
    }

    public void RotatePlatformAndExplode()
    {
        platform.RotatePlatform(Explode, time_rotatePlatform, false);
        uiController.ShowCountdown();
    }

    private void Clear()
    {
        StopExplode();
        RemoveActiveType();
    }

    private void RemoveActiveType()
    {
        if (!ActiveModel)
            return;

        DestroyImmediate(ActiveModel);
        ActiveModelType = ModelTypes.NONE;
    }

    private void Explode(bool startGame)
    {
        StopExplode();
        CR_Explode = StartCoroutine(ExplodeCR(startGame));
    }

    private IEnumerator ExplodeCR(bool startGame)
    {
        // Explode
        yield return new WaitForSeconds(time_beforeExplosion);
        FruitItem[] fruitItems = ActiveModel.GetComponentsInChildren<FruitItem>(true);
        foreach (var fi in fruitItems)
            fi.Explode();
        audioManager.PlaySound(audioManager.audio_explode);

        // Start game
        if (startGame)
        {
            yield return new WaitForSeconds(time_afterExplosion);
            GameController.Instance.StartGame();
        }
        else
        {
            TutorialsController.Instance.ShowTutorial(2);
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
}
