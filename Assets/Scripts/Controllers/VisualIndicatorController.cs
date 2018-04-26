using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualIndicatorController : Singleton<VisualIndicatorController>
{
    [SerializeField]
    private GameObject Prefab;
    [SerializeField]
    private Transform Platform;

    private Dictionary<FruitItem, GameObject> Indicators;

    void Start ()
    {
        Indicators = new Dictionary<FruitItem, GameObject>();	
	}
	
    public void ShowIndicator(FruitItem item, Vector3 pos)
    {
        var go = Instantiate(Prefab, pos, Prefab.transform.rotation, Platform);

        // Raise indicator by 0.5f;
        var localPos = go.transform.localPosition;
        localPos.y = go.transform.localScale.x / 2f;
        go.transform.localPosition = localPos;

        Indicators.Add(item, go);
    }

    public void HideIndicator(FruitItem item)
    {
        if (!Indicators.ContainsKey(item))
            return;

        Destroy(Indicators[item]);
        Indicators.Remove(item);
    }

    public void HideIndicators()
    {
        foreach (var go in Indicators.Values)
            Destroy(go);
        Indicators.Clear();
    }
}
