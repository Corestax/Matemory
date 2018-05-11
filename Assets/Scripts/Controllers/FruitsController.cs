using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FruitsController : Singleton<FruitsController>
{
    public enum FruitTypes { APPLE = 1, APPLE_HALF = 20, APPLE_WEDGE = 40, BANANA = 60, BROCCOLI = 80, CARAMBOLA = 100, CARAMBOLA_SLICE = 120, CELERY = 140, CELERY_SHORT = 160, CUCUMBER = 180, CUCUMBER_SLICE = 200, GRAPE = 220, ORANGE = 240, ORANGE_HALF = 260, ORANGE_WEDGE = 280, PEAR = 300, PEAR_HALF = 320, PEAR_WEDGE = 340, PINEAPPLE = 360, STRAWBERRY = 380, WATERMELON = 400, WATERMELON_HALF = 420, WATERMELON_SLICE = 440, CUSTOM1 = 460, CUSTOM2 = 480, CUSTOM3 = 500, CUSTOM4 = 520, CUSTOM5 = 540 }
    [SerializeField]

    private List<FruitItem> Fruits;
    void Start()
    {
        Fruits = new List<FruitItem>();
        SetARCoreLightShader();        
    }

    private void SetARCoreLightShader()
    {
        // Determine if ARCore lighting should be enabled
        Material[] mats = Resources.LoadAll("Materials", typeof(Material)).Cast<Material>().ToArray();
        bool enableARLight = GameController.Instance.EnableAR;
        foreach (var m in mats)
        {
            if (enableARLight)
                m.EnableKeyword("ARCORELIGHT_ON");
            else
                m.DisableKeyword("ARCORELIGHT_ON");
        }

        //foreach (var f in Fruits)
        //{
        //    if (enableARLight)
        //        f.GetComponentInChildren<MeshRenderer>().material.EnableKeyword("ARCORELIGHT_ON");
        //    else
        //        f.GetComponentInChildren<MeshRenderer>().material.DisableKeyword("ARCORELIGHT_ON");
        //}
    }

    public void PopulateFruits(Transform _model)
    {
        Fruits.Clear();
        Fruits = _model.GetComponentsInChildren<FruitItem>().ToList();        
    }

    public void FreezeFruits()
    {
        foreach (var item in Fruits)
        {
            if(!item.IsSnapped)
                item.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public void UnfreezeFruits()
    {
        foreach (var item in Fruits)
        {
            if (!item.IsSnapped)
                item.GetComponent<Rigidbody>().isKinematic = false;
        }
    }   
}
