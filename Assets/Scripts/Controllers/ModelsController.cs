using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelsController : Singleton<ModelsController>
{
    public enum ModelTypes { NONE, BUTTERFLY = 1, GIRAFFE = 100, LIZARD = 200, SPACESHIP = 300 }

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

    private RoomController roomController;
    private FruitsController fruitsController;
    private AudioManager audioManager;
    private GameObject activeModel;

    public Dictionary<string, GameObject> Models { get; private set; }

    void Start()
    {
        roomController = RoomController.Instance;
        fruitsController = FruitsController.Instance;
        audioManager = AudioManager.Instance;

        Models = new Dictionary<string, GameObject>();

        // Populate dictionary of model items from inspector
        foreach (var item in modelPrefabs)
        {
            string name = item.Type.ToString();
            GameObject go = item.Prefab;
            Models.Add(name, go);
        }
    }

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
            GameController.Instance.StopGame(GameController.EndGameTypes.NONE);

        SpawnModel(_type);
        platform.RotatePlatform(Explode, 1.5f, _newGame);
    }

    private void SpawnModel(ModelTypes _type)
    {
        // Instantiate model
        GameObject go = Instantiate(Models[_type.ToString()], platform.transform);
        activeModel = go;
        fruitsController.PopulateFruits(go.transform);

        // Combine mesh to create a clone to show sillouette
        MeshCombiner.Instance.CombineMesh(go.GetComponent<DynamicOutline>(), mat_meshCombinedOutline, platform.transform, false);

        audioManager.PlaySound(audioManager.audio_spawn);
    }

    private void Clear()
    {
        GameController.Instance.Clear();

        StopExplode();
        RemoveActiveType();
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
            GameController.Instance.StartGame();
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
