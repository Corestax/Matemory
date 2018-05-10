using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FruitsController : Singleton<FruitsController>
{
    private List<FruitItem> Fruits;

    void Start()
    {
        Fruits = new List<FruitItem>();
    }

    public void PopulateFruits(Transform _model)
    {
        Fruits.Clear();
        Fruits = _model.GetComponentsInChildren<FruitItem>().ToList();
    }

    public void FreezeFruits()
    {
        print(Fruits.Count);
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
