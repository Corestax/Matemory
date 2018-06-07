using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FruitsController : Singleton<FruitsController>
{
    public enum FruitTypes { NONE = 0, APPLE = 1, APPLE_HALF = 20, APPLE_WEDGE = 40, BANANA = 60, BROCCOLI = 80, CARAMBOLA = 100, CARAMBOLA_SLICE = 120, CELERY = 140, CELERY_SHORT = 160, CUCUMBER = 180, CUCUMBER_SLICE = 200, GRAPE = 220, ORANGE = 240, ORANGE_HALF = 260, ORANGE_WEDGE = 280, PEAR = 300, PEAR_HALF = 320, PEAR_WEDGE = 340, PINEAPPLE = 360, PINEAPPLE_SLICE = 365, STRAWBERRY = 380, STRAWBERRY_HALF = 385, WATERMELON = 400, WATERMELON_HALF = 420, WATERMELON_SLICE = 440, MUSKMELON = 460, MUSKMELON_HALF = 480, CARROT = 500, CARROT_SLICE = 520, CUSTOM5 = 540 }

    public List<FruitItem> ActiveFruits { get; private set; }

    void Start()
    {
        ActiveFruits = new List<FruitItem>();
    }

    public void PopulateFruits(Transform _model)
    {
        ActiveFruits.Clear();
        ActiveFruits = _model.GetComponentsInChildren<FruitItem>().ToList();
    }

    public void FreezeFruits()
    {
        foreach (var item in ActiveFruits)
        {
            if(!item.IsSnapped)
                item.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public void UnfreezeFruits()
    {
        foreach (var item in ActiveFruits)
        {
            if (!item.IsSnapped)
                item.GetComponent<Rigidbody>().isKinematic = false;
        }
    }   
}
